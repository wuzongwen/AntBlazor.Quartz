using Blazor.Quartz.Common.Mail.Entity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazor.Quartz.Common
{
    public static class FileConfig
    {
        private static string filePath = "File/Mail.txt";

        private static MailEntity mailData = null;
        public static async Task<MailEntity> GetMailInfoAsync()
        {
            if (mailData == null)
            {
                if (!System.IO.File.Exists(filePath)) return new MailEntity();
                var mail = await System.IO.File.ReadAllTextAsync(filePath);
                mailData = JsonConvert.DeserializeObject<MailEntity>(mail);
            }
            //深度复制，调用方修改。
            return JsonConvert.DeserializeObject<MailEntity>(JsonConvert.SerializeObject(mailData));
        }

        public static async Task<bool> SaveMailInfoAsync(MailEntity mailEntity)
        {
            mailData = mailEntity;
            await System.IO.File.WriteAllTextAsync(filePath, JsonConvert.SerializeObject(mailEntity));
            return true;
        }
    }
}
