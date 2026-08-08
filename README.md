# Open Source Multi-Tenant URL Shortener

> Yüksek performanslı, multi-tenant, self-hosted, özel domain destekli ve analitik özellikleri bulunan açık kaynak URL kısaltma platformu.

## 1. Projenin Amacı

Bu proje yalnızca `long-url -> short-code` üreten basit bir servis değildir.

Hedef; aşağıdaki kullanım modellerini aynı ürün altında destekleyen, production-grade bir URL Shortener platformu oluşturmaktır:

- Üyeliksiz ziyaretçilerin CAPTCHA korumalı şekilde kısa link oluşturabilmesi
- Giriş yapmış kullanıcıların kendi linklerini yönetebilmesi
- Multi-tenant yönetim paneli
- Bir tenant'ın birden fazla özel domain kullanabilmesi
- Self-hosted kurulum
- Çok yüksek redirect throughput
- Link bazlı analitik
- Rate limiting ve abuse prevention
- Link expiration
- QR code gibi sonradan eklenebilecek özelliklere uygun extensible yapı
- API üzerinden programatik link oluşturma
- Açık kaynak dağıtım
- Vendor lock-in oluşturmayan altyapı

Bu README aynı zamanda projenin teknik tasarım dokümanı ve geliştirme yol haritasıdır.

---

# 2. Temel Ürün Senaryoları

## 2.1 Anonymous URL Shortening

Kullanıcı sisteme giriş yapmadan:

1. Uzun URL girer.
2. CAPTCHA doğrulanır.
3. Rate limit kontrol edilir.
4. URL normalize ve validate edilir.
5. Zararlı veya yasaklı protokoller reddedilir.
6. Short code üretilir.
7. Varsayılan expiration uygulanır.
8. Kısa URL kullanıcıya döndürülür.

Örnek:

```text
https://www.example.com/products/very-long-url
```

sonucu:

```text
https://sho.rt/Ab3xPq
```

Anonymous linklerde daha kısıtlı özellikler uygulanmalıdır.

Önerilen başlangıç politikası:

- Expiration: 7 gün
- Custom alias: kapalı
- Analytics dashboard: kapalı
- Maksimum URL oluşturma limiti: IP + fingerprint/risk bazlı
- CAPTCHA: zorunlu
- Abuse kontrolleri: zorunlu

Bu değerler konfigüre edilebilir olmalıdır.

---

## 2.2 Authenticated User

Kimliği doğrulanmış kullanıcı:

- Link oluşturabilir
- Custom alias seçebilir
- Linki disable/enable edebilir
- Expiration değiştirebilir
- Link listesini görebilir
- Link analitiğini görebilir
- Tag ekleyebilir
- Link adı/açıklaması ekleyebilir
- QR Code üretebilir
- UTM template uygulayabilir
- Linkleri export edebilir
- API Key oluşturabilir
- Kendi özel domainlerini kullanabilir

---

## 2.3 Tenant

Her tenant birbirinden mantıksal olarak tamamen izole olmalıdır.

Tenant:

- Kullanıcılar
- Roller
- Domainler
- Linkler
- API key'ler
- Analytics
- Rate-limit politikaları
- Retention politikaları
- Branding
- Default expiration
- Custom alias politikaları

gibi varlıkların sahibi olur.

Önerilen izolasyon modeli:

```text
Shared Database
Shared Schema
TenantId Column
```

İlk sürüm için database-per-tenant veya schema-per-tenant gereksiz operasyonel maliyet yaratır.

Bunun yerine tüm tenant-owned tablolarda zorunlu `TenantId` bulunmalıdır.

Repository/query katmanında tenant filtresi merkezi olarak uygulanmalıdır.

---

# 3. Mimari Karar

## Önerilen Mimari

Başlangıçta microservice mimarisi kullanılmamalıdır.

Önerilen yaklaşım:

```text
Modular Monolith
+
Clean Architecture prensipleri
+
CQRS-lite
+
Event Driven Background Processing
```

Neden?

URL Shortener domain olarak nispeten sınırlıdır.

İlk günden onlarca mikroservis oluşturmak:

- deployment karmaşıklığını artırır,
- distributed transaction sorunları oluşturur,
- gözlemlenebilirliği zorlaştırır,
- development velocity'yi düşürür.

Ancak redirect ve analytics workload'ları birbirinden çok farklıdır.

Bu nedenle deployable'lar başlangıçtan itibaren ayrılabilir.

Önerilen solution:

```text
src/
  Shortener.Api
  Shortener.Redirect
  Shortener.Worker
  Shortener.Domain
  Shortener.Application
  Shortener.Infrastructure
  Shortener.Contracts

web/
  shortener-web

tests/
  Shortener.UnitTests
  Shortener.IntegrationTests
  Shortener.ArchitectureTests
  Shortener.LoadTests
```

### Shortener.Api

Control-plane API.

Sorumlulukları:

- link oluşturma
- link güncelleme
- tenant yönetimi
- domain yönetimi
- API key yönetimi
- dashboard API
- analytics query API
- OAuth2/OIDC authentication

### Shortener.Redirect

Data-plane servisidir.

Tek görevi mümkün olduğunca hızlı şekilde:

```text
GET /{shortCode}
```

isteğini çözmek ve redirect etmektir.

Redirect servisinin kritik path'inde:

- EF change tracking
- analytics insert
- RabbitMQ publish bekleme
- external HTTP request
- CAPTCHA
- identity provider çağrısı

olmamalıdır.

### Shortener.Worker

Asenkron operasyonları yürütür:

- analytics aggregation
- expiration sweep
- outbox processing
- cache invalidation/reconciliation
- retention cleanup
- abandoned anonymous-link cleanup
- domain verification jobs
- malware/domain reputation job'ları (gelecekte)
- aggregate statistics

---

# 4. Veri Katmanı Kararı

## Sonuç

Ana veri deposu olarak:

```text
PostgreSQL
```

kullanılmalıdır.

MongoDB ilk versiyonda kullanılmamalıdır.

Redis ise cache ve dağıtık koordinasyon için kullanılmalıdır.

Önerilen mimari:

```text
PostgreSQL = Source of Truth
Redis      = Hot Redirect Cache + Rate Limit State
```

MongoDB eklemek ilk sürümde gereksizdir.

## Neden PostgreSQL?

URL Shortener domaininde aşağıdaki ilişkiler vardır:

```text
Tenant
  -> Domains
  -> Users
  -> Links
  -> API Keys
  -> Tags
  -> Plans / Limits
```

Ayrıca:

- uniqueness constraint
- transactional update
- domain ownership
- alias uniqueness
- pagination
- reporting
- tenant isolation
- relational authorization

gereksinimleri RDBMS ile doğal şekilde çözülmektedir.

Analitik için de ilk aşamada PostgreSQL partitioning yeterlidir.

MongoDB ancak ileride analytics event hacmi PostgreSQL operasyonlarını baskılamaya başlarsa ayrı bir analytics store olarak değerlendirilebilir.

---

# 5. NuGet ve Açık Kaynak Lisans Politikası

Temel kural:

> Production kullanımını, ekip büyüklüğünü, şirket cirosunu veya temel özellikleri ücretli lisansa bağlayan hiçbir üçüncü parti NuGet paketi kullanılmayacaktır.

Amaç yalnızca "ücretsiz bugün" olan paketleri seçmek değil; projenin gelecekte bir vendor'ın ticari lisans değişikliğine bağımlı kalmasını mümkün olduğunca önlemektir.

## Kabul Edilen Lisanslar

Aşağıdaki gibi OSI uyumlu/permissive açık kaynak lisansları kabul edilebilir:

```text
MIT
Apache-2.0
BSD-2-Clause
BSD-3-Clause
ISC
PostgreSQL License
```

Bir paket dual-license ise, production kullanımına açık ve ücretsiz olan lisans açıkça uygulanabiliyorsa kullanılabilir.

## Yasak Paket Kategorileri

Aşağıdaki paketler kullanılmaz:

