using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Entry_Data_Processing.Core.Common;
using Entry_Data_Processing.Core.Session;
using Entry_Data_Processing.Features.RequestKodeBarang.Models;
using Entry_Data_Processing.Features.RequestKodeBarang.Services;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace Entry_Data_Processing.Features.RequestKodeBarang.ViewModels
{
    public partial class ApprovalWizardViewModel : ViewModelBase
    {
        private readonly IReqEdpKodeService _service;
        private readonly IUserSession _userSession;
        private readonly ISnackbarService _snackbarService;
        private readonly IContentDialogService _contentDialogService;
        
        public int RequestId { get; set; }

        [ObservableProperty]
        private int _currentStepIndex = 0;

        [ObservableProperty]
        private bool _isLoading;

        // Step 1 Data
        [ObservableProperty]
        private ProductDataDto _productData = new();
        
        [ObservableProperty]
        private string _productSearchQuery = string.Empty;

        public ObservableCollection<ProductDataDto> ProductSearchResults { get; } = new();
        
        [ObservableProperty]
        private ProductDataDto? _selectedProduct;

        // Factory Search for New Product Mode
        [ObservableProperty]
        private string _factorySearchQuery = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasSelectedFactory))]
        private FactoryDto? _selectedFactory;

        public bool HasSelectedFactory => SelectedFactory != null;

        public ObservableCollection<FactoryDto> FactorySearchResults { get; } = new();

        // Product Type (Jenis) Search for New Product Mode
        [ObservableProperty]
        private string _productTypeSearchQuery = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasSelectedProductType))]
        private ProductTypeDto? _selectedProductType;

        public bool HasSelectedProductType => SelectedProductType != null;

        public ObservableCollection<ProductTypeDto> ProductTypeSearchResults { get; } = new();

        // Step 2 Data
        [ObservableProperty]
        private PriceDataDto _priceData = new();

        [ObservableProperty]
        private string _priceGroupSearchQuery = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasSelectedPriceGroup))]
        private string _selectedPriceGroup = string.Empty;

        public bool HasSelectedPriceGroup => !string.IsNullOrWhiteSpace(PriceData.BrHrgGol) || !string.IsNullOrWhiteSpace(SelectedPriceGroup);

        public ObservableCollection<string> PriceGroupSearchResults { get; } = new();

        [ObservableProperty]
        private string _unitSearchQuery = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasSelectedUnit))]
        private UnitDto? _selectedUnit;

        public bool HasSelectedUnit => SelectedUnit != null || !string.IsNullOrWhiteSpace(PriceData.SatKd);

        public ObservableCollection<UnitDto> UnitSearchResults { get; } = new();

        // Step 3 Data
        [ObservableProperty]
        private ItemDataDto _itemData = new();

        // Control properties
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanGoPrevious))]
        private bool _isFirstStep = true;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanGoNext))]
        private bool _isLastStep = false;

        public bool CanGoPrevious => !IsFirstStep && !IsLoading;
        public bool CanGoNext => !IsLoading;

        [ObservableProperty]
        private bool _isNewProductMode = false;

        [ObservableProperty]
        private bool _hasSelectedProduct = false;

        public ApprovalWizardViewModel(
            IReqEdpKodeService service,
            IUserSession userSession,
            ISnackbarService snackbarService,
            IContentDialogService contentDialogService)
        {
            _service = service;
            _userSession = userSession;
            _snackbarService = snackbarService;
            _contentDialogService = contentDialogService;
        }

        public async Task InitializeAsync(int requestId)
        {
            RequestId = requestId;
            CurrentStepIndex = 0;
            IsNewProductMode = false;
            HasSelectedProduct = false;
            SelectedProduct = null;
            SelectedFactory = null;
            SelectedProductType = null;
            SelectedUnit = null;
            SelectedPriceGroup = string.Empty;
            UpdateStepState();
            
            IsLoading = true;
            try
            {
                var req = await _service.GetRequestByIdAsync(requestId);
                if (req != null)
                {
                    ItemData.BrNm = req.NmBrg ?? string.Empty;
                    ProductData.BrPrdNm = req.NmBrg ?? string.Empty;
                }

                ProductSearchQuery = string.Empty;
                await SearchProductsAsync();

                FactorySearchQuery = string.Empty;
                await SearchFactoriesAsync();

                ProductTypeSearchQuery = string.Empty;
                await SearchProductTypesAsync();

                PriceGroupSearchQuery = string.Empty;
                await SearchPriceGroupsAsync();

                UnitSearchQuery = string.Empty;
                await SearchUnitsAsync();
            }
            finally
            {
                IsLoading = false;
            }
        }

        partial void OnIsNewProductModeChanged(bool value)
        {
            ProductData.IsNewProduct = value;
            if (value)
            {
                SelectedProduct = null;
                HasSelectedProduct = false;
                ProductData.BrPrdKd = string.Empty;
                if (string.IsNullOrWhiteSpace(ProductData.BrPrdNm))
                {
                    ProductData.BrPrdNm = ItemData.BrNm;
                }
            }
            else
            {
                if (SelectedProduct != null)
                {
                    HasSelectedProduct = true;
                    ProductData.BrPrdKd = SelectedProduct.BrPrdKd;
                    ProductData.BrPrdNm = SelectedProduct.BrPrdNm;
                    ProductData.BrPrdFacKd = SelectedProduct.BrPrdFacKd;
                    ProductData.BrPrdFacNm = SelectedProduct.BrPrdFacNm;
                    ProductData.BrJnsKd = SelectedProduct.BrJnsKd;
                    ProductData.BrJnsNm = SelectedProduct.BrJnsNm;
                }
                else
                {
                    HasSelectedProduct = false;
                    ProductData.BrPrdKd = string.Empty;
                }
            }
        }

        [RelayCommand]
        public void SetExistingProductMode()
        {
            IsNewProductMode = false;
        }

        [RelayCommand]
        public void SetNewProductMode()
        {
            IsNewProductMode = true;
        }

        partial void OnProductSearchQueryChanged(string value)
        {
            _ = SearchProductsAsync();
        }

        [RelayCommand]
        public async Task SearchProductsAsync()
        {
            var q = ProductSearchQuery?.Trim() ?? string.Empty;
            var results = await _service.SearchProductsAsync(q);
            
            if (System.Windows.Application.Current?.Dispatcher != null)
            {
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ProductSearchResults.Clear();
                    foreach (var r in results)
                    {
                        ProductSearchResults.Add(r);
                    }
                });
            }
        }

        [RelayCommand]
        public void SelectProduct(ProductDataDto? product)
        {
            if (product == null) return;
            SelectedProduct = product;
            ProductData = new ProductDataDto
            {
                BrPrdKd = product.BrPrdKd,
                BrPrdNm = product.BrPrdNm,
                BrPrdAcm = product.BrPrdAcm,
                BrPrdFacKd = product.BrPrdFacKd,
                BrPrdFacNm = product.BrPrdFacNm,
                BrJnsKd = product.BrJnsKd,
                BrJnsNm = product.BrJnsNm,
                Pencari = product.Pencari,
                IsNewProduct = false
            };
            HasSelectedProduct = true;
            IsNewProductMode = false;
        }

        [RelayCommand]
        public void ClearSelectedProduct()
        {
            SelectedProduct = null;
            HasSelectedProduct = false;
            ProductData.BrPrdKd = string.Empty;
        }

        // Factory Selection Commands
        partial void OnSelectedFactoryChanged(FactoryDto? value)
        {
            if (value != null)
            {
                ProductData.BrPrdFacKd = value.BrPrdFacKd;
                ProductData.BrPrdFacNm = value.BrPrdFacNm;
            }
            else
            {
                ProductData.BrPrdFacKd = string.Empty;
                ProductData.BrPrdFacNm = string.Empty;
            }
        }

        partial void OnSelectedProductTypeChanged(ProductTypeDto? value)
        {
            if (value != null)
            {
                ProductData.BrJnsKd = value.BrJnsKd;
                ProductData.BrJnsNm = value.BrJnsNm;
            }
            else
            {
                ProductData.BrJnsKd = string.Empty;
                ProductData.BrJnsNm = string.Empty;
            }
        }

        partial void OnSelectedPriceGroupChanged(string value)
        {
            PriceData.BrHrgGol = value ?? string.Empty;
        }

        partial void OnSelectedUnitChanged(UnitDto? value)
        {
            if (value != null)
            {
                PriceData.SatKd = value.SatKd;
            }
            else
            {
                PriceData.SatKd = string.Empty;
            }
        }

        partial void OnFactorySearchQueryChanged(string value)
        {
            _ = SearchFactoriesAsync();
        }

        [RelayCommand]
        public async Task SearchFactoriesAsync()
        {
            var q = FactorySearchQuery?.Trim() ?? string.Empty;
            var results = await _service.SearchFactoriesAsync(q);
            if (System.Windows.Application.Current?.Dispatcher != null)
            {
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    FactorySearchResults.Clear();
                    foreach (var r in results)
                    {
                        FactorySearchResults.Add(r);
                    }
                });
            }
        }

        [RelayCommand]
        public void SelectFactory(FactoryDto? factory)
        {
            if (factory == null) return;
            SelectedFactory = factory;
            ProductData.BrPrdFacKd = factory.BrPrdFacKd;
            ProductData.BrPrdFacNm = factory.BrPrdFacNm;
        }

        [RelayCommand]
        public void ClearSelectedFactory()
        {
            SelectedFactory = null;
            ProductData.BrPrdFacKd = string.Empty;
            ProductData.BrPrdFacNm = string.Empty;
            FactorySearchQuery = string.Empty;
            _ = SearchFactoriesAsync();
        }

        // Product Type (Jenis) Selection Commands
        partial void OnProductTypeSearchQueryChanged(string value)
        {
            _ = SearchProductTypesAsync();
        }

        [RelayCommand]
        public async Task SearchProductTypesAsync()
        {
            var q = ProductTypeSearchQuery?.Trim() ?? string.Empty;
            var results = await _service.SearchProductTypesAsync(q);
            if (System.Windows.Application.Current?.Dispatcher != null)
            {
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ProductTypeSearchResults.Clear();
                    foreach (var r in results)
                    {
                        ProductTypeSearchResults.Add(r);
                    }
                });
            }
        }

        [RelayCommand]
        public void SelectProductType(ProductTypeDto? type)
        {
            if (type == null) return;
            SelectedProductType = type;
            ProductData.BrJnsKd = type.BrJnsKd;
            ProductData.BrJnsNm = type.BrJnsNm;
        }

        [RelayCommand]
        public void ClearSelectedProductType()
        {
            SelectedProductType = null;
            ProductData.BrJnsKd = string.Empty;
            ProductData.BrJnsNm = string.Empty;
            ProductTypeSearchQuery = string.Empty;
            _ = SearchProductTypesAsync();
        }

        // Price Group Selection Commands
        partial void OnPriceGroupSearchQueryChanged(string value)
        {
            _ = SearchPriceGroupsAsync();
        }

        [RelayCommand]
        public async Task SearchPriceGroupsAsync()
        {
            var q = PriceGroupSearchQuery?.Trim() ?? string.Empty;
            var results = await _service.SearchPriceGroupsAsync(q);
            if (System.Windows.Application.Current?.Dispatcher != null)
            {
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    PriceGroupSearchResults.Clear();
                    foreach (var r in results)
                    {
                        PriceGroupSearchResults.Add(r);
                    }
                });
            }
        }

        [RelayCommand]
        public void SelectPriceGroup(string? group)
        {
            if (string.IsNullOrWhiteSpace(group)) return;
            SelectedPriceGroup = group;
            PriceData.BrHrgGol = group;
        }

        [RelayCommand]
        public void ClearSelectedPriceGroup()
        {
            SelectedPriceGroup = string.Empty;
            PriceData.BrHrgGol = string.Empty;
            PriceGroupSearchQuery = string.Empty;
            _ = SearchPriceGroupsAsync();
        }

        // Unit Selection Commands
        partial void OnUnitSearchQueryChanged(string value)
        {
            _ = SearchUnitsAsync();
        }

        [RelayCommand]
        public async Task SearchUnitsAsync()
        {
            var q = UnitSearchQuery?.Trim() ?? string.Empty;
            var results = await _service.SearchUnitsAsync(q);
            
            if (System.Windows.Application.Current?.Dispatcher != null)
            {
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    UnitSearchResults.Clear();
                    foreach (var r in results)
                    {
                        UnitSearchResults.Add(r);
                    }
                });
            }
        }

        [RelayCommand]
        public void SelectUnit(UnitDto? unit)
        {
            if (unit == null) return;
            SelectedUnit = unit;
            PriceData.SatKd = unit.SatKd;
        }

        [RelayCommand]
        public void ClearSelectedUnit()
        {
            SelectedUnit = null;
            PriceData.SatKd = string.Empty;
            UnitSearchQuery = string.Empty;
            _ = SearchUnitsAsync();
        }

        [RelayCommand]
        private void PrepareNewProduct()
        {
            IsNewProductMode = true;
        }

        [RelayCommand]
        private async Task NextStepAsync()
        {
            if (await ValidateCurrentStepAsync())
            {
                if (CurrentStepIndex < 2)
                {
                    CurrentStepIndex++;
                    UpdateStepState();
                }
            }
        }

        [RelayCommand]
        private void PreviousStep()
        {
            if (CurrentStepIndex > 0)
            {
                CurrentStepIndex--;
                UpdateStepState();
            }
        }

        private void UpdateStepState()
        {
            IsFirstStep = CurrentStepIndex == 0;
            IsLastStep = CurrentStepIndex == 2;
        }

        private async Task<bool> ValidateCurrentStepAsync()
        {
            if (CurrentStepIndex == 0)
            {
                if (!IsNewProductMode)
                {
                    // Strict Segment 1: Must choose an existing registered product
                    if (!HasSelectedProduct || SelectedProduct == null || string.IsNullOrWhiteSpace(ProductData.BrPrdKd))
                    {
                        _snackbarService.Show("Validasi", "Silakan pilih salah satu produk terdaftar terlebih dahulu.", ControlAppearance.Danger, null, System.TimeSpan.FromSeconds(3));
                        return false;
                    }
                    ProductData.IsNewProduct = false;
                    return true;
                }
                else
                {
                    // Strict Segment 2: Must create new product, code must NOT duplicate existing products
                    var kd = ProductData.BrPrdKd?.Trim();
                    if (string.IsNullOrWhiteSpace(kd))
                    {
                        _snackbarService.Show("Validasi", "Kode Produk baru wajib diisi.", ControlAppearance.Danger, null, System.TimeSpan.FromSeconds(3));
                        return false;
                    }
                    if (string.IsNullOrWhiteSpace(ProductData.BrPrdNm))
                    {
                        _snackbarService.Show("Validasi", "Nama Produk baru wajib diisi.", ControlAppearance.Danger, null, System.TimeSpan.FromSeconds(3));
                        return false;
                    }

                    // Check if code already exists in tmabrprd
                    var exists = await _service.CheckProductExistsAsync(kd);
                    if (exists)
                    {
                        _snackbarService.Show("Kode Produk Duplikat", $"Kode Produk '{kd}' sudah terdaftar di database! Tidak boleh sama saat membuat master produk baru.", ControlAppearance.Danger, null, System.TimeSpan.FromSeconds(4));
                        return false;
                    }

                    ProductData.BrPrdKd = kd;
                    ProductData.IsNewProduct = true;
                    return true;
                }
            }
            if (CurrentStepIndex == 1)
            {
                if (string.IsNullOrWhiteSpace(PriceData.BrHrgGol) || string.IsNullOrWhiteSpace(PriceData.SatKd))
                {
                    _snackbarService.Show("Validasi", "Golongan Harga dan Satuan wajib diisi", ControlAppearance.Danger, null, System.TimeSpan.FromSeconds(3));
                    return false;
                }
                return true;
            }
            return true;
        }

        public async Task<bool> SubmitApprovalAsync()
        {
            if (!await ValidateCurrentStepAsync()) return false;

            if (string.IsNullOrWhiteSpace(ItemData.BrKdNo) || string.IsNullOrWhiteSpace(ItemData.BrNm))
            {
                _snackbarService.Show("Validasi", "No Kode Barang dan Nama Barang wajib diisi", ControlAppearance.Danger, null, System.TimeSpan.FromSeconds(3));
                return false;
            }

            var brKdFormatted = $"{ProductData.BrPrdKd.Trim()}.{PriceData.SatKd.Trim()}.{ItemData.BrKdNo.Trim()}";
            var itemExists = await _service.CheckItemExistsAsync(brKdFormatted);
            if (itemExists)
            {
                _snackbarService.Show("Kode Barang Duplikat", $"Kode Barang '{brKdFormatted}' sudah ada di database. Silakan gunakan No Kode Barang yang berbeda.", ControlAppearance.Danger, null, System.TimeSpan.FromSeconds(4));
                return false;
            }

            IsLoading = true;
            
            var submitDto = new ApprovalWizardSubmitDto
            {
                RequestId = RequestId,
                ApproverNip = _userSession.CurrentUser?.Nip ?? "UNKNOWN",
                ProductData = ProductData,
                PriceData = PriceData,
                ItemData = ItemData
            };

            var result = await _service.ProcessWizardApprovalAsync(submitDto);
            
            IsLoading = false;

            if (result.IsSuccess)
            {
                _snackbarService.Show("Sukses", "Permohonan berhasil disetujui", ControlAppearance.Success, null, System.TimeSpan.FromSeconds(3));
                return true;
            }
            else
            {
                _snackbarService.Show("Error", result.ErrorMessage ?? "Gagal menyimpan", ControlAppearance.Danger, null, System.TimeSpan.FromSeconds(3));
                return false;
            }
        }
    }
}
