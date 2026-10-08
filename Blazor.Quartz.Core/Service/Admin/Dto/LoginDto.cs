using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazor.Quartz.Core.Service.Admin.Dto
{
    /// <summary>
    /// 登录请求参数
    /// </summary>
    public class LoginDto
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public string USERNAME { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string PASSWORD { get; set; }

        /// <summary>
        /// 验证码ID（登录页生成验证码图时返回）
        /// </summary>
        public string CAPTCHA_ID { get; set; }

        /// <summary>
        /// 用户输入的图形验证码
        /// </summary>
        public string CAPTCHA_CODE { get; set; }
    }
}
