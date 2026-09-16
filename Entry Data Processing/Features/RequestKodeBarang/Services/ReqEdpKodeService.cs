using System;
using System.Collections.Generic;
using System.Linq;
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
            
            var sql = @"
                SELECT 
                    r.id,
                    r.created_at,
                    r.updated_at,
                    r.nm_brg,
                    r.jns_brg,
                    j.BrJnsNm AS nm_jns,
                    r.ready_or_mix,
                    r.supplier,
                    r.kd_supp,
                    ts.namaPT AS nm_supplier,
                    r.pricelist,
                    r.disc,
                    r.hrg_beli,
                    r.ket_beli,
                    r.faktur_pajak,
                    r.lampiran_faktur,
                    r.foto_brg,
                    r.tkkd,
                    s.store_call,
                    s.nama_toko,
                    r.id_area,
                    r.kd_prd,
                    r.nm_prd_acc,
                    r.kd_brg,
                    r.nm_brg_acc,
                    r.gol,
                    r.sat,
                    r.supplier_acc,
                    r.kd_supp_acc,
                    r.acc_tidak,
                    r.status,
                    r.changed_by,
                    r.changed_at,
                    r.acc_by,
                    r.acc_at,
                    r.created_by
                FROM req_edp_kode r
                LEFT JOIN tmabrgjns j ON CONVERT(r.jns_brg USING utf8mb4) = CONVERT(j.BrJnsKd USING utf8mb4)
                LEFT JOIN store s ON CONVERT(r.tkkd USING utf8mb4) = CONVERT(s.kode_toko USING utf8mb4)
                LEFT JOIN t_bridge_supplier bs ON CONVERT(r.supplier USING utf8mb4) = CONVERT(bs.id_supplier USING utf8mb4)
                LEFT JOIN t_supplier ts ON bs.id_suppAll = ts.id_suppAll
                WHERE 1=1
            ";

            var param = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(filter.TabStatus) && filter.TabStatus != "All" && filter.TabStatus != "Semua")
            {
                if (filter.TabStatus.Equals("Draft", StringComparison.OrdinalIgnoreCase))
                {
                    sql += " AND (r.status = 'draft' OR r.acc_tidak = 3)";
                }
                else if (filter.TabStatus.Equals("Pending", StringComparison.OrdinalIgnoreCase))
                {
                    sql += " AND (r.status = 'pending' OR r.acc_tidak = 0)";
                }
                else if (filter.TabStatus.Equals("Approved", StringComparison.OrdinalIgnoreCase) || filter.TabStatus.Equals("Disetujui", StringComparison.OrdinalIgnoreCase))
                {
                    sql += " AND (r.status = 'approve' OR r.acc_tidak = 1)";
                }
                else if (filter.TabStatus.Equals("Rejected", StringComparison.OrdinalIgnoreCase) || filter.TabStatus.Equals("Ditolak", StringComparison.OrdinalIgnoreCase))
                {
                    sql += " AND (r.status = 'reject' OR r.acc_tidak = 2)";
                }
            }
            else if (filter.Status.HasValue)
            {
                if (filter.Status.Value == 0)
                {
                    sql += " AND (r.acc_tidak IN (0, 3) OR r.status IN ('draft', 'pending'))";
                }
                else if (filter.Status.Value == 1)
                {
                    sql += " AND (r.acc_tidak = 1 OR r.status = 'approve')";
                }
                else if (filter.Status.Value == 2)
                {
                    sql += " AND (r.acc_tidak = 2 OR r.status = 'reject')";
                }
            }

            if (!string.IsNullOrWhiteSpace(filter.Area) && filter.Area != "< Semua Area >")
            {
                sql += " AND r.id_area = @Area";
                param.Add("Area", filter.Area);
            }

            if (!string.IsNullOrWhiteSpace(filter.Toko) && filter.Toko != "< Semua Toko >")
            {
                sql += " AND r.tkkd = @Toko";
                param.Add("Toko", filter.Toko);
            }
            
            if (filter.StartDate.HasValue)
            {
                sql += " AND r.created_at >= @StartDate";
                param.Add("StartDate", filter.StartDate.Value);
            }
            if (filter.EndDate.HasValue)
            {
                sql += " AND r.created_at <= @EndDate";
                param.Add("EndDate", filter.EndDate.Value);
            }
            
            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                sql += " AND (r.nm_brg LIKE @Kw OR r.supplier LIKE @Kw OR ts.namaPT LIKE @Kw OR r.tkkd LIKE @Kw OR s.store_call LIKE @Kw OR r.id_area LIKE @Kw OR j.BrJnsNm LIKE @Kw)";
                param.Add("Kw", $"%{filter.Keyword.Trim()}%");
            }
            
            sql += " ORDER BY r.id DESC";

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
                whereClause += " AND r.id_area = @Area";
                param.Add("Area", filter.Area);
            }

            if (!string.IsNullOrWhiteSpace(filter.Toko) && filter.Toko != "< Semua Toko >")
            {
                whereClause += " AND r.tkkd = @Toko";
                param.Add("Toko", filter.Toko);
            }
            
            if (filter.StartDate.HasValue)
            {
                whereClause += " AND r.created_at >= @StartDate";
                param.Add("StartDate", filter.StartDate.Value);
            }
            if (filter.EndDate.HasValue)
            {
                whereClause += " AND r.created_at <= @EndDate";
                param.Add("EndDate", filter.EndDate.Value);
            }
            
            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                whereClause += " AND (r.nm_brg LIKE @Kw OR r.supplier LIKE @Kw OR ts.namaPT LIKE @Kw OR r.tkkd LIKE @Kw OR s.store_call LIKE @Kw OR r.id_area LIKE @Kw OR j.BrJnsNm LIKE @Kw)";
                param.Add("Kw", $"%{filter.Keyword.Trim()}%");
            }

            var sql = $@"
                SELECT 
                    COUNT(*) AS `All`,
                    COALESCE(SUM(CASE WHEN r.status = 'draft' OR r.acc_tidak = 3 THEN 1 ELSE 0 END), 0) AS `Draft`,
                    COALESCE(SUM(CASE WHEN r.status = 'pending' OR r.acc_tidak = 0 THEN 1 ELSE 0 END), 0) AS `Pending`,
                    COALESCE(SUM(CASE WHEN r.status = 'approve' OR r.acc_tidak = 1 THEN 1 ELSE 0 END), 0) AS `Approved`,
                    COALESCE(SUM(CASE WHEN r.status = 'reject' OR r.acc_tidak = 2 THEN 1 ELSE 0 END), 0) AS `Rejected`
                FROM req_edp_kode r
                LEFT JOIN tmabrgjns j ON CONVERT(r.jns_brg USING utf8mb4) = CONVERT(j.BrJnsKd USING utf8mb4)
                LEFT JOIN store s ON CONVERT(r.tkkd USING utf8mb4) = CONVERT(s.kode_toko USING utf8mb4)
                LEFT JOIN t_bridge_supplier bs ON CONVERT(r.supplier USING utf8mb4) = CONVERT(bs.id_supplier USING utf8mb4)
                LEFT JOIN t_supplier ts ON bs.id_suppAll = ts.id_suppAll
                {whereClause};
            ";

            return await connection.QueryFirstOrDefaultAsync<StatusCountsDto>(sql, param) ?? new StatusCountsDto();
        }

        public async Task<ReqEdpKodeRecord?> GetRequestByIdAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                SELECT 
                    r.*,
                    j.BrJnsNm AS nm_jns,
                    s.store_call,
                    s.nama_toko,
                    ts.namaPT AS nm_supplier
                FROM req_edp_kode r
                LEFT JOIN tmabrgjns j ON CONVERT(r.jns_brg USING utf8mb4) = CONVERT(j.BrJnsKd USING utf8mb4)
                LEFT JOIN store s ON CONVERT(r.tkkd USING utf8mb4) = CONVERT(s.kode_toko USING utf8mb4)
                LEFT JOIN t_bridge_supplier bs ON CONVERT(r.supplier USING utf8mb4) = CONVERT(bs.id_supplier USING utf8mb4)
                LEFT JOIN t_supplier ts ON bs.id_suppAll = ts.id_suppAll
                WHERE r.id = @Id;
            ";
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

        public async Task<Result<int>> ApproveBulkAsync(IEnumerable<int> ids, string approverNip)
        {
            var idList = ids.ToList();
            if (!idList.Any()) return Result<int>.Failure("Tidak ada data yang dipilih.");

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
                    WHERE id IN @Ids;
                ";

                var rows = await connection.ExecuteAsync(sql, new { ApproverNip = approverNip, Ids = idList });
                return Result<int>.Success(rows);
            }
            catch (Exception ex)
            {
                return Result<int>.Failure(ex.Message);
            }
        }

        public async Task<Result<int>> RejectBulkAsync(IEnumerable<int> ids, string approverNip, string? alasan)
        {
            var idList = ids.ToList();
            if (!idList.Any()) return Result<int>.Failure("Tidak ada data yang dipilih.");

            try
            {
                using var connection = _connectionFactory.CreateConnection();
                var sql = @"
                    UPDATE req_edp_kode 
                    SET acc_tidak = 2,
                        acc_by = @ApproverNip,
                        acc_at = NOW(),
                        status = 'reject',
                        updated_at = NOW()
                    WHERE id IN @Ids;
                ";

                var rows = await connection.ExecuteAsync(sql, new { ApproverNip = approverNip, Ids = idList });
                return Result<int>.Success(rows);
            }
            catch (Exception ex)
            {
                return Result<int>.Failure(ex.Message);
            }
        }
    }
}
