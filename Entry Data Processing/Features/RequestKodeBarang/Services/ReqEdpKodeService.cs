using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Entry_Data_Processing.Core.Common;
using Entry_Data_Processing.Core.Data;
using Entry_Data_Processing.Features.RequestKodeBarang.Models;

namespace Entry_Data_Processing.Features.RequestKodeBarang.Services
{
    public class ReqEdpKodeService : IReqEdpKodeService
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public ReqEdpKodeService(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<ReqEdpKodeRecord>> GetRequestsAsync(ReqEdpFilter filter)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var sql = "SELECT * FROM req_edp_kode WHERE 1=1";
            
            if (filter.Status.HasValue)
            {
                sql += " AND acc_tidak = @Status";
            }
            
            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                sql += " AND (nm_brg LIKE @Keyword OR no_request LIKE @Keyword OR supplier LIKE @Keyword)";
                filter.Keyword = $"%{filter.Keyword}%";
            }
            
            sql += " ORDER BY created_at DESC";

            return await connection.QueryAsync<ReqEdpKodeRecord>(sql, filter);
        }

        public async Task<ReqEdpKodeRecord?> GetRequestByIdAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = "SELECT * FROM req_edp_kode WHERE id = @Id";
            return await connection.QueryFirstOrDefaultAsync<ReqEdpKodeRecord>(sql, new { Id = id });
        }

        public async Task<Result<bool>> ApproveRequestAsync(ApprovalActionDto action)
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();
                var sql = @"
                    UPDATE req_edp_kode 
                    SET acc_tidak = 1,
                        acc_by = @ApproverNip,
                        acc_at = NOW(),
                        status = 'approved',
                        updated_at = NOW()
                    WHERE id = @Id;
                ";

                var rows = await connection.ExecuteAsync(sql, action);
                return rows > 0 ? Result<bool>.Success(true) : Result<bool>.Failure("Data tidak ditemukan atau gagal diupdate.");
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure(ex.Message);
            }
        }

        public async Task<Result<bool>> RejectRequestAsync(ApprovalActionDto action)
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();
                var sql = @"
                    UPDATE req_edp_kode 
                    SET acc_tidak = 2,
                        acc_by = @ApproverNip,
                        acc_at = NOW(),
                        keterangan_tolak = @Alasan,
                        status = 'rejected',
                        updated_at = NOW()
                    WHERE id = @Id;
                ";

                var rows = await connection.ExecuteAsync(sql, action);
                return rows > 0 ? Result<bool>.Success(true) : Result<bool>.Failure("Data tidak ditemukan atau gagal diupdate.");
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure(ex.Message);
            }
        }
    }
}
