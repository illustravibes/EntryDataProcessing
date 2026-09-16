using System;

namespace Entry_Data_Processing.Features.RequestKodeBarang.Models
{
    public class ReqEdpKodeRecord
    {
        public int Id { get; set; }
        public string? NoRequest { get; set; }
        public string? NmBrg { get; set; }
        public string? JnsBrg { get; set; }
        public string? ReadyOrMix { get; set; }
        public string? Supplier { get; set; }
        public decimal Pricelist { get; set; }
        public decimal HrgBeli { get; set; }
        public double Disc { get; set; }
        public string? FakturPajak { get; set; }
        public string? Satuan { get; set; }
        public string? Keterangan { get; set; }
        public string? FotoPath { get; set; }
        public int AccTidak { get; set; } // 0 = Pending, 1 = Approved, 2 = Rejected
        public string? AccBy { get; set; }
        public DateTime? AccAt { get; set; }
        public string? KeteranganTolak { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Virtual Display Properties
        public string StatusLabel => AccTidak switch {
            1 => "Disetujui",
            2 => "Ditolak",
            _ => "Menunggu Persetujuan"
        };
    }
}
