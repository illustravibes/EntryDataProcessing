using Microsoft.Extensions.Configuration;
using System.IO;

namespace Entry_Data_Processing.Core.Configuration
{
    public static class ConfigurationManager
    {
        public static AppConfig LoadConfiguration()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var appConfig = new AppConfig();
            config.Bind(appConfig);
            return appConfig;
        }
    }
}
