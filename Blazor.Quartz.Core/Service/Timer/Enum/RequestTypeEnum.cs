using System;
using System.Collections.Generic;
using System.Text;

namespace Blazor.Quartz.Core.Service.Timer.Enum
{
    /// <summary>
    /// http请求类型
    /// </summary>
    public enum RequestTypeEnum
    {
        None = 0,
        Get = 1,
        Post = 2,
        Put = 4,
        Delete = 8
    }
}
