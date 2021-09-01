using Blazor.Quartz.Core.Service.App.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazor.Quartz.Core.Entity
{
    public class GroupEntity
    {
        /// <summary>
        /// 分组名称
        /// </summary>
        [DisplayName("名称")]
        [Required(ErrorMessage = "名称必须填写")]
        public string JOB_GROUP_NAME { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [DisplayName("描述")]
        public string DESCRIPTION { get; set; }

        /// <summary>
        /// 启用状态(1:启用 0:停用)
        /// </summary>
        [DisplayName("状态")]
        public AppStatusEnum IS_ENABLE { get; set; }
    }
}