- Production kullanımında ücretli lisans isteyenler
- Belirli ekip büyüklüğünden sonra ücret isteyenler
- Ciro/revenue sınırı bulunan community lisansları
- License key gerektiren commercial/community modeller
- Temel özellikleri ücretli edition arkasına koyanlar
- Kaynak kodu açık görünse bile OSI uyumlu olmayan source-available lisanslar
- Yeni major sürümü ticari lisansa geçen ve eski ücretsiz major sürüme pinlenerek kullanılmaya çalışılan paketler

## Açıkça Yasaklanan Dependency Aileleri

```text
MediatR
AutoMapper
```

Güncel MediatR 13+ ve AutoMapper 15+ ticari lisans modeline geçtiği için bu projede hiçbir sürümüne bağımlılık eklenmeyecektir.

Eski ücretsiz sürümlere pinlemek kabul edilen bir çözüm değildir.

Bu ihtiyaçlar kendi basit uygulama kodumuzla çözülecektir:

```text
MediatR yerine:
Explicit Command/Query Handlers

AutoMapper yerine:
Explicit Mapping / Mapper Extensions
```

## Sponsorluk ile Ticari Lisans Aynı Şey Değildir

Bir açık kaynak projenin GitHub Sponsors/OpenCollective üzerinden bağış veya sponsorluk istemesi tek başına yasak sebebi değildir.

Paket:

- tam işlevli şekilde açık kaynak kalıyor,
- production kullanımında ücret istemiyor,
- license key istemiyor,
- ekip/ciro sınırı koymuyor

ise kullanılabilir.

## Başlangıçta Kullanılabilecek Paketler

| Paket | Rol | Lisans / Durum |
|---|---|---|
| Npgsql | PostgreSQL driver | PostgreSQL License — uygun |
| EF Core PostgreSQL Provider | ORM / control-plane persistence | Npgsql ekosistemi — uygunluk sürüm bazında CI'da doğrulanacak |
| Dapper | Read-optimized SQL | Apache-2.0 — uygun |
| RabbitMQ.Client | Messaging | Apache-2.0/MPL-2.0 — uygun |
| xUnit | Unit/integration tests | Apache-2.0 — uygun |
| Testcontainers for .NET | Integration tests | MIT — uygun |
| Serilog | Structured logging | Apache-2.0 — uygun |
| FluentValidation | Validation | Apache-2.0 — production lisansı zorunlu değil; sponsorluk isteği engel değildir |

Her paket yine de **ekleneceği gün** güncel lisans koşulları açısından yeniden kontrol edilmelidir.

## Dependency Governance

Repository'de:

```text
docs/DEPENDENCIES.md
```

dosyası tutulmalıdır.

Her dependency için:

```text
Package
Version
License
Repository
Purpose
CommercialLicenseRequired
LicenseKeyRequired
RevenueOrTeamLimit
ApprovedAt
```

kaydı tutulur.

Yeni NuGet ekleyen her PR şu kontrollerden geçmelidir:

- güncel license
- upstream repository
- package owner/maintainer
- production için commercial license gerekiyor mu?
- community license limiti var mı?
- license key gerekiyor mu?
- paid-only feature bağımlılığımız var mı?
- transitive dependency lisansları
- maintenance/release durumu
- bilinen license-change riski

CI pipeline dependency policy ihlalinde build'i fail etmelidir.

---

# 6. Teknoloji Stack'i

## Backend

Önerilen:

```text
.NET 10 LTS
ASP.NET Core Minimal APIs
C#
PostgreSQL
Redis
```

.NET 8 zorunlu deployment ortamı varsa yapı .NET 8'e geri alınabilir.

Ancak sıfırdan başlayan proje için güncel LTS tercih edilmelidir.

## Frontend

Önerilen:

```text
Next.js
TypeScript
React
```

Next.js bu proje için uygun tercihtir.

Avantajları:

- public landing page için SSR/SSG
- SEO
- yönetim paneli
- güçlü ekosistem
- server-side auth callback desteği
- self-host edilebilir
- MIT lisansı

Frontend iki logical alan içerebilir:

```text
/
  Public Shortener

/app/*
  Management Panel
```

İlk etapta iki ayrı Next.js application gerekli değildir.

---

# 7. Authentication

Management Panel authentication:

```text
Authority:
https://idp.klogs.io

ClientId:
klogs.web
```

OIDC Authorization Code Flow kullanılmalıdır.

Öneri:

```text
Authorization Code + PKCE
```

Gerekli diğer bilgiler daha sonra konfigürasyona eklenecektir:

```text
OIDC__Authority
OIDC__ClientId
OIDC__ClientSecret
OIDC__Scopes
OIDC__CallbackPath
```

Backend API:

```text
JWT Bearer
```

ile korunmalıdır.

Tenant bilgisi aşağıdaki seçeneklerden biriyle çözülebilir:

```text
tenant_id claim
```

tercihen token üzerinden.

Alternatif olarak kullanıcı -> tenant membership lookup yapılabilir.

### Authorization

Role yerine yalnızca role-based authorization kullanmak yerine permission/policy tabanlı model önerilir.

Örnek permissions:

```text
links.read
links.create
links.update
links.delete

analytics.read

domains.read
domains.manage

tenant.users.read
tenant.users.manage

api-keys.manage
```

---

# 8. Domain Model

Ana entity'ler:

```text
Tenant
TenantUser
Domain
ShortLink
ShortLinkTag
ApiKey
ClickEvent
DailyLinkStatistic
OutboxMessage
DomainVerification
```

---

# 9. ShortLink Modeli

Önerilen alanlar:

```text
Id
TenantId
DomainId

ShortCode
DestinationUrl

Title
Description

Status

CreatedAt
CreatedBy

UpdatedAt
UpdatedBy

ExpiresAt

LastAccessedAt

RedirectType

IsAnonymous

PasswordHash        // future
MaxClicks            // future
ClickCountSnapshot

Version
```

Status:

```text
Active
Disabled
Expired
Blocked
Deleted
```

`Deleted` soft-delete olarak uygulanabilir.

---

# 10. Short Code Stratejisi

Short code generation redirect performansının temelidir.

Önerilen alphabet:

```text
0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz
```

Base62.

7 karakter:

```text
62^7 ~= 3.5 trilyon
```

kombinasyon sağlar.

İlk sürüm:

```text
7 characters
```

uygundur.

## Generation

Random cryptographic code üret:

```text
RandomNumberGenerator
```

kullan.

Database'de:

```text
UNIQUE (DomainId, ShortCode)
```

constraint olmalıdır.

Collision olduğunda retry edilir.

Custom alias:

```text
https://sho.rt/summer-sale
```

desteklenmelidir.

Alias kuralları:

- case sensitivity politikası net olmalı
- reserved route'lar engellenmeli
- Unicode tercihen ilk sürümde kapalı
- normalization uygulanmalı

Reserved aliases:

```text
api
admin
app
login
logout
health
metrics
swagger
docs
robots.txt
favicon.ico
```

---

# 11. Domain Çözümleme

Short code tek başına global unique olmak zorunda değildir.

Doğru key:

```text
Host + ShortCode
```

olmalıdır.

Örnek:

```text
a.com/hello
b.com/hello
```

iki farklı link olabilir.

DB constraint:

```text
UNIQUE (DomainId, ShortCode)
```

Redis key:

```text
sl:{normalized-host}:{short-code}
```

Örnek:

```text
sl:go.example.com:Ab3xPq
```

---

# 12. Custom Domain Desteği

Bu proje self-hosted kullanılacağı için domain altyapısı first-class citizen olmalıdır.

Domain tablosu:

```text
Id
TenantId

Host
NormalizedHost

Status

VerificationToken
VerifiedAt

IsDefault

CreatedAt
```

Status:

```text
Pending
Verified
Active
Disabled
```

## Domain verification

Önerilen doğrulama:

```text
TXT record
```

Örnek:

```text
_shortener-verification.example.com
```

value:

```text
shortener-verification=<random-token>
```

Alternatif CNAME doğrulaması da eklenebilir.

Self-hosted modda tek domain kullanımı için verification devre dışı bırakılabilir.

---

# 13. Redirect Pipeline

En kritik endpoint:

```text
GET /{shortCode}
```

Akış:

