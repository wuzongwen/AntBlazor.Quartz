using Blazor.Quartz.Core.Service.Admin;
using Blazor.Quartz.Core.Service.Admin.Dto;
using Blazor.Quartz.Web.Services;
using Blazor.Quartz.Core.Service.Base.Dto;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Blazor.Quartz.Web.Controllers
{
    /// <summary>
    /// 管理员登录
    /// </summary>
    [Route("api/[controller]/[Action]")]
    [ApiController]
    [EnableCors("AllowSameDomain")] //允许跨域 
    public class LoginController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly ICaptchaService _captchaService;

        public LoginController(IAdminService adminService, ICaptchaService captchaService)
        {
            _adminService = adminService;
            _captchaService = captchaService;
        }

        /// <summary>
        /// 登录（校验管理员表中的账号密码，成功后写入Cookie）
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> In([FromBody] LoginDto model)
        {
            //先校验图形验证码（一次性消费：无论对错都作废，防止重放枚举；前端收到失败提示后自动换新图）
            if (!_captchaService.Validate(model?.CAPTCHA_ID, model?.CAPTCHA_CODE))
            {
                return Json(new BaseResult { Code = -1, Msg = "验证码错误或已过期" });
            }

            var loginResult = await _adminService.Login(model);
            if (loginResult.Code != 200)
            {
                return Json(new BaseResult { Code = loginResult.Code, Msg = loginResult.Msg });
            }

            var admin = loginResult.Data;
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, admin.USERNAME),
                new Claim(ClaimTypes.NameIdentifier, admin.ID.ToString()),
                new Claim("RealName", string.IsNullOrEmpty(admin.REAL_NAME) ? admin.USERNAME : admin.REAL_NAME)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties()
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8),
                    AllowRefresh = true
                });

            return Json(new BaseResult { Msg = "登录成功" });
        }

        /// <summary>
        /// 退出登录（清除Cookie）
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Out()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Json(new BaseResult { Msg = "退出成功" });
        }
    }
}
