using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazor.Quartz.Core.Service.Timer.Enum
{
    public enum JobTypeEnum
    {
        None = 0,
        Url = 1,
        Emial = 2,
        Mqtt = 3,
        RabbitMQ = 4,
        Hotreload = 5
    }
}
