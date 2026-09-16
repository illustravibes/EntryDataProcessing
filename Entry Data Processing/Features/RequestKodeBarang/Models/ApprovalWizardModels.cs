using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Entry_Data_Processing.Features.RequestKodeBarang.Models
{
    public class FactoryDto
    {
        public string BrPrdFacKd { get; set; } = string.Empty;
        public string BrPrdFacNm { get; set; } = string.Empty;
        public string DisplayText => string.IsNullOrWhiteSpace(BrPrdFacNm) ? BrPrdFacKd : BrPrdFacNm;
        public override string ToString() => DisplayText;
    }

    public class ProductTypeDto
    {
        public string BrJnsKd { get; set; } = string.Empty;
        public string BrJnsNm { get; set; } = string.Empty;
        public string DisplayText => string.IsNullOrWhiteSpace(BrJnsNm) ? BrJnsKd : BrJnsNm;
        public override string ToString() => DisplayText;
    }

    public class UnitDto
    {
        public string SatKd { get; set; } = string.Empty;
        public string SatNm { get; set; } = string.Empty;
        public string DisplayText => string.IsNullOrWhiteSpace(SatNm) ? SatKd : SatNm;
        public override string ToString() => DisplayText;
    }

    public class ProductDataDto
    {
        public bool IsNewProduct { get; set; }
        
        [Required(ErrorMessage = "Kode Produk wajib diisi")]
        [StringLength(5, ErrorMessage = "Maksimal 5 karakter")]
        public string BrPrdKd { get; set; } = string.Empty;

        public string BrPrdNm { get; set; } = string.Empty;
        public string BrPrdAcm { get; set; } = string.Empty;
        public string BrPrdFacKd { get; set; } = string.Empty;
        public string BrPrdFacNm { get; set; } = string.Empty;
        public string Pencari { get; set; } = string.Empty;
        public string BrJnsKd { get; set; } = string.Empty;
        public string BrJnsNm { get; set; } = string.Empty;

        public string DisplayName => string.IsNullOrWhiteSpace(BrPrdKd) ? BrPrdNm : $"{BrPrdKd} - {BrPrdNm}";
        public override string ToString() => DisplayName;
    }

    public class PriceDataDto
    {
        [Required(ErrorMessage = "Golongan Harga wajib diisi")]
        public string BrHrgGol { get; set; } = string.Empty;

        [Required(ErrorMessage = "Satuan wajib diisi")]
        public string SatKd { get; set; } = string.Empty;
    }

    public class ItemDataDto
    {
        [Required(ErrorMessage = "No Kode Barang wajib diisi")]
        [StringLength(10, ErrorMessage = "Maksimal 10 karakter")]
        public string BrKdNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nama Barang wajib diisi")]
        [StringLength(35, ErrorMessage = "Maksimal 35 karakter")]
        public string BrNm { get; set; } = string.Empty;
    }

    public class ApprovalWizardSubmitDto
    {
        public int RequestId { get; set; }
        public string ApproverNip { get; set; } = string.Empty;
        public ProductDataDto ProductData { get; set; } = new();
        public PriceDataDto PriceData { get; set; } = new();
        public ItemDataDto ItemData { get; set; } = new();
    }
}
