﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazor.Quartz.Core.Const
{
    public class QuartzConstant
    {
        /// <summary>
        /// 请求url RequestUrl
        /// </summary>
        public const string REQUESTURL = "RequestUrl";
        /// <summary>
        /// 请求参数 RequestParameters
        /// </summary>
        public const string REQUESTPARAMETERS = "RequestParameters";
        /// <summary>
        /// Headers（可以包含：Authorization授权认证）
        /// </summary>
        public const string HEADERS = "Headers";
        /// <summary>
        /// 是否发送邮件
        /// </summary>
        public const string MAILMESSAGE = "MailMessage";
        /// <summary>
        /// 请求类型 RequestType
        /// </summary>
        public const string REQUESTTYPE = "RequestType";
        /// <summary>
        /// 日志 LogList
        /// </summary>
        public const string LOGLIST = "LogList";
        /// <summary>
        /// 异常 Exception
        /// </summary>
        public const string EXCEPTION = "Exception";
        /// <summary>
        /// 执行次数
        /// </summary>
        public const string RUNNUMBER = "RunNumber";

        /// <summary>
        /// 超时时长
        /// </summary>
        public const string TIMEOUT = "TimeOut";

        public const string MailTitle = "MailTitle";
        public const string MailContent = "MailContent";
        public const string MailTo = "MailTo";

        public const string JobTypeEnum = "JobTypeEnum";

        public const string EndAt = "EndAt";

        /// <summary>
        /// 表前缀
        /// </summary>
        public const string TablePrefix = "QRTZ_";
    }
}
