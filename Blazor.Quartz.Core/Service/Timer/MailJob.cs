using Blazor.Quartz.Common.Mail;
using Blazor.Quartz.Core.Const;
using Blazor.Quartz.Core.Service.Timer.Dto;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazor.Quartz.Core.Service.Timer
{
    public class MailJob : JobBase<LogMailModel>, IJob
    {
        public MailJob() : base(new LogMailModel())
        { }

        public override async Task NextExecute(IJobExecutionContext context)
        {
            var title = context.JobDetail.JobDataMap.GetString(QuartzConstant.MailTitle);
            var content = context.JobDetail.JobDataMap.GetString(QuartzConstant.MailContent);
            var mailTo = context.JobDetail.JobDataMap.GetString(QuartzConstant.MailTo);

            LogInfo.Title = title;
            LogInfo.Content = content;
            LogInfo.MailTo = mailTo;

            await MailHelper.SendMail(title, content, mailTo);

            LogInfo.Result = "发送成功！";
        }
    }
}
