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
        public static string ApiHost => ConfigurationManager.GetTryConfig("ApiHost");
        public static string ServiceName => ConfigurationManager.GetTryConfig("SysConfig:ServiceName");
        public static string DisplayName => ConfigurationManager.GetTryConfig("SysConfig:DisplayName");
        public static string Description => ConfigurationManager.GetTryConfig("SysConfig:Description");
    }
}
