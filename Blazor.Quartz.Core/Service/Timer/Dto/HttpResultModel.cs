using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazor.Quartz.Core.Service.Timer.Dto
{
    /// <summary>
    /// Job任务结果
    /// </summary>
    public class HttpResultModel
    {
        /// <summary>
        /// 返回值编码
        /// </summary>
        public resCode resCode { get; set; }
        /// <summary>
        /// 返回值说明
        /// </summary>
        public string resMsg { get; set; }

        /// <summary>
        /// 返回数据集 
        /// </summary>
        public object resData { get; set; }

        /// <summary>
        /// 是否成功
        /// </summary>
        /// <returns></returns>
        public bool isSuccess { get; set; }
    }

    /// <summary>
    /// 错误码  0成功 -1失败
    /// </summary>
    public enum resCode
    {
        /// <summary>
        /// 成功
        /// </summary>
        Success = 0,

        /// <summary>
        /// 失败
        /// </summary>
        Error = -1
    }
}
