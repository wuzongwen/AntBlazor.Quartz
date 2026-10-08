using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazor.Quartz.Core.Entity
{
    /// <summary>
    /// 管理员表实体（SYS_ADMIN）
    /// </summary>
    public class AdminEntity
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
        /// 密码（PBKDF2哈希，Base64存储）
        /// </summary>
        public string PASSWORD { get; set; }

        /// <summary>
        /// 密码盐（Base64存储）
        /// </summary>
        public string SALT { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        public string REAL_NAME { get; set; }

        /// <summary>
        /// 是否启用：1-启用 0-禁用
        /// </summary>
        public int IS_ENABLE { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CREATE_TIME { get; set; }

        /// <summary>
        /// 最后登录时间
        /// </summary>
        public DateTime? LAST_LOGIN_TIME { get; set; }
    }
}
