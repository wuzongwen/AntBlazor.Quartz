using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Blazor.Quartz.Core.Service.Timer.Dto
{
    public abstract class LogModel
    {
        /// <summary>
        /// 开始执行时间
        /// </summary>
        [JsonIgnore]
        public string BeginTime { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        [JsonIgnore]
        public string EndTime { get; set; }
        /// <summary>
        /// 耗时（秒）
        /// </summary>
        [JsonIgnore]
        public string ExecuteTime { get; set; }
        /// <summary>
        /// 任务名称
        /// </summary>
        public string JobName { get; set; }
        /// <summary>
        /// 结果
        /// </summary>
        public string Result { get; set; }
        /// <summary>
        /// 异常消息
        /// </summary>
        public string ErrorMsg { get; set; }
    }
}
