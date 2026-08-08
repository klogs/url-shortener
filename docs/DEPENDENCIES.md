# Dependency Registry

All third-party NuGet packages must be recorded here before merging.

**Policy**: Only OSI-compatible permissive licenses accepted (MIT, Apache-2.0, BSD-2-Clause, BSD-3-Clause, ISC, PostgreSQL License).
MediatR and AutoMapper are permanently banned regardless of version. See README §5 for full policy.

---

## Approval Checklist (per new dependency)

Before approving, confirm all of the following:

- [ ] Current license is OSI-compatible permissive
- [ ] No production commercial license required
- [ ] No team/revenue limit in community edition
- [ ] No license key required
- [ ] No paid-only features in our usage path
- [ ] Transitive dependencies reviewed
- [ ] Active maintenance / recent releases
- [ ] No known license-change risk

---

## Approved Dependencies

### Microsoft.Extensions.Hosting `10.0.10`

| Field | Value |
|---|---|
| Package | `Microsoft.Extensions.Hosting` |
| Version | `10.0.10` |
| License | MIT |
| Repository | https://github.com/dotnet/runtime |
| Purpose | Generic host for Shortener.Worker background service |
| CommercialLicenseRequired | No |
| LicenseKeyRequired | No |
| RevenueOrTeamLimit | No |
| ApprovedAt | 2026-08-08 |

### xunit `2.9.3`

| Field | Value |
|---|---|
| Package | `xunit` |
| Version | `2.9.3` |
| License | Apache-2.0 |
| Repository | https://github.com/xunit/xunit |
| Purpose | Unit and integration test framework |
| CommercialLicenseRequired | No |
| LicenseKeyRequired | No |
| RevenueOrTeamLimit | No |
| ApprovedAt | 2026-08-08 |

### Microsoft.NET.Test.Sdk `17.14.1`

| Field | Value |
|---|---|
| Package | `Microsoft.NET.Test.Sdk` |
| Version | `17.14.1` |
| License | MIT |
| Repository | https://github.com/microsoft/vstest |
| Purpose | Test runner infrastructure |
| CommercialLicenseRequired | No |
| LicenseKeyRequired | No |
| RevenueOrTeamLimit | No |
| ApprovedAt | 2026-08-08 |

### coverlet.collector `6.0.4`

| Field | Value |
|---|---|
| Package | `coverlet.collector` |
| Version | `6.0.4` |
| License | MIT |
| Repository | https://github.com/coverlet-coverage/coverlet |
| Purpose | Code coverage collection |
| CommercialLicenseRequired | No |
| LicenseKeyRequired | No |
| RevenueOrTeamLimit | No |
| ApprovedAt | 2026-08-08 |

### Npgsql.EntityFrameworkCore.PostgreSQL `10.0.3`

| Field | Value |
|---|---|
| Package | `Npgsql.EntityFrameworkCore.PostgreSQL` |
| Version | `10.0.3` |
| License | PostgreSQL License |
| Repository | https://github.com/npgsql/efcore.pg |
| Purpose | EF Core provider for PostgreSQL |
| CommercialLicenseRequired | No |
| LicenseKeyRequired | No |
| RevenueOrTeamLimit | No |
| ApprovedAt | 2026-08-08 |

### Microsoft.Extensions.Configuration.Binder `10.0.0`

| Field | Value |
|---|---|
| Package | `Microsoft.Extensions.Configuration.Binder` |
| Version | `10.0.0` |
| License | MIT |
| Repository | https://github.com/dotnet/runtime |
| Purpose | `IConfiguration.Get<T>()` extension for strongly-typed options binding |
| CommercialLicenseRequired | No |
| LicenseKeyRequired | No |
| RevenueOrTeamLimit | No |
| ApprovedAt | 2026-08-08 |

### RabbitMQ.Client `7.1.2`

| Field | Value |
|---|---|
| Package | `RabbitMQ.Client` |
| Version | `7.1.2` |
| License | Apache-2.0 |
| Repository | https://github.com/rabbitmq/rabbitmq-dotnet-client |
| Purpose | Async RabbitMQ publisher (analytics) and consumer (Worker) |
| CommercialLicenseRequired | No |
| LicenseKeyRequired | No |
| RevenueOrTeamLimit | No |
| ApprovedAt | 2026-08-08 |

### Dapper `2.1.66`

