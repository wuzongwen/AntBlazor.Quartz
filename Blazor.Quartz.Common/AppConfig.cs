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
    }
}
