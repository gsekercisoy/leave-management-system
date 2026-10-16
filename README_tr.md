[English](README.md) | **Türkçe**

# İzin Yönetim Sistemi

Portföy için full-stack başlangıç projesi: çalışanlar izin talebi açar, yöneticiler onaylar/reddeder, onaylanan günler yıllık bakiyeden düşer. **.NET 8 Web API + EF Core** ve **React 18 + Vite** ile geliştirildi.

## Özellikler

- JWT kimlik doğrulama ve roller (`Admin`, `Manager`, `Employee`); parola hash'leme (SHA256 + kullanıcı başına salt, Identity bağımlılığı yok)
- Çalışanlar: talep oluşturma, kendi taleplerini listeleme, yıllık bakiye görüntüleme
- Yönetici/Admin: tüm talepleri listeleme, onaylama (bakiyeden düşer, yetersiz bakiyede 400) / reddetme
- Onay/bakiye kuralları için xUnit birim testleri (EF Core InMemory)
- Docker Compose: API + SQL Server + web

## Teknolojiler

| Katman | Teknoloji |
|--------|-----------|
| API    | .NET 8, ASP.NET Core, EF Core (SqlServer + InMemory), JWT Bearer, Swashbuckle |
| Test   | xUnit, EF Core InMemory |
| Web    | React 18, react-router-dom, axios, Vite |
| Altyapı| Docker, SQL Server 2022 |

## Hızlı Başlatma

### API (.NET 8 SDK gerekir)

```bash
cd leave-api
# appsettings.json: connection string + Jwt:Key alanlarını doldurun (placeholder, secret commit'lemeyin)
dotnet restore
dotnet run   # https://localhost:7010 (Development'ta Swagger)
```

Testler:

```bash
dotnet test
```

### Web (Node 20+ gerekir)

```bash
cd leave-web
npm install
npm run dev      # http://localhost:5173, /api istekleri https://localhost:7010 adresine yönlendirilir
```

> Vite proxy'si `https://localhost:7010` adresine `secure: false` ile gider. `dotnet run` HTTPS için geliştirme sertifikasına bir kez güvenin: `dotnet dev-certs https --trust` (Windows). Alternatif olarak API'yi HTTP'de (`http://localhost:5010`) çalıştırıp proxy'yi oraya çevirebilirsiniz.

### Docker

```bash
SA_PASSWORD=ChangeMe123! JWT_KEY=<en-az-32-karakter-uzun-rastgele-secret> docker compose up --build
# api: http://localhost:7010, web: http://localhost:8081
```

## API

| Yöntem | Uç Nokta                    | Yetki             | Not |
|--------|-----------------------------|-------------------|-----|
| POST   | `/api/auth/register`        | —                 | `{username, password, role?}` → 201 |
| POST   | `/api/auth/login`           | —                 | `{username, password}` → `{token, username, role}` |
| GET    | `/api/leaves`               | Employee+         | Kendi talepleri |
| POST   | `/api/leaves`               | Employee+         | `{startDate, endDate, reason}` |
| GET    | `/api/leaves/all`           | Manager, Admin    | Tüm talepler |
| POST   | `/api/leaves/{id}/approve`  | Manager, Admin    | Bakiyeden düşer; yetersizse 400 |
| POST   | `/api/leaves/{id}/reject`   | Manager, Admin    | Bakiye değişmez |
| GET    | `/api/balances/me`          | Employee+         | Cari yıl bakiyesi |

## Klasör Yapısı

```
leave-api/            .NET 8 Web API (Controllers, Models, Data, Services, Dtos)
leave-api.Tests/      xUnit birim testleri (LeaveService kuralları, EF InMemory)
leave-web/            Vite + React 18 (Login/Register, Dashboard, Approvals)
docker-compose.yml    api + sqlserver + web
```
