using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazor.Quartz.Core.Service.Admin.Dto
{
    /// <summary>
    /// 管理员信息（不含密码等敏感字段）
    /// </summary>
    public class AdminInfo
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string USERNAME { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        public string REAL_NAME { get; set; }
    }
}
