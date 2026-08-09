# Open Source Multi-Tenant URL Shortener

Yüksek performanslı, multi-tenant, self-hosted, özel domain destekli ve analitik özellikleri bulunan açık kaynak URL kısaltma platformu.

## Özellikler

- Üyeliksiz ziyaretçilerin CAPTCHA korumalı şekilde kısa link oluşturabilmesi
- Kimliği doğrulanmış kullanıcılar için tam link yönetimi (düzenleme, devre dışı bırakma, expiration)
- Multi-tenant yönetim paneli ile tam izolasyon
- Birden fazla özel domain desteği (TXT doğrulama)
- Self-hosted kurulum (Docker Compose ile tek komut)
- Yüksek hızlı redirect: Redis → PostgreSQL fallback
- Link bazlı analitik (tıklama, coğrafya, cihaz, kaynak)
- A/B test ve coğrafi yönlendirme
- Rate limiting ve abuse prevention (CAPTCHA, blocklist, abuse report)
- Link expiration (zamanında otomatik 410 Gone)
- QR Code oluşturma
- API Keys ile programatik link yönetimi
- Webhook desteği (link oluşturma, expiration olayları)
- Billing & quota yönetimi (Free / Pro / Enterprise plan)
- OpenTelemetry tabanlı observability

## Hızlı Başlangıç

Tüm servisleri Docker ile başlatmak için:

```bash
docker compose up
```

Bağımlılıkları ayağa kaldırıp backend'i debug modunda çalıştırmak için:

```bash
docker compose up -d postgres redis rabbitmq

dotnet run --project src/Shortener.Api
dotnet run --project src/Shortener.Redirect
dotnet run --project src/Shortener.Worker
```

Frontend:

```bash
cd web/shortener-web
npm install
npm run dev
```

Detaylı kurulum, konfigürasyon ve özellik dökümanları için [Wiki](../../wiki)'ye bakınız.

## Teknoloji

| Katman | Teknoloji |
|---|---|
| Backend | .NET 10, ASP.NET Core Minimal APIs, C# |
| Frontend | Next.js, TypeScript, React |
| Veritabanı | PostgreSQL |
| Cache | Redis |
| Mesaj Kuyruğu | RabbitMQ |
| ORM | EF Core + Dapper |
| CAPTCHA | Cloudflare Turnstile / Google reCAPTCHA |
| Auth | OIDC (Keycloak, Authentik, Entra ID, vb.) |

## Mimari

Modular Monolith + Clean Architecture + CQRS-lite + Event-Driven Background Processing.

Redirect hot-path tasarımı: `Redis HIT → validate → redirect` şeklinde; hiçbir synchronous analitik yazma veya dış servis çağrısı içermez.

## Lisans

MIT