| Field | Value |
|---|---|
| Package | `Dapper` |
| Version | `2.1.66` |
| License | Apache-2.0 |
| Repository | https://github.com/DapperLib/Dapper |
| Purpose | Lightweight ORM for high-throughput ClickEvent inserts |
| CommercialLicenseRequired | No |
| LicenseKeyRequired | No |
| RevenueOrTeamLimit | No |
| ApprovedAt | 2026-08-08 |

### Npgsql `10.0.3`

| Field | Value |
|---|---|
| Package | `Npgsql` |
| Version | `10.0.3` |
| License | PostgreSQL License |
| Repository | https://github.com/npgsql/npgsql |
| Purpose | Low-level PostgreSQL driver used by Dapper in ClickEventRepository |
| CommercialLicenseRequired | No |
| LicenseKeyRequired | No |
| RevenueOrTeamLimit | No |
| ApprovedAt | 2026-08-08 |

### Microsoft.AspNetCore.Authentication.JwtBearer `10.0.0`

| Field | Value |
|---|---|
| Package | `Microsoft.AspNetCore.Authentication.JwtBearer` |
| Version | `10.0.0` |
| License | MIT |
| Repository | https://github.com/dotnet/aspnetcore |
| Purpose | OIDC JWT bearer authentication for Shortener.Api |
| CommercialLicenseRequired | No |
| LicenseKeyRequired | No |
| RevenueOrTeamLimit | No |
| ApprovedAt | 2026-08-08 |

### Microsoft.EntityFrameworkCore.Design `10.0.3`

| Field | Value |
|---|---|
| Package | `Microsoft.EntityFrameworkCore.Design` |
| Version | `10.0.3` |
| License | MIT |
| Repository | https://github.com/dotnet/efcore |
| Purpose | Design-time EF Core tooling for `dotnet ef migrations` |
| CommercialLicenseRequired | No |
| LicenseKeyRequired | No |
| RevenueOrTeamLimit | No |
| ApprovedAt | 2026-08-08 |

### StackExchange.Redis `2.8.41`

| Field | Value |
|---|---|
| Package | `StackExchange.Redis` |
| Version | `2.8.41` |
| License | MIT |
| Repository | https://github.com/StackExchange/StackExchange.Redis |
| Purpose | Redis client for redirect cache (IRedirectCache implementation) |
| CommercialLicenseRequired | No |
| LicenseKeyRequired | No |
| RevenueOrTeamLimit | No |
| ApprovedAt | 2026-08-08 |

### QRCoder `1.6.0`

| Field | Value |
|---|---|
| Package | `QRCoder` |
| Version | `1.6.0` |
| License | MIT |
| Repository | https://github.com/codebude/QRCoder |
| Purpose | Server-side QR code generation (PNG + SVG) for short link QR endpoint |
| CommercialLicenseRequired | No |
| LicenseKeyRequired | No |
| RevenueOrTeamLimit | No |
| ApprovedAt | 2026-08-08 |

### MaxMind.Db `4.0.0`

| Field | Value |
|---|---|
| Package | `MaxMind.Db` |
| Version | `4.0.0` |
| License | Apache-2.0 |
| Repository | https://github.com/maxmind/MaxMind-DB-Reader-dotnet |
| Purpose | Read GeoLite2-Country.mmdb for IP-to-country resolution in geo routing |
| CommercialLicenseRequired | No |
| LicenseKeyRequired | No (DB file requires free MaxMind account) |
| RevenueOrTeamLimit | No |
| ApprovedAt | 2026-08-08 |

### AspNetCore.HealthChecks.Npgsql `9.0.0`

| Field | Value |
|---|---|
| Package | `AspNetCore.HealthChecks.Npgsql` |
| Version | `9.0.0` |
| License | Apache-2.0 |
| Repository | https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks |
| Purpose | PostgreSQL readiness check for Api and Redirect (`/health/ready`) |
| CommercialLicenseRequired | No |
| LicenseKeyRequired | No |
| RevenueOrTeamLimit | No |
| ApprovedAt | 2026-08-09 |

### AspNetCore.HealthChecks.Redis `9.0.0`

| Field | Value |
|---|---|
| Package | `AspNetCore.HealthChecks.Redis` |
| Version | `9.0.0` |
| License | Apache-2.0 |
| Repository | https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks |
| Purpose | Redis readiness check for Api and Redirect (`/health/ready`) |
| CommercialLicenseRequired | No |
| LicenseKeyRequired | No |
| RevenueOrTeamLimit | No |
| ApprovedAt | 2026-08-09 |

