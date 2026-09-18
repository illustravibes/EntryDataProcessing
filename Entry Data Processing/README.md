# EDP Portal (Entry Data Processing)

Aplikasi Desktop Modern untuk pemrosesan dan manajemen data **Entry Data Processing (EDP)**, dibangun dengan **WPF (.NET 10)** dan desain modern menggunakan **WPF-UI (Windows 11 Fluent Design)** serta arsitektur **MVVM Clean Architecture**.

---

## 🚀 Tech Stack

| Layer | Teknologi | Deskripsi |
|---|---|---|
| **Runtime** | .NET 10.0 (`net10.0-windows`) | Modern high-performance .NET runtime |
| **UI Framework** | WPF + **WPF-UI (v4.3.0)** | Fluent Windows 11 Design System |
| **MVVM Architecture** | **CommunityToolkit.Mvvm (v8.4.2)** | Source generators untuk ObservableProperty & RelayCommand |
| **Data Access / ORM** | **Dapper (v2.1.86)** | High-performance Micro-ORM |
| **Database Drivers** | **MySqlConnector (v2.6.2)** & **System.Data.OleDb (v9.0.0)** | Koneksi native ke MySQL & MS Access |
| **Dependency Injection** | **Microsoft.Extensions.DependencyInjection (v9.0.0)** | IoC Container & Service Lifetimes |
| **Configuration** | **Microsoft.Extensions.Configuration (v9.0.0)** | Konfigurasi fleksibel via `appsettings.json` |
| **Security** | **BCrypt.Net-Next (v4.2.0)** | Verifikasi & hashing password yang aman |

---

## 🗄️ Database Configuration & Provider Switching

Aplikasi ini mendukung arsitektur **Multi-Database Provider**, memungkinkan pergantian antara database lokal/jaringan **MS Access (`.mdb`)** dan database terpusat **MySQL** secara instan tanpa perlu mengubah kode program.

### Pengaturan di `appsettings.json`

Salin file `appsettings.example.json` menjadi `appsettings.json` di root direktori proyek, lalu sesuaikan koneksinya:

```json
{
  "ConnectionStrings": {
    "Provider": "Access",
    "WambDatabase": "Server=YOUR_MYSQL_HOST;Port=3306;Database=db_wamb;Uid=YOUR_USER;Pwd=YOUR_PASSWORD;Charset=utf8mb4;SslMode=Preferred;",
    "AccessDatabase": "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\\Path\\To\\Your\\Wajdb.mdb;Jet OLEDB:Database Password=YOUR_ACCESS_PASSWORD;"
  },
  "Application": {
    "AppName": "EDP Portal",
    "Version": "1.0.0",
    "Branch": "Central",
    "AutoRefreshIntervalSeconds": 60
  }
}
```

### Cara Mengganti Database Provider

Cukup ubah properti `"Provider"` pada file `appsettings.json`:

| Provider | Nilai `Provider` | Keterangan |
|---|---|---|
| **MS Access** | `"Access"` *(Case-Insensitive: `access`, `ACCESS`)* | Menggunakan `AccessConnectionFactory` (OleDb ke file database Access `.mdb`) |
| **MySQL** | `"MySql"` *(atau nilai selain Access)* | Menggunakan `MySqlConnectionFactory` (MySqlConnection ke database server MySQL) |

> 💡 **Catatan Dialek SQL (`SqlDialect`):**
> Query otomatis diadaptasi oleh class `SqlDialect.Adapt()` untuk memastikan kompatibilitas sintaks antara MS Access (misal: `SELECT TOP N`, `Nz()`, escaping `[user]`) dan MySQL (misal: `LIMIT N`, `COALESCE()`, escaping `` `user` ``).
> 
> 🔐 **Autentikasi User:**
> Tabel `user` sudah terhubung (linked) di MS Access maupun MySQL, sehingga proses login mengikuti provider yang sedang aktif secara dinamis.

---

## 📁 Struktur Proyek

