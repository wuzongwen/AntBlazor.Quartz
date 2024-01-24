using Google.Protobuf.WellKnownTypes;
using Quartz;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazor.Quartz.Core.Service.Timer.Dto
{
    public class JobBriefInfoEntity
    {
        /// <summary>
        /// 任务组名
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// 任务信息
        /// </summary>
        public List<JobBriefInfo> JobInfoList { get; set; } = new List<JobBriefInfo>();
    }

    public class JobBriefInfo
    {
        /// <summary>
        /// 任务名称
        /// </summary>
        [DisplayName("任务名称")]
        public string Name { get; set; }

        /// <summary>
        /// 任务组名
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// 下次执行时间(时间戳)
        /// </summary>
        [DisplayName("下次执行时间")]
        public long? Next_Fire_Time { get; set; }

        /// <summary>
        /// 下次执行时间
        /// </summary>
        [DisplayName("下次执行时间")]
        public DateTime? NextFireTime {
            get
            {
                if (Next_Fire_Time != null)
                {
                    return new DateTime(Next_Fire_Time.Value, DateTimeKind.Utc).ToLocalTime();
                }
                return null;
            }
            set
            {

            }
        }

        /// <summary>
        /// 上次执行时间(时间戳)
        /// </summary>
        [DisplayName("上次执行时间")]
        public long? Prev_Fire_Time { get; set; }

        /// <summary>
        /// 上次执行时间
        /// </summary>
        [DisplayName("上次执行时间")]
        public DateTime? PreviousFireTime {
            get
            {
                if (Prev_Fire_Time != null)
                {
                    return new DateTime(Prev_Fire_Time.Value, DateTimeKind.Utc).ToLocalTime();
                }
                return null;
            }
            set 
            {
            
            }
        }

        /// <summary>
        /// 上次执行的异常信息
        /// </summary>
        public string LastErrMsg { get; set; }

        /// <summary>
        /// 任务状态
        /// </summary>
        public TriggerState TriggerState { get; set; }

        /// <summary>
        /// 任务状态
        /// </summary>
        public string Trigger_State { get; set; }

        /// <summary>
        /// 显示状态
        /// </summary>
        [DisplayName("状态")]
        public string DisplayState
        {
            get
            {
                var state = string.Empty;
                switch (Trigger_State)
                {
                    case "NORMAL":
                    case "WAITING":
                    case "ACQUIRED":
                    case "EXECUTING":
                        state = "正常";
                        break;
                    case "PAUSED":
                        state = "暂停";
                        break;
                    case "PAUSED_BLOCKED":
                        state = "暂停中";
                        break;
                    case "COMPLETE":
                        state = "完成";
                        break;
                    case "ERROR":
                        state = "异常";
                        break;
                    case "BLOCKED":
                        state = "阻塞";
                        break;
                    case "NONE":
                        state = "不存在";
                        break;
                    default:
                        state = "未知";
                        break;
                }
                return state;
            }
            set { }
        }

        /// <summary>
        /// 已经执行次数
        /// </summary>
        [DisplayName("已经执行次数")]
        public long TotalExecutions { get; set; }

        /// <summary>
        /// 异常次数
        /// </summary>
        [DisplayName("异常次数")]
        public long ErrorExecutions { get; set; }
    }
}
