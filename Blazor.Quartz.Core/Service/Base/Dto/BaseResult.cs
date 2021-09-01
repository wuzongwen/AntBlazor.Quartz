using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazor.Quartz.Core.Service.Base.Dto
{
    public class BaseResult<T>
    {
        public int Code { get; set; } = 200;
        public string Msg { get; set; }

        public T Data { get; set; }
    }

    public class BaseResult
    {
        public int Code { get; set; } = 200;
        public string Msg { get; set; }
    }
}