### AspNetCore.HealthChecks.RabbitMQ `9.0.0`

| Field | Value |
|---|---|
| Package | `AspNetCore.HealthChecks.RabbitMQ` |
| Version | `9.0.0` |
| License | Apache-2.0 |
| Repository | https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks |
| Purpose | RabbitMQ readiness check for Api (`/health/ready`) |
| CommercialLicenseRequired | No |
| LicenseKeyRequired | No |
| RevenueOrTeamLimit | No |
| ApprovedAt | 2026-08-09 |

### AspNetCore.HealthChecks.UI.Client `9.0.0`

| Field | Value |
|---|---|
| Package | `AspNetCore.HealthChecks.UI.Client` |
| Version | `9.0.0` |
| License | Apache-2.0 |
| Repository | https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks |
| Purpose | `UIResponseWriter.WriteHealthCheckUIResponse` for JSON health check output |
| CommercialLicenseRequired | No |
| LicenseKeyRequired | No |
| RevenueOrTeamLimit | No |
| ApprovedAt | 2026-08-09 |

### OpenTelemetry.Extensions.Hosting `1.17.0`

| Field | Value |
|---|---|
| Package | `OpenTelemetry.Extensions.Hosting` |
| Version | `1.17.0` |
| License | Apache-2.0 |
| Repository | https://github.com/open-telemetry/opentelemetry-dotnet |
| Purpose | `AddOpenTelemetry()` host integration for all three services |
| CommercialLicenseRequired | No |
| LicenseKeyRequired | No |
| RevenueOrTeamLimit | No |
| ApprovedAt | 2026-08-09 |

### OpenTelemetry.Instrumentation.AspNetCore `1.17.0`

| Field | Value |
|---|---|
| Package | `OpenTelemetry.Instrumentation.AspNetCore` |
| Version | `1.17.0` |
| License | Apache-2.0 |
| Repository | https://github.com/open-telemetry/opentelemetry-dotnet |
| Purpose | ASP.NET Core HTTP request tracing + metrics for Api and Redirect |
| CommercialLicenseRequired | No |
| LicenseKeyRequired | No |
| RevenueOrTeamLimit | No |
| ApprovedAt | 2026-08-09 |

### OpenTelemetry.Instrumentation.Http `1.17.0`

| Field | Value |
|---|---|
| Package | `OpenTelemetry.Instrumentation.Http` |
| Version | `1.17.0` |
| License | Apache-2.0 |
| Repository | https://github.com/open-telemetry/opentelemetry-dotnet |
| Purpose | HttpClient tracing + metrics for Api and Redirect |
| CommercialLicenseRequired | No |
| LicenseKeyRequired | No |
| RevenueOrTeamLimit | No |
| ApprovedAt | 2026-08-09 |

### OpenTelemetry.Exporter.OpenTelemetryProtocol `1.17.0`

| Field | Value |
|---|---|
| Package | `OpenTelemetry.Exporter.OpenTelemetryProtocol` |
| Version | `1.17.0` |
| License | Apache-2.0 |
| Repository | https://github.com/open-telemetry/opentelemetry-dotnet |
| Purpose | OTLP gRPC exporter for traces and metrics (points at OTEL Collector) |
| CommercialLicenseRequired | No |
| LicenseKeyRequired | No |
| RevenueOrTeamLimit | No |
| ApprovedAt | 2026-08-09 |

### OpenTelemetry.Exporter.Prometheus.AspNetCore `1.17.0-beta.1`

| Field | Value |
|---|---|
| Package | `OpenTelemetry.Exporter.Prometheus.AspNetCore` |
| Version | `1.17.0-beta.1` |
| License | Apache-2.0 |
| Repository | https://github.com/open-telemetry/opentelemetry-dotnet |
| Purpose | `/metrics` Prometheus scrape endpoint on Api and Redirect |
| CommercialLicenseRequired | No |
| LicenseKeyRequired | No |
| RevenueOrTeamLimit | No |
| ApprovedAt | 2026-08-09 |

### Serilog.AspNetCore `9.0.0`

| Field | Value |
|---|---|
| Package | `Serilog.AspNetCore` |
| Version | `9.0.0` |
| License | Apache-2.0 |
| Repository | https://github.com/serilog/serilog-aspnetcore |
| Purpose | Structured JSON logging for Api + Redirect; request logging middleware |
| CommercialLicenseRequired | No |
| LicenseKeyRequired | No |
| RevenueOrTeamLimit | No |
| ApprovedAt | 2026-08-09 |

