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

        public event EventHandler? ApprovalSubmitted;
        public event EventHandler? CancelRequested;
        
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

        // Live validation for New Product Mode
        [ObservableProperty]
        private string _newProductCode = string.Empty;

        [ObservableProperty]
        private bool _isCheckingProductCode = false;

        [ObservableProperty]
        private bool? _isProductCodeValid = null;

        [ObservableProperty]
        private string _productCodeValidationMessage = string.Empty;

        private System.Threading.CancellationTokenSource? _productCodeCts;

        partial void OnNewProductCodeChanged(string value)
        {
            var kd = value?.Trim().ToUpper() ?? string.Empty;
            ProductData.BrPrdKd = kd;

            _productCodeCts?.Cancel();
            _productCodeCts?.Dispose();
            _productCodeCts = new System.Threading.CancellationTokenSource();
            var token = _productCodeCts.Token;

            if (string.IsNullOrWhiteSpace(kd))
            {
                IsCheckingProductCode = false;
                IsProductCodeValid = null;
                ProductCodeValidationMessage = "Masukkan kode produk (maks. 5 karakter)";
                return;
            }

            _ = ValidateProductCodeLiveAsync(kd, token);
        }

        private async System.Threading.Tasks.Task ValidateProductCodeLiveAsync(string code, System.Threading.CancellationToken token)
        {
            try
            {
                IsCheckingProductCode = true;
                ProductCodeValidationMessage = "Memeriksa ketersediaan kode...";

                await System.Threading.Tasks.Task.Delay(350, token);
                if (token.IsCancellationRequested) return;

                var exists = await _service.CheckProductExistsAsync(code);
                if (token.IsCancellationRequested) return;

                if (exists)
                {
                    IsProductCodeValid = false;
                    ProductCodeValidationMessage = $"Kode '{code}' sudah digunakan di master produk. Gunakan kode lain.";
                }
                else
                {
                    IsProductCodeValid = true;
                    ProductCodeValidationMessage = $"Kode '{code}' tersedia dan dapat digunakan.";
                }
            }
            catch (System.OperationCanceledException)
            {
            }
            catch (System.Exception ex)
            {
                IsProductCodeValid = null;
                ProductCodeValidationMessage = "Gagal memverifikasi kode: " + ex.Message;
            }
            finally
            {
                if (!token.IsCancellationRequested)
                {
                    IsCheckingProductCode = false;
                }
            }
        }

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
        [NotifyPropertyChangedFor(nameof(IsNotLastStep))]
        private bool _isLastStep = false;

        public bool CanGoPrevious => !IsFirstStep && !IsLoading;
        public bool CanGoNext => !IsLoading;
        public bool IsNotLastStep => !IsLastStep;

        public bool IsStep1Done => CurrentStepIndex > 0;
        public bool IsStep2Done => CurrentStepIndex > 1;

        [ObservableProperty]
        private bool? _isPriceCombinationExisting = null;

        [ObservableProperty]
        private string _newIdHrgText = string.Empty;

        partial void OnNewIdHrgTextChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                PriceData.IdHrg = null;
                return;
            }

            var digits = new string(value.Where(char.IsDigit).ToArray());
            if (digits != value)
            {
                NewIdHrgText = digits;
                return;
            }

            if (short.TryParse(digits, out var parsed))
            {
                PriceData.IdHrg = parsed;
            }
            else
            {
                PriceData.IdHrg = null;
            }
        }

        [ObservableProperty]
        private bool _isNewProductMode = false;

        [ObservableProperty]
        private bool _hasSelectedProduct = false;

        [ObservableProperty]
        private bool _hasValidationWarning = false;

        [ObservableProperty]
        private string _validationWarningTitle = string.Empty;

        [ObservableProperty]
        private string _validationWarningMessage = string.Empty;

        private string _productSelectionError = string.Empty;
        public string ProductSelectionError { get => _productSelectionError; set => SetProperty(ref _productSelectionError, value); }

        private string _productCodeError = string.Empty;
        public string ProductCodeError { get => _productCodeError; set => SetProperty(ref _productCodeError, value); }

        private string _productNameError = string.Empty;
        public string ProductNameError { get => _productNameError; set => SetProperty(ref _productNameError, value); }

        private string _productAcronymError = string.Empty;
        public string ProductAcronymError { get => _productAcronymError; set => SetProperty(ref _productAcronymError, value); }

        private string _factoryError = string.Empty;
        public string FactoryError { get => _factoryError; set => SetProperty(ref _factoryError, value); }

        private string _productTypeError = string.Empty;
        public string ProductTypeError { get => _productTypeError; set => SetProperty(ref _productTypeError, value); }

        private string _requesterError = string.Empty;
        public string RequesterError { get => _requesterError; set => SetProperty(ref _requesterError, value); }

        private string _priceGroupError = string.Empty;
        public string PriceGroupError { get => _priceGroupError; set => SetProperty(ref _priceGroupError, value); }

        private string _unitError = string.Empty;
        public string UnitError { get => _unitError; set => SetProperty(ref _unitError, value); }

        private string _priceIdError = string.Empty;
        public string PriceIdError { get => _priceIdError; set => SetProperty(ref _priceIdError, value); }

        private string _itemCodeError = string.Empty;
        public string ItemCodeError { get => _itemCodeError; set => SetProperty(ref _itemCodeError, value); }

        private string _itemNameError = string.Empty;
        public string ItemNameError { get => _itemNameError; set => SetProperty(ref _itemNameError, value); }

        private void ClearFieldErrors()
        {
            ProductSelectionError = string.Empty;
            ProductCodeError = string.Empty;
            ProductNameError = string.Empty;
            ProductAcronymError = string.Empty;
            FactoryError = string.Empty;
            ProductTypeError = string.Empty;
            RequesterError = string.Empty;
            PriceGroupError = string.Empty;
            UnitError = string.Empty;
            PriceIdError = string.Empty;
            ItemCodeError = string.Empty;
            ItemNameError = string.Empty;
        }

        [RelayCommand]
        public void ClearValidationWarning()
        {
            HasValidationWarning = false;
            ValidationWarningTitle = string.Empty;
            ValidationWarningMessage = string.Empty;
        }

        public void SetValidationWarning(string title, string message)
        {
            ValidationWarningTitle = title;
            ValidationWarningMessage = message;
            HasValidationWarning = true;

            _snackbarService.Show(title, message, ControlAppearance.Danger, null, System.TimeSpan.FromSeconds(4));
        }

        public ApprovalWizardViewModel(
            IReqEdpKodeService service,
            IUserSession userSession,
            ISnackbarService snackbarService)
        {
            _service = service;
            _userSession = userSession;
            _snackbarService = snackbarService;
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
            NewIdHrgText = string.Empty;
            PriceData.IdHrg = null;
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
                SelectedPriceGroup = string.Empty;
                PriceData.BrHrgGol = string.Empty;
                SelectedUnit = null;
                PriceData.SatKd = string.Empty;
                IsPriceCombinationExisting = null;
                NewIdHrgText = string.Empty;
                PriceData.IdHrg = null;
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
                    _ = LoadDefaultPriceCombinationAsync(SelectedProduct.BrPrdKd);
                }
                else
                {
                    HasSelectedProduct = false;
                    ProductData.BrPrdKd = string.Empty;
                    SelectedPriceGroup = string.Empty;
                    PriceData.BrHrgGol = string.Empty;
                    SelectedUnit = null;
                    PriceData.SatKd = string.Empty;
                    IsPriceCombinationExisting = null;
                    NewIdHrgText = string.Empty;
                    PriceData.IdHrg = null;
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
            _ = RunSearchSafelyAsync(SearchProductsAsync);
        }

        private async Task RunSearchSafelyAsync(Func<Task> searchOperation)
        {
            try
            {
                await searchOperation();
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                SetValidationWarning("Pencarian Gagal", ex.Message);
            }
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

            _ = LoadDefaultPriceCombinationAsync(product.BrPrdKd);
        }

        [RelayCommand]
        public void ClearSelectedProduct()
        {
            SelectedProduct = null;
            HasSelectedProduct = false;
            ProductData.BrPrdKd = string.Empty;
            SelectedPriceGroup = string.Empty;
            PriceData.BrHrgGol = string.Empty;
            SelectedUnit = null;
            PriceData.SatKd = string.Empty;
            IsPriceCombinationExisting = null;
            NewIdHrgText = string.Empty;
            PriceData.IdHrg = null;
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
            _ = CheckPriceCombinationStatusAsync();
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
            _ = CheckPriceCombinationStatusAsync();
        }

        public async Task CheckPriceCombinationStatusAsync()
        {
            var prdKd = ProductData.BrPrdKd?.Trim() ?? string.Empty;
            var gol = PriceData.BrHrgGol?.Trim() ?? string.Empty;
            var sat = PriceData.SatKd?.Trim() ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(prdKd) && 
                !string.IsNullOrWhiteSpace(gol) && 
                !string.IsNullOrWhiteSpace(sat))
            {
                var exists = await _service.CheckPriceCombinationExistsAsync(prdKd, gol, sat);
                IsPriceCombinationExisting = exists;
                if (exists)
                {
                    NewIdHrgText = string.Empty;
                    PriceData.IdHrg = null;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(NewIdHrgText))
                    {
                        var nextId = await _service.GetNextIdHrgAsync();
                        NewIdHrgText = nextId.ToString();
                        PriceData.IdHrg = nextId;
                    }
                }
            }
            else
            {
                IsPriceCombinationExisting = null;
                NewIdHrgText = string.Empty;
                PriceData.IdHrg = null;
            }
        }

        partial void OnFactorySearchQueryChanged(string value)
        {
            _ = RunSearchSafelyAsync(SearchFactoriesAsync);
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
            _ = RunSearchSafelyAsync(SearchFactoriesAsync);
        }

        // Product Type (Jenis) Selection Commands
        partial void OnProductTypeSearchQueryChanged(string value)
        {
            _ = RunSearchSafelyAsync(SearchProductTypesAsync);
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
            _ = RunSearchSafelyAsync(SearchProductTypesAsync);
        }

        partial void OnSelectedProductChanged(ProductDataDto? value)
        {
            ClearValidationWarning();
        }

        // Price Group Selection Commands
        partial void OnPriceGroupSearchQueryChanged(string value)
        {
            _ = RunSearchSafelyAsync(SearchPriceGroupsAsync);
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
            _ = RunSearchSafelyAsync(SearchPriceGroupsAsync);
        }

        // Unit Selection Commands
        partial void OnUnitSearchQueryChanged(string value)
        {
            _ = RunSearchSafelyAsync(SearchUnitsAsync);
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
            _ = RunSearchSafelyAsync(SearchUnitsAsync);
        }

        [RelayCommand]
        private void PrepareNewProduct()
        {
            IsNewProductMode = true;
            NewProductCode = string.Empty;
            IsProductCodeValid = null;
            ProductCodeValidationMessage = "Ketik kode produk di atas untuk memeriksa ketersediaan.";
            ProductData = new ProductDataDto
            {
                IsNewProduct = true
            };
        }

        [RelayCommand]
        private void CancelNewProduct()
        {
            IsNewProductMode = false;
            NewProductCode = string.Empty;
            IsProductCodeValid = null;
            ProductCodeValidationMessage = string.Empty;
        }

        [RelayCommand]
        public async Task NextStepAsync()
        {
            try
            {
                if (await ValidateCurrentStepAsync() && CurrentStepIndex < 2)
                {
                    CurrentStepIndex++;
                    UpdateStepState();
                }
            }
            catch (Exception ex)
            {
                SetValidationWarning("Terjadi Kesalahan", $"Gagal memvalidasi data: {ex.Message}");
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            CancelRequested?.Invoke(this, EventArgs.Empty);
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
            OnPropertyChanged(nameof(IsStep1Done));
            OnPropertyChanged(nameof(IsStep2Done));
            if (CurrentStepIndex == 1)
            {
                if (!IsNewProductMode && !string.IsNullOrWhiteSpace(ProductData.BrPrdKd))
                {
                    _ = LoadDefaultPriceCombinationAsync(ProductData.BrPrdKd);
                }
                else
                {
                    _ = CheckPriceCombinationStatusAsync();
                }
            }
            else if (CurrentStepIndex == 2)
            {
                if (!string.IsNullOrWhiteSpace(SelectedPriceGroup))
                {
                    PriceData.BrHrgGol = SelectedPriceGroup.Trim();
                }
                if (SelectedUnit != null)
                {
                    PriceData.SatKd = SelectedUnit.SatKd.Trim();
                }
                if (ItemData is not null && ProductData is not null && string.IsNullOrWhiteSpace(ItemData.BrNm))
                {
                    ItemData.BrNm = ProductData.BrPrdNm;
                }

                OnPropertyChanged(nameof(PriceData));
                OnPropertyChanged(nameof(ProductData));
                OnPropertyChanged(nameof(ItemData));
            }
        }

        public async Task LoadDefaultPriceCombinationAsync(string brPrdKd)
        {
            if (string.IsNullOrWhiteSpace(brPrdKd)) return;

            try
            {
                var existingPrice = await _service.GetDefaultPriceCombinationForProductAsync(brPrdKd);
                if (existingPrice != null)
                {
                    void ApplyPrice()
                    {
                        if (!string.IsNullOrWhiteSpace(existingPrice.BrHrgGol))
                        {
                            SelectedPriceGroup = existingPrice.BrHrgGol;
                            PriceData.BrHrgGol = existingPrice.BrHrgGol;
                        }

                        if (!string.IsNullOrWhiteSpace(existingPrice.SatKd))
                        {
                            PriceData.SatKd = existingPrice.SatKd;
                            var unit = UnitSearchResults.FirstOrDefault(u => 
                                string.Equals(u.SatKd, existingPrice.SatKd, StringComparison.OrdinalIgnoreCase));
                            if (unit != null)
                            {
                                SelectedUnit = unit;
                            }
                            else
                            {
                                var newUnit = new UnitDto { SatKd = existingPrice.SatKd, SatNm = existingPrice.SatKd };
                                UnitSearchResults.Add(newUnit);
                                SelectedUnit = newUnit;
                            }
                        }
                    }

                    if (System.Windows.Application.Current?.Dispatcher != null)
                    {
                        await System.Windows.Application.Current.Dispatcher.InvokeAsync(ApplyPrice);
                    }
                    else
                    {
                        ApplyPrice();
                    }

                    await CheckPriceCombinationStatusAsync();
                }
                else
                {
                    await CheckPriceCombinationStatusAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LoadDefaultPriceCombinationAsync] Error: {ex.Message}");
            }
        }

        private async Task<bool> ValidateCurrentStepAsync()
        {
            ClearFieldErrors();

            if (CurrentStepIndex == 0)
            {
                if (!IsNewProductMode)
                {
                    if (!HasSelectedProduct || SelectedProduct == null || string.IsNullOrWhiteSpace(ProductData.BrPrdKd))
                    {
                        ProductSelectionError = "Pilih produk terdaftar terlebih dahulu.";
                        SetValidationWarning("Master Produk Belum Dipilih", ProductSelectionError);
                        return false;
                    }

                    ProductData.IsNewProduct = false;
                    ClearValidationWarning();
                    return true;
                }

                var kd = NewProductCode?.Trim() ?? string.Empty;
                var hasError = false;

                if (string.IsNullOrWhiteSpace(kd))
                {
                    ProductCodeError = "Kode produk wajib diisi.";
                    hasError = true;
                }
                else if (kd.Length > 5)
                {
                    ProductCodeError = "Maksimal 5 karakter.";
                    hasError = true;
                }
                else if (IsCheckingProductCode)
                {
                    ProductCodeError = "Sedang memeriksa ketersediaan kode.";
                    hasError = true;
                }
                else if (IsProductCodeValid != true)
                {
                    ProductCodeError = IsProductCodeValid == false
                        ? "Kode produk sudah digunakan."
                        : "Kode produk belum berhasil diverifikasi.";
                    hasError = true;
                }

                if (string.IsNullOrWhiteSpace(ProductData.BrPrdNm))
                {
                    ProductNameError = "Nama produk wajib diisi.";
                    hasError = true;
                }

                if (string.IsNullOrWhiteSpace(ProductData.BrPrdAcm))
                {
                    ProductAcronymError = "Akronim wajib diisi.";
                    hasError = true;
                }

                if (SelectedFactory == null && string.IsNullOrWhiteSpace(ProductData.BrPrdFacKd))
                {
                    FactoryError = "Pabrik wajib dipilih.";
                    hasError = true;
                }

                if (SelectedProductType == null && string.IsNullOrWhiteSpace(ProductData.BrJnsKd))
                {
                    ProductTypeError = "Jenis barang wajib dipilih.";
                    hasError = true;
                }

                if (string.IsNullOrWhiteSpace(ProductData.Pencari))
                {
                    RequesterError = "Pencari wajib diisi.";
                    hasError = true;
                }

                if (hasError)
                {
                    SetValidationWarning("Data Produk Belum Lengkap", "Lengkapi field yang ditandai sebelum melanjutkan.");
                    return false;
                }

                var exists = await _service.CheckProductExistsAsync(kd);
                if (exists)
                {
                    IsProductCodeValid = false;
                    ProductCodeError = "Kode produk sudah terdaftar di database.";
                    ProductCodeValidationMessage = $"Kode '{kd}' sudah digunakan di master produk.";
                    SetValidationWarning("Kode Produk Sudah Terdaftar", ProductCodeError);
                    return false;
                }

                ProductData.BrPrdKd = kd;
                ProductData.IsNewProduct = true;
                ClearValidationWarning();
                return true;
            }

            if (CurrentStepIndex == 1)
            {
                if (!string.IsNullOrWhiteSpace(SelectedPriceGroup))
                {
                    PriceData.BrHrgGol = SelectedPriceGroup.Trim();
                }
                if (SelectedUnit != null)
                {
                    PriceData.SatKd = SelectedUnit.SatKd.Trim();
                }

                var hasError = false;
                if (string.IsNullOrWhiteSpace(PriceData.BrHrgGol))
                {
                    PriceGroupError = "Golongan harga wajib dipilih.";
                    hasError = true;
                }
                if (string.IsNullOrWhiteSpace(PriceData.SatKd))
                {
                    UnitError = "Satuan wajib dipilih.";
                    hasError = true;
                }
                if (hasError)
                {
                    SetValidationWarning("Data Harga Belum Lengkap", "Lengkapi field yang ditandai sebelum melanjutkan.");
                    return false;
                }
                if (IsPriceCombinationExisting == null)
                {
                    PriceGroupError = "Status kombinasi harga belum selesai diperiksa.";
                    SetValidationWarning("Kombinasi Harga Belum Terverifikasi", PriceGroupError);
                    return false;
                }

                if (IsPriceCombinationExisting == false)
                {
                    if (string.IsNullOrWhiteSpace(NewIdHrgText) || !short.TryParse(NewIdHrgText.Trim(), out var manualId) || manualId <= 0)
                    {
                        PriceIdError = "ID Harga wajib diisi dengan angka valid 1-32767.";
                        SetValidationWarning("ID Harga Wajib Diisi", PriceIdError);
                        return false;
                    }

                    var idExists = await _service.CheckIdHrgExistsAsync(manualId);
                    if (idExists)
                    {
                        PriceIdError = "ID Harga sudah digunakan.";
                        SetValidationWarning("ID Harga Sudah Digunakan", PriceIdError);
                        return false;
                    }

                    PriceData.IdHrg = manualId;
                }

                if (ItemData is not null && ProductData is not null && string.IsNullOrWhiteSpace(ItemData.BrNm))
                {
                    ItemData.BrNm = ProductData.BrPrdNm;
                }
                ClearValidationWarning();
                return true;
            }

            if (CurrentStepIndex == 2)
            {
                var kdNo = ItemData?.BrKdNo?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(kdNo))
                {
                    ItemCodeError = "No. Kode Barang wajib diisi.";
                    SetValidationWarning("No. Kode Barang Wajib Diisi", ItemCodeError);
                    return false;
                }
                if (kdNo.Length > 25)
                {
                    ItemCodeError = "Maksimal 25 karakter.";
                    SetValidationWarning("No. Kode Barang Terlalu Panjang", ItemCodeError);
                    return false;
                }
                if (string.IsNullOrWhiteSpace(ItemData?.BrNm))
                {
                    ItemNameError = "Nama barang belum tersedia.";
                    SetValidationWarning("Nama Barang Wajib Diisi", ItemNameError);
                    return false;
                }
            }

            ClearValidationWarning();
            return true;
        }

        public async Task<bool> SubmitApprovalAsync()
        {
            if (!await ValidateCurrentStepAsync()) return false;

            var prdKd = ProductData?.BrPrdKd?.Trim() ?? string.Empty;
            var satKd = PriceData?.SatKd?.Trim() ?? string.Empty;
            var kdNo = ItemData?.BrKdNo?.Trim() ?? string.Empty;
            var brNm = ItemData?.BrNm?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(kdNo))
            {
                SetValidationWarning("No. Kode Barang Wajib Diisi", "Silakan masukkan penomoran kode barang (bagian akhir) untuk melengkapi format kode barang unik.");
                return false;
            }

            if (kdNo.Length > 25)
            {
                SetValidationWarning("No. Kode Barang Terlalu Panjang", "No. Kode Barang maksimal 25 karakter.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(brNm))
            {
                SetValidationWarning("Nama Barang Wajib Diisi", "Nama barang tidak boleh kosong. Pastikan nama barang sudah terisi sesuai permohonan.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(prdKd) || string.IsNullOrWhiteSpace(satKd))
            {
                SetValidationWarning("Data Approval Belum Lengkap", "Kode Produk dan Satuan belum lengkap. Silakan periksa kembali Step 1 dan Step 2.");
                return false;
            }

            var brKdFormatted = $"{prdKd}.{satKd}.{kdNo}";
            var itemExists = await _service.CheckItemExistsAsync(brKdFormatted);
            if (itemExists)
            {
                SetValidationWarning("Kode Barang Sudah Terdaftar", $"Kode Barang '{brKdFormatted}' sudah ada di database. Silakan gunakan No. Kode Barang yang berbeda.");
                return false;
            }

            ClearValidationWarning();

            IsLoading = true;
            
            var submitDto = new ApprovalWizardSubmitDto
            {
                RequestId = RequestId,
                ApproverNip = _userSession.CurrentUser?.Nip ?? "UNKNOWN",
                ProductData = ProductData ?? new ProductDataDto(),
                PriceData = PriceData ?? new PriceDataDto(),
                ItemData = ItemData ?? new ItemDataDto()
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

        [RelayCommand]
        private async Task SubmitAsync()
        {
            try
            {
                if (await SubmitApprovalAsync())
                {
                    ApprovalSubmitted?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                SetValidationWarning("Terjadi Kesalahan", $"Gagal memproses approval: {ex.Message}");
            }
        }
    }
}
