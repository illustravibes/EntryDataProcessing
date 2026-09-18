using System.Threading.Tasks;
using Dapper;
using Entry_Data_Processing.Core.Data;
using Entry_Data_Processing.Features.Dashboard.Models;

namespace Entry_Data_Processing.Features.Dashboard.Services
{
    public class DashboardService
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public DashboardService(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<DashboardSummary> GetSummaryAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var pendingCount = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM req_edp_kode WHERE acc_tidak IN (0, 3) OR status IN ('draft', 'pending')"
            );
            
            return new DashboardSummary
            {
                PendingRequestCount = pendingCount
            };
        }
    }
}
