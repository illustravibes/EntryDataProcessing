using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Entry_Data_Processing.Features.RequestKodeBarang.Models
{
    public partial class ReqEdpKodeRecord : ObservableObject
    {
        [ObservableProperty]
        private bool _isSelected;

        public int Id { get; set; }
        public int RowNumber { get; set; }
        
        // MySQL Columns
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? NmBrg { get; set; }
        public string? JnsBrg { get; set; }
        public string? NmJns { get; set; } // From tmabrgjns
        public string? ReadyOrMix { get; set; }
        public string? Supplier { get; set; }
        public string? KdSupp { get; set; }
        public string? NmSupplier { get; set; } // From t_supplier
        public decimal Pricelist { get; set; }
        public decimal Disc { get; set; }
        public decimal HrgBeli { get; set; }
        public string? KetBeli { get; set; }
        public bool FakturPajak { get; set; }
        public string? LampiranFaktur { get; set; }
        public string? FotoBrg { get; set; }
        public string? Tkkd { get; set; }
        public string? StoreCall { get; set; } // From store
        public string? NamaToko { get; set; } // From store
        public string? IdArea { get; set; }
        public string? KdPrd { get; set; }
        public string? NmPrdAcc { get; set; }
        public string? KdBrg { get; set; }
        public string? NmBrgAcc { get; set; }
        public string? Gol { get; set; }
        public string? Sat { get; set; }
        public string? SupplierAcc { get; set; }
        public string? KdSuppAcc { get; set; }
        public int AccTidak { get; set; } // 1 = Approve, 2 = Reject, 3 = Draft, 0 = Pending
        public string? Status { get; set; } // draft, pending, approve, reject
        public string? ChangedBy { get; set; }
        public DateTime? ChangedAt { get; set; }
        public string? AccBy { get; set; }
        public string? AccByName { get; set; }
        public DateTime? AccAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? CreatedByName { get; set; }
        public string? KeteranganTolak { get; set; }
        
        // Virtual Display Properties
        public string PengajuDisplay => !string.IsNullOrWhiteSpace(CreatedByName) 
            ? CreatedByName 
            : (!string.IsNullOrWhiteSpace(CreatedBy) ? CreatedBy : "-");
        
        public string AccByDisplay => !string.IsNullOrWhiteSpace(AccByName) 
            ? AccByName 
            : (!string.IsNullOrWhiteSpace(AccBy) ? AccBy : "-");

        public bool IsNewSupplier => string.IsNullOrWhiteSpace(KdSupp) && string.IsNullOrWhiteSpace(NmSupplier);
        public string FormattedTglRequest => CreatedAt.HasValue && CreatedAt.Value != DateTime.MinValue ? CreatedAt.Value.ToString("dd/MM/yyyy HH:mm") : "-";
        
        // Toko: "01/01 - HEAD OFFICE"
        public string TokoNamaDisplay => !string.IsNullOrWhiteSpace(StoreCall) 
            ? StoreCall 
            : (!string.IsNullOrWhiteSpace(NamaToko) ? NamaToko : "HEAD OFFICE");
            
        public string TokoKodeAndName => !string.IsNullOrWhiteSpace(Tkkd)
            ? $"{Tkkd} - {TokoNamaDisplay}"
            : TokoNamaDisplay;

        public string AreaBadge => !string.IsNullOrWhiteSpace(IdArea) ? IdArea : "JGJ";

        // Jenis: "Cat Genteng" (fallback to JnsBrg)
        public string JenisDisplay => !string.IsNullOrWhiteSpace(NmJns) ? NmJns : (JnsBrg ?? "-");

        // Supplier: "S00148 - AA"
        public string SupplierDisplay => !string.IsNullOrWhiteSpace(NmSupplier) && !string.IsNullOrWhiteSpace(Supplier)
            ? $"{Supplier} - {NmSupplier}"
            : (!string.IsNullOrWhiteSpace(Supplier) ? Supplier : "-");

        // Prices & Discount
        public string FormattedPricelist => Pricelist > 0 ? Pricelist.ToString("N0", new System.Globalization.CultureInfo("id-ID")) : "0";
        public string FormattedDisc => Disc > 0 ? $"{Disc:N0}%" : "0%";
        public bool HasDiscount => Disc > 0;
        public string DiscBadgeText => Disc > 0 ? $"{Disc:N0}%" : "0%";
        public string DiscBadgeBackground => HasDiscount ? "#EFF6FF" : "#F8FAFC";
        public string DiscBadgeBorder => HasDiscount ? "#BFDBFE" : "#E2E8F0";
        public string DiscBadgeForeground => HasDiscount ? "#0284C7" : "#94A3B8";
        public string FormattedHrgBeli => HrgBeli > 0 ? HrgBeli.ToString("N0", new System.Globalization.CultureInfo("id-ID")) : "0";
        
        // Ready / Mix badge
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
        public bool IsRejected => AccTidak == 2 || string.Equals(Status, "reject", StringComparison.OrdinalIgnoreCase);
        public bool CanApproveOrReject => AccTidak == 0 || Status == "pending";
    }
}
