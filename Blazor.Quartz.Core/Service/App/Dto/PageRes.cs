using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazor.Quartz.Core.Service.App.Dto
{
    public class PageRes<T>
    {
        public T data { get; set; }

        public int total { get; set; }
    }
}
