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
            var param = new DynamicParameters();
            
            // Tab Status Filter (Semua, Draft, Pending, Approved, Rejected)
            if (!string.IsNullOrWhiteSpace(filter.TabStatus) && filter.TabStatus != "All" && filter.TabStatus != "Semua")
            {
                if (filter.TabStatus.Equals("Draft", StringComparison.OrdinalIgnoreCase))
                {
                    sql += " AND (status = 'draft' OR acc_tidak = 3)";
                }
                else if (filter.TabStatus.Equals("Pending", StringComparison.OrdinalIgnoreCase))
                {
                    sql += " AND (status = 'pending' OR acc_tidak = 0)";
                }
                else if (filter.TabStatus.Equals("Approved", StringComparison.OrdinalIgnoreCase) || filter.TabStatus.Equals("Disetujui", StringComparison.OrdinalIgnoreCase))
                {
                    sql += " AND (status = 'approve' OR acc_tidak = 1)";
                }
                else if (filter.TabStatus.Equals("Rejected", StringComparison.OrdinalIgnoreCase) || filter.TabStatus.Equals("Ditolak", StringComparison.OrdinalIgnoreCase))
                {
                    sql += " AND (status = 'reject' OR acc_tidak = 2)";
                }
            }
            else if (filter.Status.HasValue)
            {
                if (filter.Status.Value == 0) // Pending
                {
                    sql += " AND (acc_tidak IN (0, 3) OR status IN ('draft', 'pending'))";
                }
                else if (filter.Status.Value == 1) // Disetujui
                {
                    sql += " AND (acc_tidak = 1 OR status = 'approve')";
                }
                else if (filter.Status.Value == 2) // Ditolak
                {
                    sql += " AND (acc_tidak = 2 OR status = 'reject')";
                }
            }

            if (!string.IsNullOrWhiteSpace(filter.Area) && filter.Area != "< Semua Area >")
            {
                sql += " AND id_area = @Area";
                param.Add("Area", filter.Area);
            }

            if (!string.IsNullOrWhiteSpace(filter.Toko) && filter.Toko != "< Semua Toko >")
            {
                sql += " AND tkkd = @Toko";
                param.Add("Toko", filter.Toko);
            }
            
            if (filter.StartDate.HasValue)
            {
                sql += " AND created_at >= @StartDate";
                param.Add("StartDate", filter.StartDate.Value);
            }
            if (filter.EndDate.HasValue)
            {
                sql += " AND created_at <= @EndDate";
                param.Add("EndDate", filter.EndDate.Value);
            }
            
            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                sql += " AND (nm_brg LIKE @Kw OR supplier LIKE @Kw OR tkkd LIKE @Kw OR id_area LIKE @Kw OR jns_brg LIKE @Kw)";
                param.Add("Kw", $"%{filter.Keyword.Trim()}%");
            }
            
            sql += " ORDER BY id DESC";

            var list = (await connection.QueryAsync<ReqEdpKodeRecord>(sql, param)).AsList();
            for (int i = 0; i < list.Count; i++)
            {
                list[i].RowNumber = i + 1;
            }
            return list;
        }

        public async Task<StatusCountsDto> GetStatusCountsAsync(ReqEdpFilter filter)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var whereClause = " WHERE 1=1";
            var param = new DynamicParameters();
            
            if (!string.IsNullOrWhiteSpace(filter.Area) && filter.Area != "< Semua Area >")
            {
                whereClause += " AND id_area = @Area";
                param.Add("Area", filter.Area);
            }

            if (!string.IsNullOrWhiteSpace(filter.Toko) && filter.Toko != "< Semua Toko >")
            {
                whereClause += " AND tkkd = @Toko";
                param.Add("Toko", filter.Toko);
            }
            
            if (filter.StartDate.HasValue)
            {
                whereClause += " AND created_at >= @StartDate";
                param.Add("StartDate", filter.StartDate.Value);
            }
            if (filter.EndDate.HasValue)
            {
                whereClause += " AND created_at <= @EndDate";
                param.Add("EndDate", filter.EndDate.Value);
            }
            
            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                whereClause += " AND (nm_brg LIKE @Kw OR supplier LIKE @Kw OR tkkd LIKE @Kw OR id_area LIKE @Kw OR jns_brg LIKE @Kw)";
                param.Add("Kw", $"%{filter.Keyword.Trim()}%");
            }

            var sql = $@"
                SELECT 
                    COUNT(*) AS `All`,
                    COALESCE(SUM(CASE WHEN status = 'draft' OR acc_tidak = 3 THEN 1 ELSE 0 END), 0) AS `Draft`,
                    COALESCE(SUM(CASE WHEN status = 'pending' OR acc_tidak = 0 THEN 1 ELSE 0 END), 0) AS `Pending`,
                    COALESCE(SUM(CASE WHEN status = 'approve' OR acc_tidak = 1 THEN 1 ELSE 0 END), 0) AS `Approved`,
                    COALESCE(SUM(CASE WHEN status = 'reject' OR acc_tidak = 2 THEN 1 ELSE 0 END), 0) AS `Rejected`
                FROM req_edp_kode
                {whereClause};
            ";

            return await connection.QueryFirstOrDefaultAsync<StatusCountsDto>(sql, param) ?? new StatusCountsDto();
        }

        public async Task<ReqEdpKodeRecord?> GetRequestByIdAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = "SELECT * FROM req_edp_kode WHERE id = @Id";
            return await connection.QueryFirstOrDefaultAsync<ReqEdpKodeRecord>(sql, new { Id = id });
        }

        public async Task<IEnumerable<string>> GetDistinctAreasAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = "SELECT DISTINCT id_area FROM req_edp_kode WHERE id_area IS NOT NULL AND id_area != '' ORDER BY id_area";
            return await connection.QueryAsync<string>(sql);
        }

        public async Task<IEnumerable<string>> GetDistinctTokosAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = "SELECT DISTINCT tkkd FROM req_edp_kode WHERE tkkd IS NOT NULL AND tkkd != '' ORDER BY tkkd";
            return await connection.QueryAsync<string>(sql);
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
                        status = 'approve',
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
                        status = 'reject',
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
