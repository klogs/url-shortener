# Özellikler

## Üyeliksiz (Anonymous) Link Kısaltma

Sisteme giriş yapmadan kısa link oluşturulabilir. Her anonymous oluşturma isteği CAPTCHA doğrulaması ve rate limiting gerektirir.

- Varsayılan expiration: 7 gün (konfigüre edilebilir)
- Custom alias: kapalı
- Analytics paneli: kapalı
- CAPTCHA: zorunlu

---

## CAPTCHA Desteği

`ICaptchaVerifier` arayüzü üzerinden sağlayıcı bağımsız CAPTCHA entegrasyonu.

### Cloudflare Turnstile (varsayılan)

```env
CAPTCHA__PROVIDER=Turnstile
CAPTCHA__SITE_KEY=<site-key>
CAPTCHA__SECRET_KEY=<secret-key>
```

Turnstile anahtarlarını [Cloudflare Dashboard](https://dash.cloudflare.com/) → Zero Trust → Turnstile bölümünden alabilirsiniz.

### Google reCAPTCHA

```env
CAPTCHA__PROVIDER=ReCaptcha
CAPTCHA__SITE_KEY=<site-key>
CAPTCHA__SECRET_KEY=<secret-key>
```

reCAPTCHA anahtarlarını [Google reCAPTCHA Admin](https://www.google.com/recaptcha/admin) panelinden alabilirsiniz.

### Geliştirme Ortamı (CAPTCHA Devre Dışı)

```env
CAPTCHA__PROVIDER=Disabled
```

---

## Kimliği Doğrulanmış Kullanıcı Yönetimi

Giriş yapmış kullanıcılar:

- Link oluşturabilir ve yönetebilir (etkinleştir/devre dışı bırak, expiration değiştir)
- Custom alias seçebilir (örn. `/summer-sale`)
- Link listesini ve analitiğini görüntüleyebilir
- QR Code üretebilir
- API Key oluşturabilir
- Özel domain kullanabilir

---

## Multi-Tenancy

Her tenant kendi kullanıcılarına, domainlerine, linklerine ve API anahtarlarına sahiptir. Tenant izolasyonu `TenantId` kolon modeli (Shared Database / Shared Schema) ile sağlanır.

- `MULTITENANCY__MODE=SingleTenant`: Tek kullanıcı/ekip için; başlangıçta varsayılan tenant otomatik oluşturulur.
- `MULTITENANCY__MODE=MultiTenant`: Tam çok kiracılı mod.

> Güvenlik: `TenantId` asla istemci girişinden alınmaz; her zaman token/session üzerinden çözümlenir.

---

## Özel Domain Desteği

Her tenant birden fazla özel domain kullanabilir.

**Doğrulama adımları:**
1. Domain yönetim panelinden yeni domain ekleyin.
2. Sistem bir TXT token oluşturur: `_shortener-verification.<domain>`
3. DNS sağlayıcınızda TXT kaydını ekleyin.
4. Worker periyodik olarak DNS'i kontrol eder ve domain'i aktif eder.

Aynı short code farklı domainlerde bağımsız linklere karşılık gelebilir:
```
a.com/hello  →  bir link
b.com/hello  →  farklı bir link
```

---

## Yüksek Hızlı Redirect

Redirect hot-path PostgreSQL'e gitmeden Redis üzerinden yanıtlanır:

```
İstek → Redis lookup
  HIT  → Status/ExpiresAt kontrolü → Yönlendir
  MISS → PostgreSQL → Redis SET → Yönlendir
```

Redis kapalı olsa bile servis PostgreSQL fallback ile çalışmaya devam eder.

Desteklenen redirect türleri: `301`, `302`, `307`, `308` (link başına yapılandırılabilir).

---

## Link Expiration

- `ExpiresAt NULL` → link hiç expire olmaz.
- Expiration kontrolü her redirect isteğinde runtime'da yapılır; worker çalışmasa bile expired link `410 Gone` döner.
- Anonymous linkler varsayılan olarak 7 günde expire olur.
- Kullanıcılar `1 saat`, `1 gün`, `7 gün`, `30 gün`, `90 gün`, `1 yıl`, `Hiçbir zaman` veya özel tarih seçebilir.

---

## Analitik Pipeline

Her redirect olayı, redirect latency'sini etkilemeden asenkron olarak işlenir:

```
Redirect → Bounded Channel<ClickEvent> → RabbitMQ → Tüketici → PostgreSQL
```

Her link için raporlanan metrikler:

- Toplam ve benzersiz tıklama tahmini
- Zaman serisi (dakika / saat / gün)
- Coğrafya (ülke, bölge)
- Teknoloji (tarayıcı, işletim sistemi, cihaz türü)
- Trafik kaynağı (referrer, direkt, arama, sosyal)
- UTM kampanya parametreleri (source, medium, campaign)
- Bot/insan trafik ayrımı

Ham click event'leri varsayılan 90 gün, aggregate istatistikler süresiz saklanır.

---

## A/B Test

Bir link birden fazla hedef URL'ye yönlendirilebilir. Her varyanta ağırlık atanır:

```
Variant A: https://landing-v1.example.com  — %70
Variant B: https://landing-v2.example.com  — %30
```

Analitik her varyant için ayrı toplanır.

---

## Coğrafi Yönlendirme (Geo Routing)

İstek IP'sinin ülke koduna göre farklı hedeflere yönlendirme:

```
TR → https://tr.example.com
DE → https://de.example.com
*  → https://example.com (varsayılan)
```

---

## QR Code

Her link için QR Code endpoint'i üzerinden PNG formatında QR Code oluşturulabilir:

```
GET /api/v1/links/{id}/qr
```

---

## API Keys

Programatik link yönetimi için tenant-scoped API anahtarları.

- Anahtar yalnızca oluşturulduğu an görüntülenir; veritabanında hash olarak saklanır.
- Format: `uks_live_xxxxx`
- Kapsam tabanlı izinler: `links.read`, `links.write`, `analytics.read`

```http
POST /api/v1/links
Authorization: ApiKey uks_live_xxxxx
```

---

## Webhook Desteği

Belirli olaylarda HTTP POST isteği gönderilir:

| Olay | Açıklama |
|---|---|
| `link.created` | Yeni link oluşturuldu |
| `link.updated` | Link güncellendi |
| `link.expiring_soon` | Link 7 gün içinde expire olacak |
| `link.auto_blocked` | Abuse nedeniyle link otomatik bloklandı |

Webhook teslimat geçmişi:

```
GET /api/v1/webhooks/{id}/deliveries
```

---

## Abuse Prevention

- CAPTCHA ve IP rate limiting (anonim oluşturma)
- URL scheme allowlist (yalnızca `http`/`https`)
- Destination hostname blocklist
- Public abuse report endpoint (`POST /api/v1/abuse-reports`)
- Yüksek rapor sayısına ulaşan linkler worker tarafından otomatik bloklanır
- Tenant askıya alma (admin)

---

## Billing & Quota

Her tenant bir plana aittir: **Free**, **Pro** veya **Enterprise**.

- Link oluşturma ve domain sayısı plan limitine bağlıdır.
- Mevcut kullanım:

```
GET /api/v1/tenants/me/usage
```

- Plan güncelleme (admin):

```
PUT /api/v1/admin/tenants/{id}/plan
```

---

## Kimlik Doğrulama (OIDC)

Yönetim paneli OIDC Authorization Code + PKCE akışını kullanır. Provider'dan bağımsız olarak yapılandırılabilir:

```env
AUTH__AUTHORITY=https://idp.example.com
AUTH__CLIENT_ID=shortener.web
AUTH__CLIENT_SECRET=
```

Popüler provider örnekleri:

| Provider | Authority |
|---|---|
| Keycloak | `https://keycloak.example.com/realms/myrealm` |
| Authentik | `https://authentik.example.com/application/o/shortener/` |
| Entra ID | `https://login.microsoftonline.com/{tenantId}/v2.0` |
| Klogs | `https://idp.klogs.io` |

---

## Observability

- **Yapılandırılmış loglama**: Serilog JSON formatı
- **Distributed tracing ve metrikler**: OpenTelemetry (OTLP exporter)
- **Grafana / Prometheus**: `docker-compose.observability.yml` ile tam stack
- **Health endpoints**: `/health/live` (canlılık), `/health/ready` (hazırlık)

Redirect servisine ait temel metrikler:

```
redirect_requests_total
redirect_cache_hits_total
redirect_cache_misses_total
redirect_duration_ms (p50/p95/p99)
analytics_events_dropped_total
```

---

## Rate Limiting

ASP.NET Core built-in rate limiting kullanılır.

- Anonim link oluşturma: IP sliding-window (varsayılan: 5/dakika, 30/saat)
- Authenticated API: TenantId / API Key bazlı
- Redirect: DDoS koruma seviyesinde global limit

```env
RATE_LIMIT__PUBLIC_CREATE_PER_MINUTE=5
RATE_LIMIT__PUBLIC_CREATE_PER_HOUR=30
```
