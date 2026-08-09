# URL Shortener — Wiki

Açık kaynak, self-hosted, multi-tenant URL kısaltma platformuna hoş geldiniz.

## İçindekiler

- [Projeyi Çalıştırma](Getting-Started) — Docker ve debug/local modları
- [Özellikler](Features) — Tüm özellikler ve konfigürasyon rehberi

## Mimari Özet

```
Shortener.Api        → Kontrol paneli API (link/tenant/domain/API key yönetimi)
Shortener.Redirect   → Veri katmanı: /{shortCode} yönlendirme (hızlı hot-path)
Shortener.Worker     → Asenkron: analitik toplama, expiration, outbox, cache
Shortener.Domain     → Entity'ler, value object'ler, domain event'leri
Shortener.Application → Use case'ler, komutlar, sorgular, arayüzler
Shortener.Infrastructure → EF Core, Dapper, Redis, RabbitMQ implementasyonları
Shortener.Migrator   → Bağımsız migration konsol uygulaması
shortener-web        → Next.js + TypeScript frontend
```

Redirect hot-path:

```
İstek → Host normalize et → Redis lookup
  HIT → Status/ExpiresAt doğrula → Yönlendir
  MISS → PostgreSQL → Redis SET → Yönlendir
```

Analitik olaylar redirect path'ini **hiçbir zaman** bloklamaz; `Bounded Channel` aracılığıyla RabbitMQ → PostgreSQL zinciriyle asenkron işlenir.
