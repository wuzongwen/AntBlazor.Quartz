using Blazor.Quartz.Core.Service.App.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazor.Quartz.Core.Service.App.Dto
{
    public class JOB_EXECUTION_LOG
    {
        /// <summary>
        /// 任务名称
        /// </summary>
        [DisplayName("任务名称")]
        [Required(ErrorMessage = "任务名称必须填写")]
        public string JOB_NAME { get; set; }

        /// <summary>
        /// 分组名称
        /// </summary>
        [DisplayName("分组名称")]
        [Required(ErrorMessage = "分组名称必须填写")]
        public string JOB_GROUP { get; set; }

        /// <summary>
        /// 任务状态(1:成功 0:失败)
        /// </summary>
        [DisplayName("状态")]
        public ExecutionStatusEnum EXECUTION_STATUS { get; set; }

        /// <summary>
        /// 请求地址
        /// </summary>
        [DisplayName("请求地址")]
        [Required(ErrorMessage = "请求地址必须填写")]
        public string REQUEST_URL { get; set; }

        /// <summary>
        /// 请求类型
        /// </summary>
        [DisplayName("请求类型")]
        [Required(ErrorMessage = "请求类型必须填写")]
        public string REQUEST_TYPE { get; set; }

        /// <summary>
        /// 请求头
        /// </summary>
        [DisplayName("请求头")]
        [Required(ErrorMessage = "请求头必须填写")]
        public string HEADERS { get; set; }

        /// <summary>
        /// 请求数据
        /// </summary>
        [DisplayName("请求数据")]
        [Required(ErrorMessage = "请求数据必须填写")]
        public string REQUEST_DATA { get; set; }

        /// <summary>
        /// 相应数据
        /// </summary>
        [DisplayName("相应数据")]
        [Required(ErrorMessage = "相应数据必须填写")]
        public string RESPONSE_DATA { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        [DisplayName("开始时间")]
        [Required(ErrorMessage = "开始时间必须填写")]
        public string BEGIN_TIME { get; set; }
    }
}
