using Shortener.Application.Interfaces;
using Shortener.Domain.Entities;

namespace Shortener.Application.Abuse.Commands.CreateAbuseReport;

public sealed class CreateAbuseReportHandler(
    IShortLinkRepository links,
    IAbuseReportRepository reports,
    TimeProvider time)
{
    public async Task HandleAsync(CreateAbuseReportCommand cmd, CancellationToken ct = default)
    {
        var link = await links.GetByHostAndCodeAsync(cmd.NormalizedHost, cmd.ShortCode, ct)
            ?? throw new InvalidOperationException("Link not found.");

        var report = AbuseReport.Create(
            link.Id,
            link.TenantId,
            cmd.ShortCode,
            cmd.NormalizedHost,
            cmd.ReporterIp,
            cmd.Reason,
            time.GetUtcNow());

        await reports.InsertAsync(report, ct);

        link.IncrementReportCount();
        await links.UpdateAsync(link, ct);
    }
}
