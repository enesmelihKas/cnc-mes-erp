# CncMes — CNC Atölyesi MES/ERP

KOBİ ölçekli bir CNC atölyesi için **MES** (üretim takibi) ve hafif **ERP** (sipariş, stok, maliyet) işlevlerini birleştiren dönem projesi.

Detaylı yol haritası: [`PLAN.md`](./PLAN.md).

## Teknoloji

- **.NET 8** — C#, MAUI Blazor Hybrid (Windows masaüstü `.exe`)
- **EF Core + SQLite** — kurulum gerektirmeyen dosya tabanlı DB (`cncmes.db`)
- **Katmanlı monolit** — Domain → Application → Infrastructure → App

## Proje Yapısı

```
CncMes.sln
└── src/
    ├── CncMes.Domain          (POCO entity'ler, iş kuralları)
    ├── CncMes.Application     (servisler, DTO'lar, interface'ler)
    ├── CncMes.Infrastructure  (EF Core DbContext, SQLite, STL parser)
    └── CncMes.App             (MAUI Blazor Hybrid UI)
```

## Geliştirme

Ön gereksinimler: .NET 8 SDK + `maui-windows` workload (`dotnet workload install maui-windows`).

```powershell
# Build
dotnet build

# Run (Windows masaüstü)
dotnet run --project src/CncMes.App -f net8.0-windows10.0.19041.0

# DB migration ekle
dotnet ef migrations add <Name> `
  --project src/CncMes.Infrastructure `
  --startup-project src/CncMes.Infrastructure `
  --output-dir Persistence/Migrations
```

Çalışma anında SQLite veritabanı `FileSystem.AppDataDirectory/cncmes.db` konumunda oluşur.

## Mevcut Faz

**Phase 0 — Kurulum:** ✅ Solution iskeleti, EF Core SQLite, "Hello CNC" sayfası.
