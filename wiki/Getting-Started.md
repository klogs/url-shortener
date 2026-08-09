# Projeyi Çalıştırma

## Gereksinimler

- [Docker Desktop](https://www.docker.com/products/docker-desktop) (Docker Compose dahil)
- [.NET 10 SDK](https://dotnet.microsoft.com/download) (local/debug modu için)
- [Node.js 20+](https://nodejs.org/) (frontend için)

---

## Docker ile Tam Kurulum (Önerilen)

Tüm servisleri tek komutla başlatın:

```bash
docker compose up
```

Bu komut şunları başlatır:

| Servis | Adres |
|---|---|
| Yönetim API | http://localhost:5000 |
| Redirect servisi | http://localhost:5001 |
| Worker | — |
| Next.js frontend | http://localhost:3000 |
| PostgreSQL | localhost:5432 |
| Redis | localhost:6379 |
| RabbitMQ | localhost:5672 (yönetim: 15672) |

İlk çalıştırma sırasında Shortener.Migrator veritabanı şemasını otomatik olarak oluşturur.

### Servisleri durdurmak

```bash
docker compose down
```

Veritabanı verilerini de silmek için:

```bash
docker compose down -v
```

---

## Debug / Local Geliştirme Modu

Altyapı servislerini Docker'da çalıştırıp .NET uygulamalarını IDE veya terminalden başlatın.

### 1. Altyapıyı başlatın

```bash
docker compose up -d postgres redis rabbitmq
```

### 2. Migration'ı çalıştırın

```bash
dotnet run --project src/Shortener.Migrator
```

> Migrator çalıştırılmadan uygulama başlatılmamalıdır.

### 3. Backend servislerini başlatın

Her birini ayrı terminal penceresinde çalıştırın:

```bash
dotnet run --project src/Shortener.Api
```

```bash
dotnet run --project src/Shortener.Redirect
```

```bash
dotnet run --project src/Shortener.Worker
```

### 4. Frontend'i başlatın

```bash
cd web/shortener-web
npm install
npm run dev
```

### 5. Testleri çalıştırın

Tüm testler:

```bash
dotnet test
```

Belirli bir proje:

```bash
dotnet test tests/Shortener.UnitTests
dotnet test tests/Shortener.IntegrationTests
dotnet test tests/Shortener.ArchitectureTests
```

---

## Konfigürasyon

Servisler environment variable veya `appsettings.json` dosyaları üzerinden yapılandırılır. Aşağıda temel değişkenler listelenmiştir:

```env
# Veritabanı
DATABASE__CONNECTION_STRING=Host=localhost;Database=shortener;Username=...;Password=...

# Redis
REDIS__CONNECTION_STRING=localhost:6379

# RabbitMQ
RABBITMQ__HOST=localhost

# OIDC (kimlik doğrulama provider'ı)
AUTH__AUTHORITY=https://idp.example.com
AUTH__CLIENT_ID=shortener.web
AUTH__CLIENT_SECRET=

# CAPTCHA
CAPTCHA__PROVIDER=Turnstile
CAPTCHA__SITE_KEY=
CAPTCHA__SECRET_KEY=

# Uygulama
APP__DEFAULT_DOMAIN=localhost:5001
MULTITENANCY__MODE=SingleTenant    # veya MultiTenant

# Analytics saklama süresi (gün)
ANALYTICS__RAW_RETENTION_DAYS=90
```

> Gerçek değerleri kaynak koda ya da `git`'e kaydetmeyin. `.env` dosyasına yazın ve `.gitignore`'a ekleyin.

### CAPTCHA Devre Dışı Bırakma (Geliştirme)

CAPTCHA doğrulamasını yerel ortamda devre dışı bırakmak için:

```env
CAPTCHA__PROVIDER=Disabled
```

### Single Tenant Modu

Kişisel veya küçük ekip kullanımı için çoklu tenant yönetimiyle uğraşmadan çalıştırmak:

```env
MULTITENANCY__MODE=SingleTenant
```

Sistem başlangıçta otomatik olarak varsayılan bir tenant oluşturur.
