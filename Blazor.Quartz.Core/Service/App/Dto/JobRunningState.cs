using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazor.Quartz.Core.Service.App.Dto
{
    public class JobRunningState
    {
        public int HourOfDay { get; set; }
        public int Count { get; set; }
        public int CountStatus0 { get; set; }
        public int CountStatus1 { get; set; }
    }
}
