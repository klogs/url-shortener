using Shortener.Domain.Enums;

namespace Shortener.Api.Endpoints.Billing;

public sealed record ChangeTenantPlanRequest(TenantPlan Plan);
