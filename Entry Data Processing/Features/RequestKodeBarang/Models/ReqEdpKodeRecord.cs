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
        
        public string AccByDisplay
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(AccByName) && !string.IsNullOrWhiteSpace(AccBy))
                    return $"{AccBy} - {AccByName}";
                if (!string.IsNullOrWhiteSpace(AccByName))
                    return AccByName;
                if (!string.IsNullOrWhiteSpace(AccBy))
                    return AccBy;
                return "-";
            }
        }

        public bool HasAccAt => AccAt.HasValue && AccAt.Value != DateTime.MinValue;
        public string FormattedTglAcc => HasAccAt ? AccAt!.Value.ToString("dd/MM/yyyy HH:mm") : "-";
        public string FormattedTglAccFull => HasAccAt ? AccAt!.Value.ToString("dd/MM/yyyy HH:mm:ss") : "-";

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

        public string SupplierKodeDisplay
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(KdSuppAcc)) return KdSuppAcc;
                if (!string.IsNullOrWhiteSpace(KdSupp)) return KdSupp;
                if (!string.IsNullOrWhiteSpace(NmSupplier) && !string.IsNullOrWhiteSpace(Supplier)) return Supplier;
                return "-";
            }
        }

        public string SupplierNamaDisplay
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(SupplierAcc)) return SupplierAcc;
                if (!string.IsNullOrWhiteSpace(NmSupplier)) return NmSupplier.Trim();
                if (!string.IsNullOrWhiteSpace(Supplier)) return Supplier.Trim();
                return "-";
            }
        }

        public bool HasKdBrg => !string.IsNullOrWhiteSpace(KdBrg);
        public bool HasKdPrd => !string.IsNullOrWhiteSpace(KdPrd);

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
        
        // Status resolution prioritizing the explicit Status column
        public string NormalizedStatus
        {
            get
            {
                var s = Status?.Trim().ToLowerInvariant() ?? string.Empty;
                if (s == "pending") return "Pending";
                if (s == "approve" || s == "approved") return "Disetujui";
                if (s == "reject" || s == "rejected") return "Ditolak";
                if (s == "draft") return "Draft";

                return AccTidak switch
                {
                    1 => "Disetujui",
                    2 => "Ditolak",
                    3 => "Draft",
                    _ => "Pending"
                };
            }
        }

        public string StatusBadgeText => NormalizedStatus;
        public string StatusLabel => StatusBadgeText;
        public bool IsRejected => NormalizedStatus == "Ditolak";
        public bool IsApproved => NormalizedStatus == "Disetujui";
        public bool IsPending => NormalizedStatus == "Pending";
        public bool CanApproveOrReject => IsPending;
    }
}
