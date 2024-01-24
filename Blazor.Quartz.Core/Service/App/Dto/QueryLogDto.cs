using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazor.Quartz.Core.Service.App.Dto
{
    public class QueryLogDto : PageQueryDto
    {
        public string group { get; set; }

        public string name { get; set; }

        public int? status { get; set; }

        public string start_time { get; set; }

        public string end_time { get; set; }
    }

    public class PageQueryDto 
    {
        public int page_index { get; set; }

        public int page_size { get; set; }
    }
}
