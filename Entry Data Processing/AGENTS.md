# AGENTS.md — EDP (Entry Data Processing) Development Standards

Dokumen ini adalah **standar wajib** bagi semua developer dan AI agent yang bekerja di proyek ini.
Semua kontribusi kode harus mematuhi panduan di bawah ini tanpa pengecualian.

---

## 📐 Tech Stack

| Layer | Teknologi | Versi |
|---|---|---|
| Runtime | .NET | 10.0-windows |
| UI Framework | WPF + **WPF-UI (Wpf.Ui)** | **4.3.0** |
| MVVM Toolkit | CommunityToolkit.Mvvm | 8.4.2 |
| Database ORM | Dapper | 2.1.86 |
| Database Driver | MySqlConnector | 2.6.2 |
| DI Container | Microsoft.Extensions.DependencyInjection | 9.0.0 |
| Config | Microsoft.Extensions.Configuration (JSON) | 9.0.0 |
| Password Hashing | BCrypt.Net-Next | 4.2.0 |

---

## 📁 Struktur Folder

```
Entry Data Processing/
├── App.xaml / App.xaml.cs         # Startup + DI registration (Program.CreateHostBuilder)
├── MainWindow.xaml / .cs          # Shell utama: NavigationView, SnackbarPresenter
├── appsettings.json               # Konfigurasi database & app
├── AssemblyInfo.cs
│
├── Assets/                        # Icon, gambar, resource statis
│
├── Core/                          # Infrastruktur lintas fitur (TIDAK boleh import Features)
│   ├── Common/
│   │   ├── ViewModelBase.cs       # Base class semua ViewModel : ObservableObject
│   │   └── Result.cs              # Result<T> pattern (Success / Failure)
│   ├── Configuration/             # AppConfig binding dari appsettings.json
│   ├── Data/
│   │   ├── IDbConnectionFactory.cs
│   │   └── MySqlConnectionFactory.cs
│   ├── Navigation/                # INavigationService, PageService
│   ├── Security/                  # IPasswordHasher, BcryptPasswordHasher
│   └── Session/                   # IUserSession, UserSession
│
├── Features/                      # Satu folder per domain fitur
│   └── {FeatureName}/
│       ├── Models/                # POCO, DTO, Filter model
│       ├── Services/              # Interface + implementasi service
│       ├── ViewModels/            # ViewModel (satu per Page / Dialog)
│       └── Views/
│           ├── {Name}Page.xaml    # Page (navigasi utama)
│           ├── {Name}Page.xaml.cs
│           └── Dialogs/           # WPF Window dialog
│               ├── {Name}Dialog.xaml
│               └── {Name}Dialog.xaml.cs
│
└── Shared/
    └── Converters/                # IValueConverter yang dipakai lintas fitur
```

### Aturan Struktur
- **Core** tidak boleh memiliki dependency ke `Features`.
- **Features** boleh memiliki dependency ke `Core` dan `Shared`.
- Setiap fitur baru **wajib** membuat folder baru di `Features/{NamaFitur}/`.
- Jangan menaruh logika bisnis di View atau code-behind (`.xaml.cs`).

---

## 🎨 WPF UI — WAJIB DIGUNAKAN

> **WPF-UI (Wpf.Ui v4.3.0) adalah MANDATORY.** Semua komponen UI yang tersedia di library ini **harus** menggunakan WPF-UI, bukan kontrol WPF standar yang setara.

### Namespace XAML
Selalu deklarasikan namespace ini di setiap file XAML:

```xml
xmlns:ui="http://schemas.lepo.co/wpfui/2022/xaml"
```

### Komponen Wajib WPF-UI

| Kebutuhan | Gunakan WPF-UI | ❌ Jangan gunakan |
|---|---|---|
| TextBox | `<ui:TextBox />` | `<TextBox />` |
| Button aksi utama | `<ui:Button Appearance="Primary" />` | `<Button />` standar |
| Icon | `<ui:SymbolIcon Symbol="..." />` | Image/Path manual |
| Loading | `<ui:ProgressRing IsIndeterminate="True" />` | `<ProgressBar />` |
| Toast notifikasi | `ISnackbarService` + `SnackbarPresenter` | `MessageBox.Show()` |
| Dialog konfirmasi | `IContentDialogService` + `ContentDialog` | `MessageBox.Show()` |
| NavigationView | `<ui:NavigationView />` di MainWindow | TabControl / ListView |

### Snackbar / Toast
- **Global Snackbar** didaftarkan di `MainWindow.xaml` sebagai `x:Name="RootSnackbar"`.
- Inject `ISnackbarService` ke ViewModel via konstruktor.
- Durasi default: `TimeSpan.FromSeconds(3)` untuk error, `TimeSpan.FromSeconds(2.5)` untuk sukses.
- Toast harus muncul **kanan bawah**, compact, `MaxWidth="380"`.
- **Dialog lokal**: Jika sebuah `Window` dialog memerlukan toast, buat `SnackbarPresenter` lokal dan swap sementara (`Loaded` → swap, `Closed` → restore).

