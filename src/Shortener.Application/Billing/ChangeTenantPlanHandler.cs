using Shortener.Application.Interfaces;
using Shortener.Domain.Enums;

namespace Shortener.Application.Billing;

public sealed record ChangeTenantPlanCommand(Guid TenantId, TenantPlan NewPlan);

public sealed class ChangeTenantPlanHandler(ITenantRepository tenants)
{
    public async Task HandleAsync(ChangeTenantPlanCommand cmd, CancellationToken ct = default)
    {
        var tenant = await tenants.GetByIdAsync(cmd.TenantId, ct)
            ?? throw new InvalidOperationException($"Tenant {cmd.TenantId} not found.");

        tenant.ChangePlan(cmd.NewPlan);
        await tenants.UpdateAsync(tenant, ct);
    }
}
