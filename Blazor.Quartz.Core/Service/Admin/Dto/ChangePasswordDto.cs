using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazor.Quartz.Core.Service.Admin.Dto
{
    /// <summary>
    /// 修改密码请求参数
    /// </summary>
    public class ChangePasswordDto
    {
        /// <summary>
        /// 原密码
        /// </summary>
        public string OLD_PASSWORD { get; set; }

        /// <summary>
        /// 新密码
        /// </summary>
        public string NEW_PASSWORD { get; set; }

        /// <summary>
        /// 确认新密码
        /// </summary>
        public string CONFIRM_PASSWORD { get; set; }
    }
}
