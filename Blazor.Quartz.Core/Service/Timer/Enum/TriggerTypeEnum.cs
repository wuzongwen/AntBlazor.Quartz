using System;
using System.Collections.Generic;
using System.Text;

namespace Blazor.Quartz.Core.Service.Timer.Enum
{
    /// <summary>
    /// 触发器类型
    /// </summary>
    public enum TriggerTypeEnum
    {
        None = 0,
        Simple = 1,
        Cron = 2
    }
}
