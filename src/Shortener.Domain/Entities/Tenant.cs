namespace Shortener.Domain.Entities;

public sealed class Tenant
{
    private Tenant() { }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public bool IsActive { get; private set; }

    public static Tenant Create(string name, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new Tenant
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            CreatedAtUtc = now,
            IsActive = true
        };
    }

    public void Deactivate() => IsActive = false;
}
