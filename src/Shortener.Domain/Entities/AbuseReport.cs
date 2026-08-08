namespace Shortener.Domain.Entities;

public sealed class AbuseReport
{
    private AbuseReport() { }

    public Guid Id { get; private set; }
    public Guid LinkId { get; private set; }
    public Guid TenantId { get; private set; }
    public string ShortCode { get; private set; } = default!;
    public string NormalizedHost { get; private set; } = default!;
    public string? ReporterIp { get; private set; }
    public string? Reason { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static AbuseReport Create(
        Guid linkId,
        Guid tenantId,
        string shortCode,
        string normalizedHost,
        string? reporterIp,
        string? reason,
        DateTimeOffset now)
    {
        return new AbuseReport
        {
            Id = Guid.NewGuid(),
            LinkId = linkId,
            TenantId = tenantId,
            ShortCode = shortCode,
            NormalizedHost = normalizedHost,
            ReporterIp = reporterIp,
            Reason = reason?.Length > 500 ? reason[..500] : reason,
            CreatedAtUtc = now,
        };
    }
}