```csharp
// Contoh swap presenter di dialog
Loaded += (_, _) => _snackbarService.SetSnackbarPresenter(DialogSnackbar);
Closed += (_, _) => _snackbarService.SetSnackbarPresenter(App.GetService<MainWindow>().RootSnackbar);
```

### ControlAppearance untuk Snackbar
```csharp
ControlAppearance.Success   // Operasi berhasil
ControlAppearance.Danger    // Error / gagal
ControlAppearance.Caution   // Warning / tolak
ControlAppearance.Secondary // Informasi netral
```

---

## 🏗️ Pola MVVM

### ViewModel
- **Wajib** mewarisi `ViewModelBase` (`Core.Common.ViewModelBase : ObservableObject`).
- Gunakan **CommunityToolkit.Mvvm source generator**:
  - `[ObservableProperty]` untuk property reaktif (generate `OnXxxChanged` partial method).
  - `[RelayCommand]` untuk perintah (generate `XxxCommand`).
- **Dilarang** menulis `INotifyPropertyChanged` manual.

```csharp
public partial class MyViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool _isLoading;

    [RelayCommand]
    public async Task LoadDataAsync() { ... }
}
```

### Binding
- Gunakan `UpdateSourceTrigger=PropertyChanged` untuk input teks real-time.
- Gunakan `BooleanToVisibilityConverter` (sudah ada di `App.xaml`) untuk kontrol tampil/sembunyi.
- **Hindari** code-behind untuk logika; gunakan `Command` binding.

---

## 🔌 Dependency Injection

### Registrasi (App.xaml.cs → Program.CreateHostBuilder)
| Lifetime | Gunakan untuk |
|---|---|
| `AddSingleton` | Service stateless, session, config, `MainWindow` |
| `AddTransient` | ViewModel, Page, Dialog (tiap navigasi = fresh instance) |

### Injeksi ke ViewModel
Selalu injeksi via **konstruktor**, bukan `App.GetService<T>()` secara langsung dari ViewModel.

```csharp
public MyViewModel(IMyService service, ISnackbarService snackbar, IUserSession session)
{
    _service = service;
    _snackbar = snackbar;
    _session = session;
}
```

> `App.GetService<T>()` hanya boleh digunakan dari code-behind View/Window jika tidak ada cara lain.

---

## 🗄️ Service & Data Access

### Interface Wajib
Setiap fitur **harus** memiliki interface service:
```
Features/{Feature}/Services/I{Feature}Service.cs
Features/{Feature}/Services/{Feature}Service.cs
```

### Pola Query (Dapper)
- Gunakan `IDbConnectionFactory` untuk membuka koneksi.
- Gunakan `Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true` (sudah diset di App startup).
- **Selalu** tutup koneksi (`using` statement).
- **Selalu** gunakan parameterized query, **jangan** string interpolation SQL.

```csharp
using var conn = _dbFactory.CreateConnection();
var result = await conn.QueryAsync<MyModel>(
    "SELECT * FROM tabel WHERE id = @Id",
    new { Id = id });
```

### Result Pattern
Semua method service yang bisa gagal **wajib** mengembalikan `Result<T>`:

```csharp
// Service
public async Task<Result<int>> SaveAsync(MyDto dto)
{
    try { /* ... */ return Result<int>.Success(affectedRows); }
    catch (Exception ex) { return Result<int>.Failure(ex.Message); }
}

// ViewModel
var res = await _service.SaveAsync(dto);
if (res.IsSuccess)
    _snackbar.Show("Sukses", "...", ControlAppearance.Success, null, TimeSpan.FromSeconds(2.5));
else
    _snackbar.Show("Gagal", res.ErrorMessage!, ControlAppearance.Danger, null, TimeSpan.FromSeconds(3));
```

---

## 🖼️ Standar UI / XAML

### Layout & Warna
- Background halaman: `#F8FAFC`
- Card / panel: `Background="#FFFFFF"`, `BorderBrush="#E2E8F0"`, `CornerRadius="8"`
- Teks utama: `#0F172A` | Teks sekunder: `#475569` | Teks muted: `#64748B`
- Primary accent: `#0284C7` | Hover: `#0369A1`
- Success green: `#16A34A` | Error red: `#DC2626` | Warning yellow: `#EAB308`

### DataGrid
- Gunakan **custom `ControlTemplate`** agar scrollbar vertikal berada di bawah header (tidak overlap).
- Header background: `#F8FAFC`, height `42`, separator `#CBD5E1`.
- Row height: `44`, hover background: `#F8FAFC`.
- Selalu nonaktifkan `CanUserAddRows`, `CanUserDeleteRows`, `AutoGenerateColumns`.
- Transparansi selection: override `SystemColors.HighlightBrushKey` → `Transparent`.

### Empty State
Setiap tabel **wajib** memiliki overlay empty state (bukan tabel kosong):
- Binding ke `IsEmpty` property di ViewModel (`!IsLoading && TotalRecords == 0`).
- Tampilkan icon (`ui:SymbolIcon`), judul, deskripsi, dan tombol refresh.
- `Margin="0,42,0,0"` agar tidak menutupi header kolom.

