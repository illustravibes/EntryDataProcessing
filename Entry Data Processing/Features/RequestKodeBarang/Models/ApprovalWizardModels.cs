using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;

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

    public partial class ProductDataDto : ObservableObject
    {
        [ObservableProperty]
        private bool _isNewProduct;
        
        [ObservableProperty]
        private string _brPrdKd = string.Empty;

        [ObservableProperty]
        private string _brPrdNm = string.Empty;

        [ObservableProperty]
        private string _brPrdAcm = string.Empty;

        [ObservableProperty]
        private string _brPrdFacKd = string.Empty;

        [ObservableProperty]
        private string _brPrdFacNm = string.Empty;

        [ObservableProperty]
        private string _pencari = string.Empty;

        [ObservableProperty]
        private string _brJnsKd = string.Empty;

        [ObservableProperty]
        private string _brJnsNm = string.Empty;

        public string DisplayName => string.IsNullOrWhiteSpace(BrPrdKd) ? BrPrdNm : $"{BrPrdKd} - {BrPrdNm}";
        public override string ToString() => DisplayName;
    }

    public partial class PriceDataDto : ObservableObject
    {
        [ObservableProperty]
        private short? _idHrg;

        [ObservableProperty]
        private string _brHrgGol = string.Empty;

        [ObservableProperty]
        private string _satKd = string.Empty;
    }

    public partial class ItemDataDto : ObservableObject
    {
        [ObservableProperty]
        [property: StringLength(25, ErrorMessage = "Maksimal 25 karakter")]
        private string _brKdNo = string.Empty;

        [ObservableProperty]
        private string _brNm = string.Empty;
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