```
Entry Data Processing/
├── App.xaml / App.xaml.cs             # Application Entry Point & DI Registration
├── MainWindow.xaml / .cs              # Shell Utama (Navigation, TitleBar, Toast/Snackbar)
├── appsettings.json                   # Konfigurasi Koneksi & Aplikasi
├── AGENTS.md                          # Standar Pengembangan & Panduan AI Agent
│
├── Assets/                            # Gambar, Logo, dan Aset Statis
│
├── Core/                              # Lapisan Fondasi & Infrastruktur Lintas Fitur
│   ├── Common/
│   │   ├── ViewModelBase.cs           # Base class MVVM ViewModel
│   │   └── Result.cs                  # Monadic Result<T> Pattern
│   ├── Configuration/                 # Binding AppConfig dari appsettings.json
│   ├── Data/
│   │   ├── IDbConnectionFactory.cs    # Abstraksi Factory Koneksi Database
│   │   ├── AccessConnectionFactory.cs # Implementasi Koneksi MS Access (OleDb)
│   │   ├── MySqlConnectionFactory.cs  # Implementasi Koneksi MySQL
│   │   └── SqlDialect.cs              # Penerjemah Dialek SQL Portabel
│   ├── Navigation/                    # Layanan Navigasi Halaman
│   ├── Security/                      # Hashing Password (BCrypt)
│   └── Session/                       # State Sesi User Aktif
│
├── Features/                          # Modul & Fitur Berbasis Domain
│   ├── Auth/                          # Login, Autentikasi & Validasi Sesi
│   │   ├── Models/
│   │   ├── Services/
│   │   ├── ViewModels/
│   │   └── Views/ (LoginWindow.xaml)
│   │
│   ├── Dashboard/                     # Dashboard Utama & Ringkasan KPI
│   │   ├── Models/
│   │   ├── Services/
│   │   ├── ViewModels/
│   │   └── Views/ (DashboardPage.xaml)
│   │
│   └── RequestKodeBarang/             # Modul Pemrosesan Request Kode Barang
│       ├── Models/
│       ├── Services/
│       ├── ViewModels/
│       └── Views/
│           ├── ReqKodeListPage.xaml   # Data Grid & Filter Permintaan
│           └── Dialogs/               # Dialog Interaktif
│               ├── ApprovalWizardDialog.xaml  # Wizard Approval & Pemetaan Kode
│               ├── RejectReasonDialog.xaml    # Dialog Penolakan & Alasan
│               └── PhotoPreviewDialog.xaml    # Preview Foto Barang
│
└── Shared/                            # Komponen yang Dipakai Bersama
    └── Converters/                    # WPF Value Converters (Color, Visibility, Format)
```

---

## ✨ Fitur Utama

1. **Autentikasi & Manajemen Sesi:**
   - Login berbasis NIP dengan verifikasi kata sandi terenkripsi BCrypt.
   - Sesi terpusat menyimpan informasi user, cabang, dan hak akses.

2. **Dashboard Interaktif:**
   - Ringkasan kartu metrik/KPI real-time.
   - Shortcut langsung ke modul pemrosesan data.

3. **Manajemen Request Kode Barang:**
   - **Tab Status Workflow:** Navigasi cepat status *Pending*, *Approved*, *Rejected*, dan *Semua*.
   - **Search & Auto-Suggest Filter:** Pencarian instan berdasarkan Toko, Supplier, Jenis Barang, dan Tanggal.
   - **Empty State & Localized Loading:** UX responsif dengan overlay progress lokal di tabel.
   - **Approval Wizard Dialog:**
     - Pemetaan Supplier, Golongan, Satuan, dan Kategori Produk.
     - Penentuan Kode Barang otomatis atau manual.
     - Pratinjau barcode dan nama barang terstandarisasi.
   - **Reject Workflow:** Dialog penolakan dengan catatan alasan yang tersimpan ke database.

---

## 🛠️ Menjalankan Aplikasi

### Prasyarat:
- **Windows 10/11** (64-bit)
- **.NET 10.0 SDK**
- **Microsoft Access Database Engine (ACE.OLEDB 12.0/16.0)** jika menggunakan provider Access.

### Build & Run:
```bash
# Restore & Build
dotnet build

# Jalankan Aplikasi
dotnet run --project "Entry Data Processing.csproj"
```

---

## 📖 Standar Kode & Kontribusi
Untuk panduan arsitektur, konvensi penamaan, dan aturan pengembangan lainnya, silakan baca dokumentasi [AGENTS.md](file:///d:/Projects/Desktop/Entry%20Data%20Processing/AGENTS.md).
