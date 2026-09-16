using System.Data;

namespace Entry_Data_Processing.Core.Data
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}
