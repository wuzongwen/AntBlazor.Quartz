using Blazor.Quartz.Common;
using Blazor.Quartz.Core.Service.App;
using Blazor.Quartz.Core.Service.App.Dto;
using Blazor.Quartz.Core.Service.Base.Dto;
using Dapper;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blazor.Quartz.Web.Controllers
{
    /// <summary>
    /// 任务调度
    /// </summary>
    [Route("api/[controller]/[Action]")]
    [EnableCors("AllowSameDomain")] //允许跨域 
    public class AppController : Controller
    {
        private readonly IAppService _appService;

        public AppController(IAppService appService)
        {
            _appService = appService;
        }

        /// <summary>
        /// 获取应用列表
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> GetList()
        {
            var model = new QueryModel()
            {
                JOB_GROUP_NAME = "测试"
            };
            return new JsonResult(await _appService.GetList(model));
        }

        /// <summary>
        /// 添加应用
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Add()
        {
            using (var connection = new SqlConnection(AppConfig.ConnectionString))
            {
                var res = await connection.ExecuteAsync(@"INSERT INTO QRTZ_JOB_GROUP ([JOB_GROUP_NAME]
                            ,[DESCRIPTION]
                            ,[IS_ENABLE]) VALUES(@JOB_GROUP_NAME,@DESCRIPTION,@IS_ENABLE)", new { JOB_GROUP_NAME = "测试任务", DESCRIPTION = "没有描述", IS_ENABLE = 1 });
                if (res > 0) 
                {
                    return Content("添加成功");
                }
                return Content("添加失败");
            }
        }
    }
}
