using Blazor.Quartz.Core.Entity;
using Blazor.Quartz.Core.Service.App.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazor.Quartz.Core.Service.App.Dto
{
    public class QueryModel: GroupEntity
    {
        /// <summary>
        /// 分组名称
        /// </summary>
        [DisplayName("名称")]
        public new string JOB_GROUP_NAME { get; set; }

        /// <summary>
        /// 启用状态(1:启用 0:停用)
        /// </summary>
        [DisplayName("状态")]
        public new AppStatusEnum? IS_ENABLE { get; set; }
    }
}
