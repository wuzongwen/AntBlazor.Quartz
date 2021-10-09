using Blazor.Quartz.Core.Service.App.Enum;
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

        /// <summary>
        /// 请求地址
        /// </summary>
        public string Req_Url { get; set; }

        /// <summary>
        /// 请求类型
        /// </summary>
        public string Req_Type { get; set; }

        /// <summary>
        /// 请求头
        /// </summary>
        public string Headers { get; set; }

        /// <summary>
        /// 超时时长(秒)
        /// </summary>
        public int TimeOut { get; set; }

        /// <summary>
        /// 响应数据
        /// </summary>
        public string Res_Data { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public ExecutionStatusEnum Status { get; set; }
    }
}
