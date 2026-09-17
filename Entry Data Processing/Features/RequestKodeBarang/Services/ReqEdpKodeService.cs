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
                    u_acc.nama AS acc_by_name,
                    r.acc_at,
                    r.created_by,
                    u_cr.nama AS created_by_name
                FROM req_edp_kode r
                LEFT JOIN tmabrgjns j ON CONVERT(r.jns_brg USING utf8mb4) = CONVERT(j.BrJnsKd USING utf8mb4)
                LEFT JOIN store s ON CONVERT(r.tkkd USING utf8mb4) = CONVERT(s.kode_toko USING utf8mb4)
                LEFT JOIN t_bridge_supplier bs ON CONVERT(r.supplier USING utf8mb4) = CONVERT(bs.id_supplier USING utf8mb4)
                LEFT JOIN t_supplier ts ON bs.id_suppAll = ts.id_suppAll
                LEFT JOIN user u_cr ON CONVERT(r.created_by USING utf8mb4) = CONVERT(u_cr.nip USING utf8mb4)
                LEFT JOIN user u_acc ON CONVERT(r.acc_by USING utf8mb4) = CONVERT(u_acc.nip USING utf8mb4)
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

            if (!string.IsNullOrWhiteSpace(filter.Area) && filter.Area != "Semua Area" && filter.Area != "< Semua Area >")
            {
                sql += " AND r.id_area = @Area";
                param.Add("Area", filter.Area);
            }

            if (!string.IsNullOrWhiteSpace(filter.Toko) && filter.Toko != "Semua Toko" && filter.Toko != "< Semua Toko >")
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
            
            sql += " ORDER BY r.created_at DESC, r.id DESC";

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
                    ts.namaPT AS nm_supplier,
                    u_cr.nama AS created_by_name,
                    u_acc.nama AS acc_by_name
                FROM req_edp_kode r
                LEFT JOIN tmabrgjns j ON CONVERT(r.jns_brg USING utf8mb4) = CONVERT(j.BrJnsKd USING utf8mb4)
                LEFT JOIN store s ON CONVERT(r.tkkd USING utf8mb4) = CONVERT(s.kode_toko USING utf8mb4)
                LEFT JOIN t_bridge_supplier bs ON CONVERT(r.supplier USING utf8mb4) = CONVERT(bs.id_supplier USING utf8mb4)
                LEFT JOIN t_supplier ts ON bs.id_suppAll = ts.id_suppAll
                LEFT JOIN user u_cr ON CONVERT(r.created_by USING utf8mb4) = CONVERT(u_cr.nip USING utf8mb4)
                LEFT JOIN user u_acc ON CONVERT(r.acc_by USING utf8mb4) = CONVERT(u_acc.nip USING utf8mb4)
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

        public async Task<IEnumerable<ProductDataDto>> SearchProductsAsync(string query)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                SELECT 
                    p.BrPrdKd, 
                    p.BrPrdNm, 
                    p.BrPrdAcm, 
                    p.BrPrdFacKd, 
                    COALESCE(f.BrprdfacNm, '') AS BrPrdFacNm,
                    p.Pencari, 
                    p.BrJnsKd,
                    COALESCE(j.BrJnsNm, '') AS BrJnsNm
                FROM tmabrprd p
                LEFT JOIN tmabrfac f ON CONVERT(p.BrPrdFacKd USING utf8mb4) = CONVERT(f.brprdfac USING utf8mb4)
                LEFT JOIN tmabrgjns j ON CONVERT(p.BrJnsKd USING utf8mb4) = CONVERT(j.BrJnsKd USING utf8mb4)
                WHERE (@Query = '' OR p.BrPrdKd LIKE @QueryPattern OR p.BrPrdNm LIKE @QueryPattern OR f.BrprdfacNm LIKE @QueryPattern OR j.BrJnsNm LIKE @QueryPattern)
                ORDER BY p.BrPrdKd
                LIMIT 50;
            ";
            var q = query?.Trim() ?? string.Empty;
            return await connection.QueryAsync<ProductDataDto>(sql, new { Query = q, QueryPattern = $"%{q}%" });
        }

        public async Task<bool> CheckProductExistsAsync(string brPrdKd)
        {
            if (string.IsNullOrWhiteSpace(brPrdKd)) return false;
            using var connection = _connectionFactory.CreateConnection();
            var sql = "SELECT COUNT(*) FROM tmabrprd WHERE BrPrdKd = @BrPrdKd";
            var count = await connection.ExecuteScalarAsync<int>(sql, new { BrPrdKd = brPrdKd.Trim() });
            return count > 0;
        }

        public async Task<bool> CheckItemExistsAsync(string brKd)
        {
            if (string.IsNullOrWhiteSpace(brKd)) return false;
            using var connection = _connectionFactory.CreateConnection();
            var sql = "SELECT COUNT(*) FROM tmabrg WHERE BrKd = @BrKd";
            var count = await connection.ExecuteScalarAsync<int>(sql, new { BrKd = brKd.Trim() });
            return count > 0;
        }

        public async Task<IEnumerable<FactoryDto>> SearchFactoriesAsync(string query)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                SELECT brprdfac AS BrPrdFacKd, BrprdfacNm
                FROM tmabrfac
                WHERE (@Query = '' OR brprdfac LIKE @QueryPattern OR BrprdfacNm LIKE @QueryPattern)
                ORDER BY BrprdfacNm ASC;
            ";
            var q = query?.Trim() ?? string.Empty;
            return await connection.QueryAsync<FactoryDto>(sql, new { Query = q, QueryPattern = $"%{q}%" });
        }

        public async Task<IEnumerable<ProductTypeDto>> SearchProductTypesAsync(string query)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                SELECT BrJnsKd, BrJnsNm
                FROM tmabrgjns
                WHERE (@Query = '' OR BrJnsKd LIKE @QueryPattern OR BrJnsNm LIKE @QueryPattern)
                ORDER BY BrJnsNm ASC;
            ";
            var q = query?.Trim() ?? string.Empty;
            return await connection.QueryAsync<ProductTypeDto>(sql, new { Query = q, QueryPattern = $"%{q}%" });
        }

        public async Task<IEnumerable<string>> GetPriceGroupsAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = "SELECT DISTINCT BrHrgGol FROM tmabrhrgjl WHERE BrHrgGol IS NOT NULL AND BrHrgGol != '' ORDER BY BrHrgGol";
            return await connection.QueryAsync<string>(sql);
        }

        public async Task<IEnumerable<string>> SearchPriceGroupsAsync(string query)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                SELECT DISTINCT BrHrgGol 
                FROM tmabrhrgjl 
                WHERE BrHrgGol IS NOT NULL AND BrHrgGol != '' 
                  AND (@Query = '' OR BrHrgGol LIKE @QueryPattern)
                ORDER BY BrHrgGol ASC;
            ";
            var q = query?.Trim() ?? string.Empty;
            return await connection.QueryAsync<string>(sql, new { Query = q, QueryPattern = $"%{q}%" });
        }

        public async Task<IEnumerable<UnitDto>> SearchUnitsAsync(string query)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                SELECT satkd AS SatKd, satnm AS SatNm 
                FROM tmabrsat 
                WHERE (@Query = '' OR satkd LIKE @QueryPattern OR satnm LIKE @QueryPattern) 
                ORDER BY satnm ASC;
            ";
            var q = query?.Trim() ?? string.Empty;
            return await connection.QueryAsync<UnitDto>(sql, new { Query = q, QueryPattern = $"%{q}%" });
        }

        public async Task<bool> CheckPriceCombinationExistsAsync(string brPrdKd, string brHrgGol, string satKd)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = "SELECT COUNT(*) FROM tmabrhrgjl WHERE BrPrdKd = @BrPrdKd AND BrHrgGol = @BrHrgGol AND SatKd = @SatKd";
            var count = await connection.ExecuteScalarAsync<int>(sql, new { BrPrdKd = brPrdKd, BrHrgGol = brHrgGol, SatKd = satKd });
            return count > 0;
        }

        public async Task<PriceDataDto?> GetDefaultPriceCombinationForProductAsync(string brPrdKd)
        {
            if (string.IsNullOrWhiteSpace(brPrdKd)) return null;
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                SELECT BrHrgGol, SatKd 
                FROM tmabrhrgjl 
                WHERE BrPrdKd = @BrPrdKd 
                ORDER BY id_hrg ASC 
                LIMIT 1;
            ";
            return await connection.QueryFirstOrDefaultAsync<PriceDataDto>(sql, new { BrPrdKd = brPrdKd.Trim() });
        }

        public async Task<short> GetNextIdHrgAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            var max = await connection.ExecuteScalarAsync<short?>("SELECT MAX(id_hrg) FROM tmabrhrgjl");
            return (short)((max ?? 0) + 1);
        }

        public async Task<bool> CheckIdHrgExistsAsync(short idHrg)
        {
            using var connection = _connectionFactory.CreateConnection();
            var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM tmabrhrgjl WHERE id_hrg = @IdHrg", new { IdHrg = idHrg });
            return count > 0;
        }

        public async Task<Result<bool>> ProcessWizardApprovalAsync(ApprovalWizardSubmitDto data)
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();
                connection.Open();
                using var transaction = connection.BeginTransaction();

                try
                {
                    var prdKd = data.ProductData?.BrPrdKd?.Trim() ?? string.Empty;
                    var prdNm = data.ProductData?.BrPrdNm?.Trim() ?? string.Empty;
                    var satKd = data.PriceData?.SatKd?.Trim() ?? string.Empty;
                    var gol = data.PriceData?.BrHrgGol?.Trim() ?? string.Empty;
                    var kdNo = data.ItemData?.BrKdNo?.Trim() ?? string.Empty;
                    var brNm = data.ItemData?.BrNm?.Trim() ?? string.Empty;
                    var approverNip = string.IsNullOrWhiteSpace(data.ApproverNip) ? "SYSTEM" : data.ApproverNip;

                    if (string.IsNullOrWhiteSpace(prdKd) || string.IsNullOrWhiteSpace(satKd) || string.IsNullOrWhiteSpace(kdNo))
                    {
                        return Result<bool>.Failure("Data approval tidak lengkap (Kode Produk, Satuan, atau No. Kode Barang kosong).");
                    }

                    if (data.ProductData?.IsNewProduct == true)
                    {
                        var checkSql = "SELECT COUNT(*) FROM tmabrprd WHERE BrPrdKd = @BrPrdKd";
                        var prodCount = await connection.ExecuteScalarAsync<int>(checkSql, new { BrPrdKd = prdKd }, transaction);
                        if (prodCount > 0)
                        {
                            throw new InvalidOperationException($"Kode Produk '{prdKd}' sudah terdaftar di master produk (tmabrprd).");
                        }

                        var insProd = @"
                            INSERT INTO tmabrprd (BrPrdKd, BrPrdNm, BrPrdAcm, BrPrdFacKd, Pencari, BrJnsKd, Aktif) 
                            VALUES (@BrPrdKd, @BrPrdNm, @BrPrdAcm, @BrPrdFacKd, @Pencari, @BrJnsKd, 1);";
                        await connection.ExecuteAsync(insProd, new
                        {
                            BrPrdKd = prdKd,
                            BrPrdNm = data.ProductData?.BrPrdNm?.Trim() ?? string.Empty,
                            BrPrdAcm = data.ProductData?.BrPrdAcm?.Trim() ?? string.Empty,
                            BrPrdFacKd = data.ProductData?.BrPrdFacKd?.Trim() ?? string.Empty,
                            Pencari = data.ProductData?.Pencari?.Trim() ?? string.Empty,
                            BrJnsKd = data.ProductData?.BrJnsKd?.Trim() ?? string.Empty
                        }, transaction);
                    }

                    var checkPriceSql = "SELECT id_hrg FROM tmabrhrgjl WHERE BrPrdKd = @BrPrdKd AND BrHrgGol = @BrHrgGol AND SatKd = @SatKd";
                    var existingIdHrg = await connection.QueryFirstOrDefaultAsync<short?>(checkPriceSql, new 
                    { 
                        BrPrdKd = prdKd,
                        BrHrgGol = gol,
                        SatKd = satKd
                    }, transaction);

                    short finalIdHrg;

                    if (!existingIdHrg.HasValue)
                    {
                        var manualId = data.PriceData?.IdHrg;
                        if (!manualId.HasValue || manualId.Value <= 0)
                        {
                            throw new InvalidOperationException("ID Harga (id_hrg) wajib diisi untuk kombinasi harga baru.");
                        }

                        var checkIdSql = "SELECT COUNT(*) FROM tmabrhrgjl WHERE id_hrg = @IdHrg";
                        var countId = await connection.ExecuteScalarAsync<int>(checkIdSql, new { IdHrg = manualId.Value }, transaction);
                        if (countId > 0)
                        {
                            throw new InvalidOperationException($"ID Harga '{manualId.Value}' sudah digunakan di database. Silakan gunakan ID Harga lain.");
                        }

                        finalIdHrg = manualId.Value;

                        var insPrice = "INSERT INTO tmabrhrgjl (id_hrg, BrPrdKd, BrHrgGol, SatKd) VALUES (@IdHrg, @BrPrdKd, @BrHrgGol, @SatKd);";
                        await connection.ExecuteAsync(insPrice, new 
                        { 
                            IdHrg = finalIdHrg,
                            BrPrdKd = prdKd,
                            BrHrgGol = gol,
                            SatKd = satKd
                        }, transaction);

                        var areasSql = "SELECT id_area FROM harga_area WHERE aktif = 1";
                        var areas = await connection.QueryAsync<int>(areasSql, null, transaction);

                        if (areas.Any())
                        {
                            var insHrgJualSql = "INSERT INTO thrgjual (id_hrg, id_area) VALUES (@IdHrg, @IdArea)";
                            foreach (var areaId in areas)
                            {
                                await connection.ExecuteAsync(insHrgJualSql, new { IdHrg = finalIdHrg, IdArea = areaId }, transaction);
                            }
                        }
                    }
                    else
                    {
                        finalIdHrg = existingIdHrg.Value;
                    }

                    // 3. Handle tmabrg
                    var brKdFormatted = $"{prdKd}.{satKd}.{kdNo}";
                    var checkItemSql = "SELECT COUNT(*) FROM tmabrg WHERE BrKd = @BrKd";
                    var itemCount = await connection.ExecuteScalarAsync<int>(checkItemSql, new { BrKd = brKdFormatted }, transaction);
                    
                    if (itemCount > 0)
                    {
                        throw new InvalidOperationException($"Kode Barang '{brKdFormatted}' sudah terdaftar di master barang (tmabrg). Silakan gunakan No. Kode Barang yang berbeda.");
                    }

                    var insItem = @"
                        INSERT INTO tmabrg (BrKd, SatKd, BrPrdKd, BrKdNo, BrNm, BrHrgGol, Aktif) 
                        VALUES (@BrKd, @SatKd, @BrPrdKd, @BrKdNo, @BrNm, @BrHrgGol, 1);";
                    await connection.ExecuteAsync(insItem, new 
                    {
                        BrKd = brKdFormatted,
                        SatKd = satKd,
                        BrPrdKd = prdKd,
                        BrKdNo = kdNo,
                        BrNm = brNm,
                        BrHrgGol = gol
                    }, transaction);

                    // 4. Update request status & hasil barang jadi
                    var updateReqSql = @"
                        UPDATE req_edp_kode 
                        SET kd_prd = @KdPrd,
                            nm_prd_acc = @NmPrdAcc,
                            kd_brg = @KdBrg,
                            nm_brg_acc = @NmBrgAcc,
                            gol = @Gol,
                            sat = @Sat,
                            acc_tidak = 1,
                            acc_by = @ApproverNip,
                            acc_at = NOW(),
                            status = 'approve',
                            updated_at = NOW()
                        WHERE id = @RequestId;
                    ";
                    await connection.ExecuteAsync(updateReqSql, new 
                    { 
                        KdPrd = prdKd,
                        NmPrdAcc = prdNm,
                        KdBrg = brKdFormatted,
                        NmBrgAcc = brNm,
                        Gol = gol,
                        Sat = satKd,
                        ApproverNip = approverNip, 
                        RequestId = data.RequestId 
                    }, transaction);

                    transaction.Commit();
                    return Result<bool>.Success(true);
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"Gagal menyimpan data: {ex.Message}");
            }
        }
    }
}