```text
Request
   |
   v
Normalize Host
   |
   v
Redis Lookup
   |
   +---- HIT ------> Validate Status/ExpiresAt -> Redirect
   |
   +---- MISS
          |
          v
      PostgreSQL
          |
          v
      Validate
          |
          v
      Redis SET
          |
          v
      Redirect
```

Hedef:

```text
Cache hit:
No PostgreSQL query
No synchronous analytics persistence
No remote authentication call
```

## Redirect Type

Varsayılan:

```text
302 Found
```

olmalıdır.

301 link cache davranışı nedeniyle sonradan hedef URL değiştirmek isteyen kullanıcılar için problem yaratabilir.

Tenant/link bazında:

```text
301
302
307
308
```

opsiyonu verilebilir.

---

# 14. Redis Kullanımı

Redis source-of-truth değildir.

Redis aşağıdaki amaçlarla kullanılmalıdır:

### Redirect Cache

```text
Host + Code -> redirect metadata
```

Value:

```json
{
  "linkId": "...",
  "destination": "https://...",
  "status": "Active",
  "expiresAt": "..."
}
```

TTL:

```text
min(configured-cache-ttl, link-expiration)
```

### Negative Cache

Bulunamayan linkler kısa süre cache'lenebilir.

Örnek:

```text
30-60 seconds
```

Bu özellik brute-force scanning sırasında PostgreSQL yükünü düşürür.

### Rate Limiting

Public URL creation rate limit state.

### Distributed coordination

Gerekirse:

- idempotency
- short-lived lock
- abuse counters

için kullanılabilir.

---

# 15. Cache Invalidation

Link değiştiğinde:

```text
DestinationUrl
Status
ExpiresAt
Domain
```

gibi redirect sonucunu etkileyen alanlar Redis'ten invalidated edilmelidir.

Cache correctness için:

```text
Cache Aside
```

kullanılmalıdır.

Transaction sonrası cache invalidate edilir.

Ek olarak worker düzenli reconciliation yapabilir.

---

# 16. Anonymous Shortener Security

Public anonymous endpoint abuse'a açıktır.

Endpoint:

```text
POST /api/public/links
```

aşağıdakileri zorunlu kullanmalıdır:

1. Rate Limit
2. CAPTCHA
3. URL validation
4. Scheme allowlist
5. Blocked destination rules
6. Maximum request size
7. Destination length limit
8. Audit/security logging
9. Abuse heuristics

---

# 17. CAPTCHA

Önerilen çözüm:

```text
Cloudflare Turnstile
```

Neden:

- ücretsiz plan
- production kullanımına uygun
- kullanıcı deneyimi reCAPTCHA'ya göre daha hafif
- Cloudflare CDN kullanmak zorunda değil
- server-side token verification destekli

Frontend yalnızca token üretir.

Backend kesinlikle:

```text
Siteverify
```

çağrısı yaparak tokenı doğrulamalıdır.

CAPTCHA provider abstraction oluşturulmalıdır:

```csharp
ICaptchaVerifier
```

Implementations:

```text
TurnstileCaptchaVerifier
DisabledCaptchaVerifier   // local/dev
```

Böylece başka CAPTCHA provider'a geçilebilir.

---

# 18. URL Validation

Sadece:

```text
http
https
```

izin verilmelidir.

Engellenmesi gereken örnekler:

```text
javascript:
data:
file:
ftp:
```

SSRF açısından public URL Shortener normalde destination URL'i server-side fetch etmemelidir.

Bu çok önemli bir güvenlik avantajıdır.

URL canonicalization dikkatli yapılmalıdır.

Destination URL üzerinde aggressive normalization yapılmamalıdır; query param ordering gibi değişiklikler hedef uygulama davranışını bozabilir.

---

# 19. Rate Limiting

ASP.NET Core built-in Rate Limiting kullanılmalıdır.

Public create endpoint için kombinasyon:

```text
IP sliding-window
+
global concurrency limit
```

Örnek başlangıç politikası:

```text
5 create / minute / IP
30 create / hour / IP
```

Değerler konfigüre edilebilir olmalıdır.

Authenticated API:

```text
TenantId / API Key
```

bazlı rate limit kullanmalıdır.

Redirect endpoint'ine normal create endpoint kadar agresif rate limit uygulanmamalıdır.

Aksi halde viral linklerin çalışması engellenir.

Redirect'te daha çok:

- DDoS protection
- reverse proxy protection
- global safety limits

kullanılmalıdır.

---

# 20. Expiration Tasarımı

Evet, URL'lerde expiration desteklenmelidir.

Ancak tüm linklerin expire olması zorunlu değildir.

```text
ExpiresAt NULL = Never Expires
```

## Anonymous

Önerilen default:

```text
7 days
```

konfigüre edilebilir.

## Authenticated

Kullanıcı:

```text
1 hour
1 day
7 days
30 days
90 days
1 year
Never
Custom
```

seçebilir.

Tenant policy maksimum değeri sınırlandırabilir.

---

# 21. Expiration Worker Gerekli mi?

Evet, ancak redirect correctness worker'a bağlı olmamalıdır.

Yanlış model:

```text
Worker linki Expired yapana kadar link çalışmaya devam eder.
```

Doğru model:

Redirect sırasında:

```text
if ExpiresAt <= UtcNow
    return 410 Gone
```

yapılır.

Dolayısıyla worker 5 dakika gecikse bile link açılmaz.

## Expiration Worker'ın görevi

Periyodik batch:

```sql
UPDATE ShortLinks
SET Status = 'Expired'
WHERE Status = 'Active'
  AND ExpiresAt IS NOT NULL
  AND ExpiresAt <= NOW()
LIMIT ...
```

mantığıyla:

- state reconciliation
- cache invalidation
- operational metrics
- reporting
- cleanup workflow

yapar.

Her link için ayrı timer/job oluşturulmaz.

Bu milyonlarca linkte kötü ölçeklenir.

Önerilen worker interval:

```text
30-60 seconds
```

Fakat correctness yine `ExpiresAt` kontrolündedir.

---

# 22. HTTP Response for Expired Links

Önerilen:

```text
410 Gone
```

404 yerine 410 linkin daha önce var olup artık geçersiz olduğunu semantik olarak daha doğru ifade eder.

Tenant isterse branded expired page gösterilebilir.

---

# 23. Analytics Tasarımı

Redirect endpoint'i analytics kayıt işlemini synchronous yapmamalıdır.

Yanlış:

```text
Request
 -> DB analytics INSERT
 -> Redirect
```

Bu redirect latency'yi analytics database performansına bağlar.

Doğru:

```text
Request
 -> Resolve
 -> enqueue/buffer event
 -> Redirect

Background
 -> persist event
```

---

# 24. Analytics Event Alanları

Her click için ihtiyaç duyulabilecek bilgiler:

```text
EventId
LinkId
TenantId
DomainId

OccurredAtUtc

IP
IPHash
AnonymizedIP

UserAgent

Referer
RefererHost

AcceptLanguage

CountryCode
Region
City

Browser
BrowserVersion

OperatingSystem
DeviceType

IsBot
BotName

UTMSource
UTMMedium
UTMCampaign
UTMTerm
UTMContent

RequestHost
ShortCode

ResponseStatus

CorrelationId
```

Ancak bütün alanların ilk sürümde tutulması gerekmez.

---

# 25. IP Adresi ve Privacy

Raw IP adresi analitik açısından faydalıdır fakat privacy açısından hassas veri niteliğinde değerlendirilebilir.

Önerilen seçenekler:

### Privacy-first default

Raw IP saklama:

```text
OFF
```

Saklanan:

```text
IPHash
AnonymizedIP
Country
Region
```

IPv4 örneği:

```text
192.168.12.34
->
192.168.12.0
```

Tenant ayarında raw IP logging açılabilir ancak:

- retention
- privacy notice
- hukuki yükümlülükler

tenant sorumluluğu olarak belgelenmelidir.

Analytics retention configurable olmalıdır.

Örnek:

```text
Raw Click Events: 90 days
Aggregates: unlimited
```

---

# 26. Analytics Kullanıcıya Ne Göstermeli?

Dashboard metrikleri:

