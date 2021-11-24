using Blazor.Quartz.Core.Service.Base.Vaildation;
using Blazor.Quartz.Core.Service.Timer.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazor.Quartz.Core.Service.Timer.Dto
{
    [CustomValidation(typeof(ModelVaildation), "ScheduleEntity")]
    public class ScheduleEntity
    {
        /// <summary>
        /// 任务名称
        /// </summary>
        [Required(ErrorMessage = "任务名称必须填写")]
        public string JobName { get; set; }
        /// <summary>
        /// 任务分组
        /// </summary>
        [Required(ErrorMessage = "任务分组必须填写")]
        public string JobGroup { get; set; }
        /// <summary>
        /// 任务类型
        /// </summary>
        public JobTypeEnum JobType { get; set; } = JobTypeEnum.Url;
        /// <summary>
        /// 开始时间
        /// </summary>
        [Required]
        public DateTime BeginTime { get; set; } = DateTime.Now;
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }
        /// <summary>
        /// Cron表达式
        /// </summary>
        public string Cron { get; set; }
        /// <summary>
        /// 执行次数（默认无限循环）
        /// </summary>
        public int? RunTimes { get; set; }
        /// <summary>
        /// 执行间隔时间，单位秒（如果有Cron，则IntervalSecond失效）
        /// </summary>
        public int? IntervalSecond { get; set; }
        /// <summary>
        /// 触发器类型
        /// </summary>
        [Required(ErrorMessage = "触发器类型必须选择")]
        public TriggerTypeEnum TriggerType { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 约定返回模型
        /// </summary>
        public bool CovenantReturnModel { get; set; }

        #region Url
        /// <summary>
        /// 请求url
        /// </summary>
        [Required(ErrorMessage = "请求url必须填写")]
        public string RequestUrl { get; set; }
        /// <summary>
        /// 请求参数（Post，Put请求用）
        /// </summary>
        public string RequestParameters { get; set; }
        /// <summary>
        /// 超时时间(单位秒)
        /// </summary>
        public int? TimeOut { get; set; }
        /// <summary>
        /// Headers(可以包含如：Authorization授权认证)
        /// 格式：{"Authorization":"userpassword.."}
        /// </summary>
        public string Headers { get; set; }
        /// <summary>
        /// 请求类型
        /// </summary>
        public RequestTypeEnum RequestType { get; set; } = RequestTypeEnum.Post;
        #endregion
    }
}
