using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talk.Extensions;

namespace Blazor.Quartz.Common
{
    public static class AppConfig
    {
        public static string DbProviderName => ConfigurationManager.GetTryConfig("DbConfig:dbProviderName");
        public static string ConnectionString => ConfigurationManager.GetTryConfig("DbConfig:connectionString");
        public static string DingTalkRobot => ConfigurationManager.GetTryConfig("DingTalkRobot");
        public static string DingTalkWebHook => ConfigurationManager.GetTryConfig("DingTalkRobot:Webhook");
        public static string DingTalkKeyWord => ConfigurationManager.GetTryConfig("DingTalkRobot:Keyword");
        public static string ApiHost => ConfigurationManager.GetTryConfig("SysConfig:ApiHost");
        public static string ServiceName => ConfigurationManager.GetTryConfig("SysConfig:ServiceName");
        public static string DisplayName => ConfigurationManager.GetTryConfig("SysConfig:DisplayName");
        public static string Description => ConfigurationManager.GetTryConfig("SysConfig:Description");
        public static int RunLogStorageDays => Convert.ToInt32(ConfigurationManager.GetTryConfig("SysConfig:RunLogStorageDays","7"));
        public static bool AutoClearnLog => Convert.ToBoolean(ConfigurationManager.GetTryConfig("SysConfig:AutoClearnLog","false"));
        public static string CheckJobCron => ConfigurationManager.GetTryConfig("CronConfig:CheckJobCron");
        public static string ReportJobCron => ConfigurationManager.GetTryConfig("CronConfig:ReportJobCron"); 
        public static string ClearnLogJobCron => ConfigurationManager.GetTryConfig("CronConfig:ClearnLogJobCron");
        public static int WarnTime => Convert.ToInt32(ConfigurationManager.GetTryConfig("SysConfig:WarnTime", "30"));
    }
}
