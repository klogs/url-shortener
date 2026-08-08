using Shortener.Domain.Enums;

namespace Shortener.Domain.Entities;

public sealed class Tenant
{
    private Tenant() { }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public bool IsActive { get; private set; }
    public TenantPlan Plan { get; private set; }

    public static Tenant Create(string name, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new Tenant
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            CreatedAtUtc = now,
            IsActive = true,
            Plan = TenantPlan.Free,
        };
    }

    public void Deactivate() => IsActive = false;

    public void ChangePlan(TenantPlan plan) => Plan = plan;
}
