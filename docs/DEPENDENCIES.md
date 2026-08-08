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