## Overview

- Total clicks
- Unique visitors estimate
- Today
- Last 7 days
- Last 30 days
- Lifetime
- Last clicked at

## Time Series

```text
Clicks by minute/hour/day
```

## Geography

- Country
- Region
- City (opsiyonel)

## Technology

- Browser
- Operating System
- Device type

## Traffic Source

- Referrer domains
- Direct
- Search
- Social

## Campaign

- UTM source
- UTM medium
- UTM campaign

## Security / Quality

- Bot traffic
- suspicious traffic
- rate-limited attempts

---

# 27. Raw Event ve Aggregate Ayrımı

Analytics iki seviyeli tutulmalıdır.

### Raw

```text
ClickEvent
```

yüksek hacimli.

### Aggregate

```text
DailyLinkStatistic
```

Örnek:

```text
LinkId
Date
Clicks
UniqueVisitorEstimate
BotClicks
```

Dashboard uzun tarih aralıklarında raw click tablosunu scan etmemelidir.

---

# 28. PostgreSQL Analytics Partitioning

ClickEvent tablosu büyüyeceği için:

```text
RANGE partition by OccurredAtUtc
```

önerilir.

Örnek:

```text
click_events_2026_08
click_events_2026_09
```

Avantaj:

- retention cleanup
- query performance
- partition pruning
- index boyutlarının yönetimi

Retention zamanı geldiğinde milyonlarca row delete yerine partition drop yapılabilir.

---

# 29. Mesajlaşma Tasarımı

RabbitMQ bu projede kullanılabilir.

Ancak redirect hot-path doğrudan RabbitMQ publish işlemini beklememelidir.

Önerilen analytics pipeline:

```text
HTTP Redirect
   |
   v
Bounded Channel<ClickEvent>
   |
   +---- request redirect cevabını hemen döner
   |
   v
Background Publisher
   |
   v
RabbitMQ
   |
   v
Analytics Consumer
   |
   v
PostgreSQL ClickEvent partitions
```

Bu tasarım redirect latency'sini broker/network gecikmesinden ayırır.

## RabbitMQ Client

İlk tercih:

```text
RabbitMQ.Client
```

olacaktır.

İlk sürümde MassTransit gibi ilave messaging abstraction'ı zorunlu değildir.

Amaç:

- dependency sayısını azaltmak
- broker davranışını açık tutmak
- retry/confirm/topology kararlarını kontrollü biçimde yönetmek
- kritik hot-path'i framework abstraction'larına bağlamamak

## Delivery Semantics

Analytics için:

```text
At-least-once where practical
```

hedeflenir.

Consumer idempotent olmalıdır.

Her event:

```text
EventId
```

taşır.

Duplicate event'ler database tarafında engellenebilir.

## Buffer Overflow Politikası

Bounded Channel dolduğunda davranış configurable olmalıdır:

```text
Drop analytics event
veya
fallback spool
```

Redirect hiçbir zaman analytics nedeniyle uzun süre bloklanmamalıdır.

Drop edilen event sayısı:

```text
analytics_events_dropped_total
```

metriğiyle izlenmelidir.

Control-plane domain event'leri için ise analytics'ten farklı olarak kayıp kabul edilmez.

Bu nedenle:

```text
PostgreSQL Transactional Outbox
 -> Background Publisher
 -> RabbitMQ
```

kullanılır.

---

# 30. Outbox Pattern

Control-plane değişikliklerinde:

```text
LinkCreated
LinkUpdated
LinkDisabled
LinkExpired
DomainActivated
```

event'leri DB transaction ile beraber Outbox'a yazılır.

Worker:

```text
OutboxMessage
```

tablosunu tüketir.

Örnek kullanım:

```text
LinkUpdated
 -> cache invalidation
 -> audit
 -> webhook (future)
```

Bu yapı ileride broker eklemeyi kolaylaştırır.

---

# 31. Application Architecture

Katmanlar:

```text
Domain
Application
Infrastructure
Presentation
```

## Domain

Bağımlılık yok.

İçerir:

- entities
- value objects
- domain rules
- domain events
- enums

## Application

İçerir:

- use cases
- commands
- queries
- interfaces
- validators
- DTO mappings
- authorization rules

MediatR kullanılmayacaktır.

Basit explicit handlers:

```text
CreateShortLinkHandler
UpdateShortLinkHandler
DisableShortLinkHandler
GetLinkAnalyticsHandler
```

Dependency Injection ile doğrudan çağrılır.

Bu yaklaşım:

- magic azaltır
- reflection azaltır
- runtime pipeline karmaşıklığını azaltır
- vendor dependency azaltır

---

# 32. CQRS-lite

Write ve read modellerini tamamen farklı database'lere ayırmak ilk sürüm için gereksizdir.

Ancak kod seviyesinde:

```text
Commands
Queries
```

ayrımı yapılmalıdır.

Örnek:

```text
Application/
  Links/
    Commands/
      CreateLink/
      UpdateLink/
      DisableLink/

    Queries/
      GetLink/
      SearchLinks/
      GetLinkStatistics/
```

---

# 33. SOLID ve Clean Code Kuralları

Zorunlu prensipler:

- SOLID
- DRY
- KISS
- YAGNI
- Composition over inheritance
- Explicit dependencies
- Fail fast
- Immutable DTO/value objects mümkün olduğunca
- CancellationToken propagation
- async all the way
- UTC everywhere
- nullable reference types enabled
- warnings as errors
- analyzers enabled
- centralized error mapping
- idempotency where required

Avoid:

- GenericRepository
- BaseService
- God classes
- Service locator
- static mutable state
- unnecessary abstractions
- reflection-heavy mediator patterns
- business logic in controllers/endpoints

---

# 34. Repository Tasarımı

Generic repository kullanılmamalıdır.

Domain-specific repository:

```text
IShortLinkRepository
IDomainRepository
ITenantRepository
IAnalyticsRepository
```

Örnek:

```text
GetByHostAndCodeAsync()
ExistsAliasAsync()
InsertAsync()
SearchAsync()
```

Bu yaklaşım persistence detayını gizler ancak query kabiliyetini generic CRUD abstraction altında öldürmez.

---

# 35. Multi-Tenant Güvenlik

En kritik güvenlik kurallarından biri:

> Client tarafından gönderilen TenantId'ye güvenilmez.

Tenant:

- authenticated identity
- token claim
- server-side membership

üzerinden çözülmelidir.

Örnek:

```text
GET /api/tenants/{tenantId}/links
```

endpoint olsa bile user'ın tenant membership kontrolü yapılmalıdır.

Repository query'leri:

```text
WHERE TenantId = @CurrentTenantId
```

zorunlu olmalıdır.

---

# 36. Audit Log

Management işlemleri için audit log önerilir.

Alanlar:

```text
TenantId
ActorUserId
Action
EntityType
EntityId
OccurredAtUtc
IPHash
Metadata
```

Örnek:

```text
Link.Created
Link.DestinationChanged
Link.Disabled
Domain.Added
ApiKey.Created
ApiKey.Revoked
```

---

# 37. API Keys

Programatik kullanım için tenant-scoped API key desteklenmelidir.

Key kullanıcıya sadece oluşturulduğu anda gösterilir.

DB'de:

```text
key hash
```

saklanır.

Prefix:

```text
uks_live_xxxxx
uks_test_xxxxx
```

gibi human-identifiable format kullanılabilir.

Permissions:

```text
links.read
links.write
analytics.read
```

---

# 38. API Versioning

İlk API:

```text
/api/v1
```

olarak başlamalıdır.

Breaking changes:

```text
/api/v2
```

ile yayınlanmalıdır.

---

# 39. OpenAPI

Backend built-in OpenAPI generation kullanmalıdır.

API dokümantasyonu:

```text
docs/API.md
```

ve OpenAPI JSON üzerinden sağlanmalıdır.

Dokümantasyon şunları içermelidir:

- authentication
- API keys
- rate limits
- request samples
- response samples
- error model
- pagination
- idempotency
- webhook docs (future)

---

# 40. Standard Error Response

Problem Details kullanılmalıdır.

Örnek:

