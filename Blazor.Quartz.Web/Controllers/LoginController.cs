using Blazor.Quartz.Core.Const;
using Blazor.Quartz.Core.Dapper;
using Blazor.Quartz.Core.Service.App.Dto;
using Blazor.Quartz.Core.Service.Base.Dto;
using Dapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System;
using System.Threading.Tasks;

namespace Blazor.Quartz.Web.Controllers
{
    /// <summary>
    /// 登录
    /// </summary>
    [Route("[controller]/[Action]")]
    [ApiController]
    public class LoginController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public LoginController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<BaseResult<bool>> UserLogin(LoginReq req)
        {
            BaseResult<bool> result = new BaseResult<bool>();
            try
            {
                var dynamicParams = new DynamicParameters();
                dynamicParams.Add("UserName", req.Account);
                dynamicParams.Add("Password", req.Password);
                var User = await DbContext.QueryFirstOrDefaultAsync<ManageUser>($"SELECT UserName,Password,IsEnable,LastLoginTime FROM {QuartzConstant.TablePrefix}USER WHERE IsEnable=1 AND UserName=@UserName AND Password=@Password", dynamicParams);
                if (User == null)
                {
                    result.Code = -1;
                    result.Msg = "登录失败";
                    return result;
                }
                if (User.IsEnable != 1)
                {
                    result.Code = -1;
                    result.Msg = "用户状态异常";
                    return result;
                }
                #region 登录逻辑
                // 设置身份验证的 Cookie，可以使用其他方式保存用户信息
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, User.UserName),
                    new Claim(ClaimTypes.Role, "User"), // 添加用户角色等信息
                };

                var claimsIdentity = new ClaimsIdentity(claims, "login");
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                await _httpContextAccessor.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal,
                     new AuthenticationProperties
                     {
                         ExpiresUtc = DateTime.UtcNow.AddMinutes(30), // 30 分钟后过期
                         IsPersistent = true,
                         AllowRefresh = false,
                     });
                #endregion
                result.Msg = "登录成功";
                return result;
            }
            catch
            {
                result.Code = -1;
                result.Msg = "登录失败";
                return result;
            }
        }
    }
}
