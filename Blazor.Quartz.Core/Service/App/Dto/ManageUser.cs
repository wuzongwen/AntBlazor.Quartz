using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazor.Quartz.Core.Service.App.Dto
{
    public class ManageUser
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        public int IsEnable { get; set; }

        public DateTime LastLoginTime { get; set; }

        public string Remark { get; set; }
    }
}
