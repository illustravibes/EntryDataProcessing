using System;

namespace Entry_Data_Processing.Features.RequestKodeBarang.Models
{
    public class ReqEdpKodeRecord
    {
        public int Id { get; set; }
        public int RowNumber { get; set; }
        
        // MySQL Columns
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? NmBrg { get; set; }
        public string? JnsBrg { get; set; }
        public string? ReadyOrMix { get; set; }
        public string? Supplier { get; set; }
        public string? KdSupp { get; set; }
        public decimal Pricelist { get; set; }
        public decimal Disc { get; set; }
        public decimal HrgBeli { get; set; }
        public string? KetBeli { get; set; }
        public bool FakturPajak { get; set; }
        public string? LampiranFaktur { get; set; }
        public string? FotoBrg { get; set; }
        public string? Tkkd { get; set; }
        public string? IdArea { get; set; }
        public string? KdPrd { get; set; }
        public string? NmPrdAcc { get; set; }
        public string? KdBrg { get; set; }
        public string? NmBrgAcc { get; set; }
        public string? Gol { get; set; }
        public string? Sat { get; set; }
        public string? SupplierAcc { get; set; }
        public string? KdSuppAcc { get; set; }
        public int AccTidak { get; set; } // 1 = Approve, 2 = Reject, 3 = Draft/Pending, 0 = Pending
        public string? Status { get; set; } // draft, pending, approve, reject
        public string? ChangedBy { get; set; }
        public DateTime? ChangedAt { get; set; }
        public string? AccBy { get; set; }
        public DateTime? AccAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? KeteranganTolak { get; set; }
        
        // Virtual / Display Properties
        public bool IsNewSupplier => string.IsNullOrWhiteSpace(KdSupp);
        public string FormattedTglRequest => CreatedAt.HasValue && CreatedAt.Value != DateTime.MinValue ? CreatedAt.Value.ToString("dd/MM/yyyy") : "-";
        public string TokoDisplay => !string.IsNullOrWhiteSpace(Tkkd) ? $"Toko {Tkkd}" : "Toko Central";
        public string AreaDisplay => !string.IsNullOrWhiteSpace(IdArea) ? IdArea : "Central";
        public string FormattedPricelist => Pricelist > 0 ? Pricelist.ToString("N0", new System.Globalization.CultureInfo("id-ID")) : "0";
        public string FormattedHrgBeli => HrgBeli > 0 ? HrgBeli.ToString("N0", new System.Globalization.CultureInfo("id-ID")) : "0";
        public string RmBadgeText => !string.IsNullOrWhiteSpace(ReadyOrMix) ? ReadyOrMix.ToUpper() : "READY";
        public bool IsMix => RmBadgeText.Contains("MIX");
        
        public string StatusBadgeText => AccTidak switch
        {
            1 => "Disetujui",
            2 => "Ditolak",
            0 => "Pending",
            3 => "Draft",
            _ => !string.IsNullOrWhiteSpace(Status) ? Status.ToUpper() : "Draft"
        };
        public string StatusLabel => StatusBadgeText;
    }
}
