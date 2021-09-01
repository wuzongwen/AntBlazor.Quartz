using Blazor.Quartz.Core.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazor.Quartz.Core.Service.Timer.Dto
{
    public class ModifyJobInput
    {
        public ScheduleEntity NewScheduleEntity { get; set; }
        public ScheduleEntity OldScheduleEntity { get; set; }
    }
}
