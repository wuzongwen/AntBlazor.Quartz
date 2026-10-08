using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Blazor.Quartz.Core.Hubs;

namespace Blazor.Quartz.Web.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class NotifyController : Controller
    {
        //private readonly IHubCallerClients<NotifyHub> _hub;
        private readonly IHubContext<NotifyHub> _hub;

        public NotifyController(IHubContext<NotifyHub> hub)
        {
            _hub = hub;
        }


        /// <summary>
        /// 测试通知
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> TestNotify(string name, string message)
        
        {
            //_hub.Clients.a
            //await hub.Broadcast("123", "346");
            //var claimsIdentity = Request.HttpContext.User.Identity as ClaimsIdentity;
            //if (claimsIdentity != null)
            //{
            //    // the principal identity is a claims identity.
            //    // now we need to find the NameIdentifier claim
            //    var userIdClaim = claimsIdentity.Claims
            //        .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

            //    if (userIdClaim != null)
            //    {
            //        var userIdValue = userIdClaim.Value;
            //        await _hub.Clients.User(userIdValue).SendAsync("Broadcast", name, message);
            //        return Content("通知成功");
            //    }
            //    return Content("通知失败");
            //}
            //var user = HttpContext.User;
            //await _hub.Clients.Client(name).SendAsync("Broadcast", name, message);
            await _hub.Clients.All.SendAsync("Broadcast", name, message);
            
            return Content("通知成功");
        }
    }
}