```json
{
  "type": "https://docs.example/errors/alias-taken",
  "title": "Alias already exists",
  "status": 409,
  "code": "SHORT_LINK_ALIAS_TAKEN",
  "traceId": "..."
}
```

---

# 41. Pagination

Offset pagination küçük dashboard listelerinde kabul edilebilir.

Fakat yüksek hacimli link listelerinde:

```text
cursor/keyset pagination
```

önerilir.

Örnek:

```text
GET /api/v1/links?after=...
```

---

# 42. Idempotency

Programatik link creation için desteklenmelidir.

Header:

```text
Idempotency-Key
```

Aynı tenant + key tekrar gönderildiğinde aynı response dönmelidir.

TTL configurable olabilir.

---

# 43. Observability

Minimum:

```text
Structured Logs
Metrics
Distributed Traces
Health Checks
```

Third-party lisans politikası nedeniyle önce built-in `ILogger` tercih edilmelidir.

OpenTelemetry kullanımı ancak repository lisans allowlist'i ile uyumluysa eklenmelidir.

Ölçülmesi gereken temel metrikler:

```text
redirect_requests_total
redirect_cache_hits_total
redirect_cache_misses_total
redirect_not_found_total
redirect_expired_total

redirect_duration_ms

link_create_total
captcha_failure_total
rate_limit_rejected_total

analytics_events_total
analytics_dropped_total
analytics_queue_depth

outbox_pending_total
expiration_processed_total
```

---

# 44. Health Endpoints

```text
/health/live
/health/ready
```

Liveness:

- process ayakta mı?

Readiness:

- PostgreSQL
- Redis
- kritik dependencies

kontrol eder.

Redirect service için Redis arızasında PostgreSQL fallback mümkün olduğundan readiness politikası dikkatli tasarlanmalıdır.

---

# 45. Redis Failure Mode

Redis down olursa kısa linkler tamamen çalışmayı bırakmamalıdır.

Akış:

```text
Redis unavailable
 -> PostgreSQL lookup
 -> redirect
```

Bu daha yavaş olacaktır ama servis devam eder.

Circuit breaker benzeri failure suppression uygulama seviyesinde basit şekilde yapılabilir.

---

# 46. PostgreSQL Failure Mode

PostgreSQL geçici olarak down ise Redis'te bulunan hot linkler teorik olarak redirect edilmeye devam edebilir.

Ancak cache entry'nin:

- Status
- ExpiresAt
- Destination

bilgileri yeterli olmalıdır.

Bu davranış configuration ile kontrol edilebilir.

Önerilen production mode:

```text
Serve valid cached redirects during short PostgreSQL outages.
```

---

# 47. Security Headers

Web:

- CSP
- HSTS
- X-Content-Type-Options
- Referrer-Policy
- frame-ancestors
- Permissions-Policy

kullanmalıdır.

Turnstile CSP gereksinimleri dokümante edilmelidir.

---

# 48. Open Redirect Konusu

URL Shortener doğası gereği controlled open redirect servisidir.

Bu nedenle genel "open redirect vulnerability" scanner sonuçları domain bağlamında değerlendirilmelidir.

Ancak:

- admin callback
- login callback
- logout redirect

gibi authentication URL'leri shortener destination mantığından tamamen ayrı validate edilmelidir.

---

# 49. Abuse Prevention

URL Shortener spam/phishing açısından kötüye kullanılabilir.

İlk versiyonda:

- CAPTCHA
- IP rate limit
- URL scheme allowlist
- destination hostname blocklist
- anonymous expiration
- report abuse endpoint
- administrative block
- tenant suspension

olmalıdır.

Gelecekte:

- URL reputation
- phishing feed
- domain reputation
- ASN risk
- disposable domain detection

eklenebilir.

---

# 50. Abuse Report

Public endpoint:

```text
POST /api/v1/abuse-reports
```

Alanlar:

```text
ShortUrl
Reason
Email (optional)
Description
CaptchaToken
```

Admin:

- reported links
- block link
- block destination domain
- suspend tenant

işlemlerini yapabilmelidir.

---

# 51. Link Preview

Güvenlik nedeniyle ilk sürümde destination URL server-side fetch edilmemelidir.

Link title/preview metadata otomatik çekilecekse bu iş:

```text
isolated background worker
```

üzerinden yapılmalıdır.

SSRF korumaları:

- private IP ranges
- localhost
- link-local
- metadata endpoints
- DNS rebinding

kontrol edilmelidir.

Bu özellik Phase 1'e alınmamalıdır.

---

# 52. Database Indexleri

ShortLinks:

```text
UNIQUE (DomainId, ShortCode)

INDEX (TenantId, CreatedAt DESC)

INDEX (TenantId, Status, CreatedAt DESC)

INDEX (ExpiresAt)
WHERE ExpiresAt IS NOT NULL
  AND Status = 'Active'
```

Domains:

```text
UNIQUE (NormalizedHost)
```

ClickEvents:

```text
INDEX (LinkId, OccurredAtUtc DESC)
```

partition bazında.

---

# 53. Link Sayacı

Her redirect için:

```sql
UPDATE ShortLinks SET ClickCount = ClickCount + 1
```

yapılmamalıdır.

Bu hot-row contention yaratır.

Doğru:

```text
ClickEvents
 -> worker aggregation
 -> statistics tables
```

Link tablosunda gösterim amaçlı eventual-consistent snapshot tutulabilir.

---

# 54. Unique Visitor

"Unique visitor" kesin kullanıcı sayısı değildir.

Authentication olmayan redirect sisteminde tahmini unique visitor hesaplanabilir.

Privacy-first fingerprint:

```text
hash(
  anonymized_ip
  + normalized_user_agent
  + daily_salt
)
```

gibi yaklaşım değerlendirilebilir.

Daily salt sayesinde uzun dönemli kullanıcı takibi azaltılır.

Bu metrik dokümantasyonda:

```text
Estimated Unique Visitors
```

olarak gösterilmelidir.

---

# 55. Bot Detection

İlk sürümde basit User-Agent pattern classification yeterlidir.

Raw click event:

```text
IsBot
BotName
```

alanları taşıyabilir.

Analytics dashboard:

```text
Human clicks
Bot clicks
All clicks
```

filtrelemelidir.

---

# 56. Frontend Sayfaları

Public:

```text
/
 /about
 /docs
 /report
```

Authenticated:

```text
/app
/app/links
/app/links/{id}
/app/domains
/app/api-keys
/app/team
/app/settings
```

Link detail:

```text
Overview
Analytics
Geography
Devices
Referrers
UTM
Settings
```

---

# 57. Public Shortener UX

Landing page mümkün olduğunca basit:

```text
Paste your long URL
[________________________]
[CAPTCHA]
[Shorten URL]
```

Sonuç:

```text
https://sho.rt/Ab3xPq

[Copy]
[QR]
```

Anonymous kullanıcıya:

```text
This link expires in 7 days.
Create an account to manage links and analytics.
```

gösterilebilir.

---

# 58. Deployment

Minimum docker-compose:

```text
shortener-api
shortener-redirect
shortener-worker
shortener-web

postgres
redis
```

Reverse proxy:

```text
Nginx
Traefik
Caddy
Fabio
```

kullanıcının tercihine bırakılabilir.

Repository örnek olarak Nginx configuration sağlayabilir.

---

# 59. Self-Hosted Domain Configuration

Environment:

```text
APP__DEFAULT_DOMAIN=s.example.com
APP__PUBLIC_WEB_URL=https://shortener.example.com

POSTGRES__CONNECTION_STRING=...
REDIS__CONNECTION_STRING=...

OIDC__AUTHORITY=https://idp.example.com
OIDC__CLIENT_ID=...
```

Klogs deployment:

```text
OIDC__AUTHORITY=https://idp.klogs.io
OIDC__CLIENT_ID=klogs.web
```

---

# 60. TLS

Application TLS termination yapmamalıdır.

TLS reverse proxy/load balancer'da terminate edilebilir.

`X-Forwarded-*` headers güvenli şekilde configure edilmelidir.

Trusted proxies/networks açıkça tanımlanmalıdır.

---

# 61. Testing Strategy

