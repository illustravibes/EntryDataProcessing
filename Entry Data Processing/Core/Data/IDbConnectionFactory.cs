using System.Data;

namespace Entry_Data_Processing.Core.Data
{
    public interface IDbConnectionFactory
    {
        DatabaseProvider Provider { get; }
        IDbConnection CreateConnection();
    }

    public enum DatabaseProvider
    {
        MySql,
        Access
    }
}