### Serilog.Extensions.Hosting `9.0.0`

| Field | Value |
|---|---|
| Package | `Serilog.Extensions.Hosting` |
| Version | `9.0.0` |
| License | Apache-2.0 |
| Repository | https://github.com/serilog/serilog-extensions-hosting |
| Purpose | `AddSerilog` for Worker generic host |
| CommercialLicenseRequired | No |
| LicenseKeyRequired | No |
| RevenueOrTeamLimit | No |
| ApprovedAt | 2026-08-09 |

### Serilog.Formatting.Compact `3.0.0`

| Field | Value |
|---|---|
| Package | `Serilog.Formatting.Compact` |
| Version | `3.0.0` |
| License | Apache-2.0 |
| Repository | https://github.com/serilog/serilog-formatting-compact |
| Purpose | `CompactJsonFormatter` for machine-readable console output |
| CommercialLicenseRequired | No |
| LicenseKeyRequired | No |
| RevenueOrTeamLimit | No |
| ApprovedAt | 2026-08-09 |

### Serilog.Settings.Configuration `9.0.0`

| Field | Value |
|---|---|
| Package | `Serilog.Settings.Configuration` |
| Version | `9.0.0` |
| License | Apache-2.0 |
| Repository | https://github.com/serilog/serilog-settings-configuration |
| Purpose | `ReadFrom.Configuration` for appsettings-based log level overrides in Worker |
| CommercialLicenseRequired | No |
| LicenseKeyRequired | No |
| RevenueOrTeamLimit | No |
| ApprovedAt | 2026-08-09 |

### Serilog.Sinks.Console `6.0.0`

| Field | Value |
|---|---|
| Package | `Serilog.Sinks.Console` |
| Version | `6.0.0` |
| License | Apache-2.0 |
| Repository | https://github.com/serilog/serilog-sinks-console |
| Purpose | Console sink for Worker (Api/Redirect get it transitively via Serilog.AspNetCore) |
| CommercialLicenseRequired | No |
| LicenseKeyRequired | No |
| RevenueOrTeamLimit | No |
| ApprovedAt | 2026-08-09 |

### Testcontainers.PostgreSql `4.4.0`

| Field | Value |
|---|---|
| Package | `Testcontainers.PostgreSql` |
| Version | `4.4.0` |
| License | MIT |
| Repository | https://github.com/testcontainers/testcontainers-dotnet |
| Purpose | Spin up real Postgres containers for integration tests |
| CommercialLicenseRequired | No |
| LicenseKeyRequired | No |
| RevenueOrTeamLimit | No |
| ApprovedAt | 2026-08-08 |

### Testcontainers.Redis `4.4.0`

| Field | Value |
|---|---|
| Package | `Testcontainers.Redis` |
| Version | `4.4.0` |
| License | MIT |
| Repository | https://github.com/testcontainers/testcontainers-dotnet |
| Purpose | Spin up real Redis containers for integration tests |
| CommercialLicenseRequired | No |
| LicenseKeyRequired | No |
| RevenueOrTeamLimit | No |
| ApprovedAt | 2026-08-08 |

### Microsoft.AspNetCore.Mvc.Testing `10.0.0`

| Field | Value |
|---|---|
| Package | `Microsoft.AspNetCore.Mvc.Testing` |
| Version | `10.0.0` |
| License | MIT |
| Repository | https://github.com/dotnet/aspnetcore |
| Purpose | WebApplicationFactory for in-process integration testing of ASP.NET Core apps |
| CommercialLicenseRequired | No |
| LicenseKeyRequired | No |
| RevenueOrTeamLimit | No |
| ApprovedAt | 2026-08-08 |

---

## Pending / Under Review

_None_

---

## Permanently Banned

| Package | Reason |
|---|---|
| `MediatR` | Commercial license from v13+ |
| `AutoMapper` | Commercial license from v15+ |

---

## Infrastructure (Docker Images)

| Image | Version | License | Purpose |
|---|---|---|---|
| `postgres` | 17-alpine | PostgreSQL License | Primary database |
| `redis` | 7-alpine | BSD-3-Clause | Redirect cache + rate limiting |
| `rabbitmq` | 4-management-alpine | MPL-2.0 | Async messaging (Phase 2) |