Test piramidi:

```text
          E2E
       Integration
     Unit Tests
```

---

# 62. Unit Tests

Önerilen test framework'ü:

```text
xUnit
```

xUnit tamamen açık kaynak ve Apache-2.0 lisanslıdır.

Assertion tarafında ilk tercih xUnit'ın kendi `Assert` API'sidir; gereksiz assertion/mocking dependency'leri eklenmez.

Test edilmesi gerekenler:

- ShortCode generation
- URL validation
- alias normalization
- expiration rules
- tenant authorization
- redirect status resolution
- cache key construction
- domain normalization
- permission evaluation
- rate-limit policy calculation
- IP anonymization
- analytics aggregation
- idempotency behavior

Mocking library yerine küçük hand-written fakes tercih edilebilir.

Bu ayrıca dependency sayısını azaltır.

---

# 63. Integration Tests

PostgreSQL ve Redis ile gerçek entegrasyon testleri.

MIT uyumlu ise Testcontainers for .NET kullanılabilir.

Alternatif:

```text
docker compose -f docker-compose.test.yml
```

ile dependency'ler kaldırılıp test process'i doğrudan bunları kullanabilir.

Testler:

- DB migrations
- unique alias
- multi-tenant isolation
- cache hit/miss
- expiration behavior
- Redis unavailable fallback
- OIDC authorization policies
- public create
- CAPTCHA verifier fake
- API key auth

---

# 64. Load Tests

Load testler ayrı project:

```text
tests/Shortener.LoadTests
```

Load test için repository'de dış SaaS zorunluluğu olmayan bir test harness tutulmalıdır.

İlk tercih, version-controlled `k6` senaryolarıdır. CI ortamında k6 kullanılamıyorsa BCL-only `HttpClient` tabanlı basit fallback harness tutulabilir.

Senaryolar:

### Scenario A — Hot Redirect

```text
100% Redis hit
```

Amaç:

- maksimum throughput
- p50/p95/p99 latency

### Scenario B — Cold Redirect

```text
Redis miss
PostgreSQL hit
```

### Scenario C — Mixed

```text
95% hot
5% cold
```

### Scenario D — Viral Link

Aynı short code'a yüksek concurrency.

### Scenario E — Random Links

Geniş key space.

### Scenario F — Link Creation

CAPTCHA verifier test mode ile public create.

### Scenario G — Redis Failure

Redis kapatılır, PostgreSQL fallback ölçülür.

---

# 65. Performance Acceptance Criteria

Hardware bağımlı olduğu için sabit RPS hedefinden önce latency/SLO tanımlanmalıdır.

Başlangıç hedefi:

```text
Hot redirect:
p95 < 20 ms application-side
p99 < 50 ms

Cold redirect:
p95 < 75 ms
```

Gerçek production hedefleri benchmark hardware ve network koşullarına göre finalize edilir.

Critical target:

```text
0 synchronous analytics DB write on redirect path
```

---

# 66. Benchmark Metrics

Her load run sonunda:

- total requests
- successful requests
- failed requests
- requests/sec
- p50
- p90
- p95
- p99
- max latency
- CPU
- memory
- GC
- PostgreSQL connections
- Redis connections
- cache hit ratio

raporlanmalıdır.

---

# 67. CI Pipeline

Pipeline:

```text
Restore
Dependency License Check
Build
Unit Test
Integration Test
Architecture Test
Frontend Lint
Frontend Test
Build Containers
Security Scan
Load Smoke Test
```

Main branch merge için:

```text
0 test failures
0 license violations
0 compiler warnings
```

hedeflenmelidir.

---

# 68. Architecture Tests

Bağımlılık kuralları CI'da test edilmelidir.

Örnek:

```text
Domain -> hiçbir project'e bağlı olamaz

Application -> Infrastructure'a bağlı olamaz

Redirect -> Management UI bağımlılığı alamaz
```

Third-party architecture testing package yerine reflection tabanlı basit MSTest kontrolleri yazılabilir.

---

# 69. Migrations

Schema migrations repository içinde version-controlled olmalıdır.

Production migration:

```text
application startup
```

üzerinden otomatik yapılmamalıdır.

Ayrı:

```text
Shortener.Migrator
```

console app veya deployment migration step kullanılmalıdır.

---

# 70. Configuration

Typed options:

```text
ShortenerOptions
RedisOptions
DatabaseOptions
OidcOptions
CaptchaOptions
AnalyticsOptions
RateLimitOptions
ExpirationOptions
```

Secrets repository'ye yazılmamalıdır.

Environment variable / secret manager desteklenmelidir.

---

# 71. Docker Images

Önerilen:

```text
shortener-api
shortener-redirect
shortener-worker
shortener-web
```

Multi-stage Docker build.

Container:

- non-root user
- read-only filesystem mümkün olduğunca
- healthcheck
- graceful shutdown
- SIGTERM handling

desteklemelidir.

---

# 72. API Örnekleri

## Create Link

```http
POST /api/v1/links
Authorization: Bearer <token>
Content-Type: application/json
```

```json
{
  "url": "https://example.com/a/very/long/url",
  "domainId": "...",
  "alias": "campaign",
  "expiresAt": "2026-12-31T23:59:59Z"
}
```

Response:

```json
{
  "id": "...",
  "shortCode": "campaign",
  "shortUrl": "https://go.example.com/campaign",
  "destinationUrl": "https://example.com/a/very/long/url",
  "status": "Active",
  "expiresAt": "2026-12-31T23:59:59Z"
}
```

---

# 73. Public Create API

```http
POST /api/v1/public/links
```

```json
{
  "url": "https://example.com/long",
  "captchaToken": "..."
}
```

Response:

```json
{
  "shortUrl": "https://sho.rt/aB92xQz",
  "expiresAt": "2026-08-15T10:00:00Z"
}
```

Anonymous API custom expiration istemeyebilir.

Server policy belirler.

---

# 74. Search API

```text
GET /api/v1/links
  ?q=sale
  &status=Active
  &domainId=...
  &after=...
  &limit=50
```

---

# 75. Analytics API

```text
GET /api/v1/links/{id}/analytics
  ?from=2026-08-01
  &to=2026-08-08
  &interval=day
```

Response logical sections:

```text
summary
timeseries
countries
devices
browsers
referrers
utm
```

---

# 76. Link Deletion

Hard-delete yerine:

```text
soft delete
```

önerilir.

Deleted link redirect:

```text
404
```

veya tenant policy'ye göre branded page.

Abuse nedeniyle blocked:

```text
451
```

her durumda uygun olmayabilir; ilk sürümde 404/410 gibi neutral responses tercih edilebilir.

---

# 77. Link Ownership Transfer

Tenant içinde link owner değiştirilebilir.

Ancak link tenant değiştirmemelidir.

Başka tenant'a taşıma:

```text
copy/create new link
```

şeklinde yapılmalıdır.

Bu isolation modelini basitleştirir.

---

# 78. Custom Alias Conflict

Alias uniqueness:

```text
DomainId + normalized alias
```

bazında kontrol edilir.

Conflict:

```text
409 Conflict
```

döndürülür.

---

# 79. Domain Deletion

Aktif link bulunan domain doğrudan silinmemelidir.

Seçenek:

```text
Disable Domain
```

veya:

```text
Force delete
```

admin-only operasyonu.

---

# 80. Backup

PostgreSQL:

- daily backup
- point-in-time recovery production için önerilir

Redis:

- cache olduğundan backup zorunlu değildir

Raw analytics:

- retention politikasına göre

Domain/config:

- PostgreSQL source-of-truth

---

# 81. Data Retention

Config:

```text
ANALYTICS__RAW_RETENTION_DAYS=90
ANALYTICS__AGGREGATE_RETENTION_DAYS=0
ANONYMOUS__LINK_RETENTION_DAYS_AFTER_EXPIRATION=30
AUDIT__RETENTION_DAYS=365
```

`0` sınırsız anlamına gelebilir ancak açık şekilde dokümante edilmelidir.

---

# 82. Clock

Bütün backend zamanları:

```text
UTC
```

olarak tutulmalıdır.

.NET:

```text
TimeProvider
```