### Loading Overlay
Loading **harus lokal ke tabel**, bukan ke seluruh aplikasi:

```xml
<Border Grid.Row="0" Margin="0,42,0,0"
        Visibility="{Binding IsLoading, Converter={StaticResource BooleanToVisibilityConverter}}"
        Panel.ZIndex="10">
    <Border.Background>
        <SolidColorBrush Color="#FFFFFF" Opacity="0.92"/>
    </Border.Background>
    <StackPanel HorizontalAlignment="Center" VerticalAlignment="Center">
        <ui:ProgressRing IsIndeterminate="True" Width="48" Height="48" />
        <TextBlock Text="Memuat data..." FontSize="12" Foreground="#475569"
                   Margin="0,10,0,0" HorizontalAlignment="Center"/>
    </StackPanel>
</Border>
```

### Tab Status
Urutan tab standar untuk fitur approval: **Pending → Approved → Rejected → Semua**

| Tab | Warna aktif | Default tab |
|---|---|---|
| Pending | `#EAB308` (kuning) | ✅ Ya (`SelectedTab = "Pending"`) |
| Approved | `#16A34A` (hijau) | |
| Rejected | `#DC2626` (merah) | |
| Semua | `#0284C7` (biru) | |

---

## 📛 Naming Convention

### C#
| Kategori | Format | Contoh |
|---|---|---|
| Class / Interface | PascalCase | `ReqKodeListViewModel`, `IReqEdpKodeService` |
| Private field | `_camelCase` | `_isLoading`, `_service` |
| Property | PascalCase | `IsLoading`, `SelectedTab` |
| Method | PascalCase | `LoadDataAsync()`, `ApproveAsync()` |
| Async method | Suffix `Async` | `SaveAsync()`, `LoadDataAsync()` |
| Command (RelayCommand) | Suffix `Command` auto-gen | `[RelayCommand] LoadData()` → `LoadDataCommand` |
| DTO / Filter | Suffix `Dto` / `Filter` | `ApprovalActionDto`, `ReqEdpFilter` |

### XAML
| Kategori | Format | Contoh |
|---|---|---|
| `x:Key` Style | PascalCase | `PrimaryFilterButton`, `ModernColumnHeaderStyle` |
| `x:Name` kontrol | camelCase | `RootSnackbar`, `DataGridMain` |
| Page class | Suffix `Page` | `ReqKodeListPage` |
| Dialog Window | Suffix `Dialog` | `ApprovalWizardDialog` |
| ViewModel | Suffix `ViewModel` | `ReqKodeListViewModel` |

### File
- `{Domain}{Purpose}{Type}.cs` → `ReqEdpKodeService.cs`, `ReqKodeListViewModel.cs`
- Satu class per file, nama file = nama class.

---

## 🧭 Navigasi

- Navigasi antar Page menggunakan `INavigationService` (bukan `Frame.Navigate` langsung).
- Page didaftarkan di `PageService` (`Core/Navigation/PageService.cs`).
- Setiap Page baru yang ditambahkan **wajib** didaftarkan di `App.xaml.cs` (DI) dan `PageService`.

---

## ✅ Checklist Fitur Baru

Saat menambahkan fitur baru, pastikan:

- [ ] Buat folder `Features/{NamaFitur}/` dengan subfolder `Models`, `Services`, `ViewModels`, `Views`
- [ ] Buat `I{Feature}Service.cs` (interface) dan `{Feature}Service.cs` (implementasi)
- [ ] Semua method service kembalikan `Result<T>` untuk operasi tulis
- [ ] ViewModel mewarisi `ViewModelBase`, gunakan `[ObservableProperty]` dan `[RelayCommand]`
- [ ] Daftarkan service & ViewModel di `App.xaml.cs`
- [ ] Daftarkan Page di `PageService`
- [ ] Gunakan `ISnackbarService` untuk semua notifikasi (bukan `MessageBox`)
- [ ] Tabel memiliki **empty state** dan **loading overlay lokal**
- [ ] Tab approval mengikuti urutan: Pending → Approved → Rejected → Semua
- [ ] Semua komponen input menggunakan WPF-UI (`ui:TextBox`, `ui:Button`, dll.)
- [ ] Tidak ada logika bisnis di code-behind (`.xaml.cs`)

---

## ⛔ Hal-hal yang DILARANG

1. **`MessageBox.Show()`** untuk notifikasi — gunakan `ISnackbarService`.
2. **`OnPropertyChanged("namafield")`** dengan string literal — gunakan source generator.
3. **SQL string interpolation** — selalu parameterized query.
4. **Logika bisnis di `.xaml.cs`** — pindahkan ke ViewModel atau Service.
5. **Loading overlay seluruh app** — harus lokal ke tabel/area yang relevan.
6. **Kontrol WPF standar** yang sudah punya padanan di WPF-UI (`TextBox`, `ProgressBar`).
7. **`new MyViewModel()`** langsung di ViewModel lain — selalu resolve dari DI container.
8. **`App.GetService<T>()`** dari dalam ViewModel — inject via konstruktor.
