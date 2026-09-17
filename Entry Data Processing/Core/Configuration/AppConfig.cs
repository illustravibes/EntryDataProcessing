using System.Text.Json.Serialization;

namespace Entry_Data_Processing.Core.Configuration
{
    public class AppConfig
    {
        public ConnectionStringsConfig ConnectionStrings { get; set; } = new();
        public ApplicationConfig Application { get; set; } = new();
    }

    public class ConnectionStringsConfig
    {
        public string Provider { get; set; } = "Access";
        public string WambDatabase { get; set; } = string.Empty;
        public string AccessDatabase { get; set; } = string.Empty;
    }

    public class ApplicationConfig
    {
        public string AppName { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Branch { get; set; } = string.Empty;
        public int AutoRefreshIntervalSeconds { get; set; } = 60;
    }
}