abstraction kullanılmalıdır.

Bu expiration testlerini deterministic hale getirir.

Frontend timezone'a dönüştürür.

---

# 83. IDs

Internal primary key için:

```text
UUID
```

kullanılabilir.

Short code ile internal ID aynı şey değildir.

Sequential ID'yi Base62'ye çevirerek short code üretmek:

- link sayısını tahmin ettirir
- enumeration kolaylaştırır

bu nedenle önerilmez.

---

# 84. Cache Stampede

Popüler bir link cache expire olduğunda yüzlerce request aynı anda DB'ye gitmemelidir.

İlk sürümde:

- cache TTL jitter
- short local single-flight

uygulanabilir.

Distributed lock her redirect için kullanılmamalıdır.

---

# 85. In-Memory L1 Cache

İleride gerekirse:

```text
L1 Memory Cache
+
L2 Redis
+
PostgreSQL
```

kullanılabilir.

Ancak Phase 1'de gereksizdir.

Önce Redis latency ölçülmelidir.

---

# 86. Configuration Modes

Repository üç mod destekleyebilir:

## Development

```text
localhost
captcha disabled/test key
docker postgres
docker redis
```

## Production

```text
OIDC
Turnstile
Redis
PostgreSQL
TLS proxy
```

## SelfHostedSimple

```text
single tenant
single domain
optional OIDC replacement
```

---

# 87. Open Source Authentication

Klogs deployment:

```text
idp.klogs.io
```

kullanacaktır.

Ancak proje public olduğu için auth provider hard-code edilmemelidir.

Configurable OIDC:

```text
AUTH__AUTHORITY
AUTH__CLIENT_ID
AUTH__CLIENT_SECRET
AUTH__SCOPES
```

kullanılmalıdır.

Böylece kullanıcı:

- Keycloak
- Authentik
- OpenIddict
- Entra ID
- başka OIDC provider

ile deploy edebilir.

---

# 88. Single Tenant Self-Hosted Mode

Küçük kullanıcılar tenant kavramıyla uğraşmak istemeyebilir.

Config:

```text
MULTITENANCY__MODE=SingleTenant
```

bu durumda sistem startup'ta default tenant oluşturabilir.

Ancak DB modeli yine TenantId kullanır.

Bu ileride migration gerektirmez.

---

# 89. Multi-Tenant SaaS Mode

```text
MULTITENANCY__MODE=MultiTenant
```

Tenant onboarding:

1. Tenant oluştur
2. Owner user ata
3. Default domain ata
4. Default policies oluştur
5. Welcome/configuration ekranı

---

# 90. Feature Flags

Harici feature-flag SaaS zorunlu değildir.

Basit config/db tabanlı feature flags:

```text
AnonymousLinks
CustomDomains
Analytics
ApiKeys
QrCodes
```

yeterlidir.

---

# 91. İlk Versiyonda Yapılmaması Gerekenler

Phase 1'e aşağıdakiler alınmamalıdır:

- MongoDB
- Kafka
- Elasticsearch
- Kubernetes requirement
- microservice explosion
- ML bot detection
- complex billing
- password protected links
- geo routing
- A/B routing
- deep-link mobile SDK
- browser extension

Önce redirect core kusursuz olmalıdır.

---

# 92. Fazlandırma

## Phase 0 — Foundation & Decisions

Amaç: repository, lisans politikası ve mimari temel.

Yapılacaklar:

- .NET solution
- Next.js app
- coding standards
- EditorConfig
- nullable enabled
- warnings as errors
- dependency policy + prohibited commercial NuGet policy
- dependency approval manifest
- LICENSE
- CONTRIBUTING.md
- SECURITY.md
- ADR structure
- Docker compose
- PostgreSQL
- Redis
- base CI
- OIDC configuration contract
- architecture tests

Çıkış kriteri:

```text
Backend + frontend + PostgreSQL + Redis local environment tek komutla kalkıyor.
```

---

## Phase 1 — Core Shortener MVP

Yapılacaklar:

- Tenant
- Domain
- ShortLink
- Base62/random code
- custom alias
- public create endpoint
- CAPTCHA
- rate limiting
- redirect service
- Redis cache
- PostgreSQL fallback
- expiration
- expiration worker
- management link CRUD
- OIDC authentication
- basic dashboard
- MSTest unit tests
- integration tests
- basic load test
- OpenAPI
- install docs

Çıkış kriteri:

```text
Production'da anonymous + authenticated link creation ve yüksek hızlı redirect çalışıyor.
```

---

## Phase 2 — Analytics

- click event pipeline
- bounded in-memory event buffer
- RabbitMQ background publisher
- RabbitMQ analytics consumer
- idempotent event processing
- batch persistence
- ClickEvent partitions
- daily aggregates
- countries
- browsers
- OS
- device
- referrers
- UTM
- bot classification
- retention worker
- analytics dashboard
- analytics API
- privacy settings

Çıkış kriteri:

```text
Analytics redirect latency'yi anlamlı şekilde etkilemeden raporlanıyor.
```

---

## Phase 3 — Custom Domains & Open Source Self Hosting

- domain onboarding
- TXT verification
- default domain
- multi-domain
- custom branded expired/not-found page
- single-tenant mode
- production docker compose
- reverse proxy examples
- backup docs
- upgrade docs
- environment reference
- self-host setup wizard veya CLI

Çıkış kriteri:

```text
GitHub'dan projeyi alan kişi kendi domain'i ile çalışan shortener kurabiliyor.
```

---

## Phase 4 — API Platform

- API keys
- API scopes
- idempotency
- tenant rate limits
- bulk URL create
- export
- SDK examples
- richer OpenAPI
- webhook foundations

---

## Phase 5 — Abuse & Operations

- abuse reporting
- destination blocklist
- suspicious traffic dashboard
- tenant suspension
- link moderation
- stronger bot classification
- admin operational dashboard
- performance dashboards
- outbox monitoring
- cache metrics

---

## Phase 6 — Advanced Features

İhtiyaca göre:

- QR Codes
- password protected links
- max-click links
- one-time links
- scheduled activation
- geo targeting
- device targeting
- A/B destinations
- deep links
- custom social metadata
- branded landing pages
- webhooks
- link bundles
- campaign templates

---

# 93. Definition of Done

Bir feature tamamlanmış sayılmak için:

- unit tests
- integration tests gerekiyorsa
- tenant isolation kontrolü
- authorization
- validation
- structured logging
- metrics gerekiyorsa
- error handling
- API docs
- migration gerekiyorsa
- frontend states
- loading/error/empty state
- accessibility
- security review
- dependency license validation

tamamlanmış olmalıdır.

---

# 94. Repository Dokümantasyonu

Önerilen:

```text
README.md
CONTRIBUTING.md
SECURITY.md
LICENSE
CHANGELOG.md

docs/
  ARCHITECTURE.md
  API.md
  INSTALLATION.md
  CONFIGURATION.md
  SELF_HOSTING.md
  CUSTOM_DOMAINS.md
  AUTHENTICATION.md
  ANALYTICS.md
  PRIVACY.md
  PERFORMANCE.md
  TROUBLESHOOTING.md
  DEPENDENCIES.md

docs/adr/
  0001-modular-monolith.md
  0002-postgresql-primary-store.md
  0003-redis-cache.md
  0004-short-code-generation.md
  0005-analytics-pipeline.md
  0006-expiration-semantics.md
```

---

# 95. Local Development

Hedef developer experience:

```bash
git clone ...
cd shortener

docker compose up -d postgres redis

dotnet run --project src/Shortener.Api
dotnet run --project src/Shortener.Redirect
dotnet run --project src/Shortener.Worker

cd web/shortener-web
npm install
npm run dev
```

Daha sonra tek komut:

```bash
docker compose up
```

ile tüm sistem çalıştırılabilir.

---

# 96. Environment Variables

Örnek:

```env
APP__DEFAULT_DOMAIN=localhost:8080

DATABASE__CONNECTION_STRING=...

REDIS__CONNECTION_STRING=localhost:6379

AUTH__AUTHORITY=https://idp.klogs.io
AUTH__CLIENT_ID=klogs.web
AUTH__CLIENT_SECRET=

CAPTCHA__PROVIDER=Turnstile
CAPTCHA__SITE_KEY=
CAPTCHA__SECRET_KEY=

ANONYMOUS__EXPIRATION_DAYS=7

ANALYTICS__RAW_RETENTION_DAYS=90

RATE_LIMIT__PUBLIC_CREATE_PER_MINUTE=5
```

Secret örnekleri gerçek README'ye yazılmamalıdır.

`.env.example` yalnızca placeholder içermelidir.

---

# 97. Production Architecture

```text
                         +------------------+
                         |   Reverse Proxy  |
                         +--------+---------+
                                  |
                 +----------------+----------------+
                 |                                 |
                 v                                 v
       +--------------------+             +------------------+
       | Shortener.Redirect |             | Shortener.Api    |
       +----------+---------+             +--------+---------+
                  |                                |
          +-------+-------+                        |
          |               |                        |
          v               v                        v
      +-------+      +------------+          +------------+
      | Redis |      | PostgreSQL |<---------| Worker     |
      +-------+      +------------+          +------------+
                           ^
                           |
                     +-----+------+
                     | Next.js Web|
                     +------------+
```

Management Web normalde API üzerinden backend'e erişir.

Redirect servisi ayrı yatay ölçeklenebilir.

---

# 98. Scale Strategy

İlk scaling:

```text
N x Redirect instances
N x API instances
N x Workers as appropriate
Redis
PostgreSQL
```

Worker job'larında concurrency control gerekir.

Expiration worker:

- SKIP LOCKED
- leader election
- distributed lease

gibi yöntemlerden biriyle çok instance-safe tasarlanmalıdır.

PostgreSQL batch processing için:

```text
FOR UPDATE SKIP LOCKED
```

yaklaşımı değerlendirilebilir.

---

# 99. Neden MongoDB Değil?

Bu proje için ilk tercih MongoDB olmamasının nedenleri:

- core data relational
- tenant/domain/link uniqueness önemli
- transaction ihtiyacı var
- analytics aggregation PostgreSQL ile yapılabilir
- ek database operasyonel maliyet
- MongoDB C# driver strict MIT-only policy ile uyumsuz
- premature polyglot persistence gereksiz

MongoDB ancak gerçek production metric'leri ihtiyaç gösterirse düşünülmelidir.

---

# 100. RabbitMQ Kullanım Kararı

RabbitMQ analytics ve background event processing için uygun görülmektedir.

Ancak broker'ı redirect request path'ine synchronous dependency yapmak yasaktır.

Kullanım alanları:

```text
Analytics click events
Domain/control-plane events
Background processing
Future webhook delivery
```

Control-plane event'lerinde:

```text
Transactional Outbox
```

zorunludur.

Analytics tarafında performans öncelikli olduğundan:

```text
Bounded Channel
 -> RabbitMQ background publisher
```

modeli kullanılır.

İlk implementasyonda doğrudan `RabbitMQ.Client` tercih edilir; gereksiz messaging framework abstraction'ı eklenmez.

---

# 101. En Kritik Tasarım Kuralları

Bu projede aşağıdaki kurallar değişmez kabul edilmelidir:

1. Redirect path mümkün olduğunca küçük olacaktır.
2. Redirect hiçbir zaman analytics write beklemeyecektir.
3. Redis cache, PostgreSQL source-of-truth olacaktır.
4. Redis arızası sistemi tamamen düşürmemelidir.
5. Expiration doğruluğu worker'a bağlı olmayacaktır.
6. Her tenant-owned entity TenantId taşıyacaktır.
7. TenantId client input'una güvenilmeyecektir.
8. Host + ShortCode birlikte linki tanımlar.
9. Her redirect için ShortLink row update edilmeyecektir.
10. Package licensing CI seviyesinde enforce edilecektir.
11. Authentication provider hard-coded olmayacaktır.
12. Self-hosting birinci sınıf senaryo olacaktır.
13. Anonymous endpoint abuse-ready tasarlanacaktır.
14. Privacy-first analytics varsayılan olacaktır.
15. Microservice'e ancak ölçülmüş ihtiyaç varsa ayrılacaktır.

---

# 102. Son Mimari Karar Özeti

| Konu | Karar |
|---|---|
| Backend | ASP.NET Core / .NET 10 LTS |
| Frontend | Next.js + TypeScript |
| Mimari | Modular Monolith + ayrılabilir Redirect/Worker |
| Primary DB | PostgreSQL |
| MongoDB | İlk sürümde yok |
| Cache | Redis |
| Queue | RabbitMQ + RabbitMQ.Client; control-plane için Transactional Outbox |
| Redirect Cache | Redis Cache-Aside |
| CAPTCHA | Cloudflare Turnstile |
| Authentication | Generic OIDC; Klogs: `https://idp.klogs.io`, ClientId `klogs.web` |
| Multi-tenancy | Shared DB / Shared Schema / TenantId |
| Short Code | Random cryptographic Base62 |
| Default Redirect | 302 |
| Expiration | `ExpiresAt` + runtime check + sweep worker |
| Expired Response | 410 Gone |
| Analytics | Async events + PostgreSQL partitions + aggregates |
| IP Privacy | Raw IP off by default; hash/anonymize |
| Rate Limit | ASP.NET Core built-in |
| Unit Test | xUnit |
| Integration Test | xUnit + Testcontainers for .NET |
| Load Test | k6 + BCL HttpClient fallback |
| API Docs | OpenAPI + `/docs` markdown |
| Deployment | Docker / self-hosted |
| Package Policy | OSI/permissive OSS; production/team/revenue bazlı commercial lisans veya license-key gerektiren NuGet'ler yasak |

---

# 103. Önerilen İlk Sprint

İlk sprintte doğrudan UI ile başlamak yerine vertical slice hazırlanmalıdır.

Hedef:

```text
Create Short Link
        +
Redis-backed Redirect
        +
Expiration
        +
Unit/Integration/Load Test
```

Akış tamamen çalıştıktan sonra management UI genişletilmelidir.

İlk vertical slice:

```text
POST /api/v1/public/links
GET /{shortCode}
```

ve:

```text
PostgreSQL
Redis
Turnstile abstraction
Rate Limit
```

ile production davranışının çekirdeğini kanıtlamalıdır.

---

# 104. İlk Teknik Milestone

Milestone tamamlandığında şu test çalışmalıdır:

```text
1. Long URL oluştur.
2. Short code al.
3. İlk redirect PostgreSQL'den gelsin.
4. İkinci redirect Redis'ten gelsin.
5. Analytics request'i bloklamasın.
6. Link expire olduğunda worker çalışmasa bile 410 dönsün.
7. Redis kapandığında link PostgreSQL üzerinden çalışsın.
8. Başka tenant aynı code'u kendi domain'inde kullanabilsin.
9. Anonymous create rate-limit ve CAPTCHA ile korunsun.
10. Load test p95/p99 raporu üretsin.
```

Bu milestone projenin mimari proof-of-concept'i olacaktır.

---

# 105. Sonuç

Bu URL Shortener için başlangıçtan itibaren devasa dağıtık sistem kurmak yerine:

```text
PostgreSQL
+
Redis
+
ASP.NET Core
+
Next.js
+
Modular Monolith
+
separate high-performance Redirect process
+
background workers
```

kombinasyonu en dengeli yaklaşımdır.

En önemli performans kararı database türünden çok redirect hot-path tasarımıdır.

Başarılı redirect path ideal olarak:

```text
HTTP Request
 -> Host/Code parse
 -> Redis GET
 -> expiration/status check
 -> analytics event non-blocking buffer
 -> HTTP redirect
```

olmalıdır.

PostgreSQL yalnızca cache miss sırasında redirect path'e girer.

Analytics, expiration cleanup, aggregation ve diğer operasyonel işler request path'inden ayrılır.

Bu yaklaşım küçük tek sunucu kurulumundan başlayıp redirect instance'larını yatay çoğaltarak çok yüksek trafiğe kadar ilerleyebilir ve aynı kod tabanı self-hosted açık kaynak dağıtım için kullanılabilir.
