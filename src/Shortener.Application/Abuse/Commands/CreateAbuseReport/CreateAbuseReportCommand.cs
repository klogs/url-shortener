namespace Shortener.Application.Abuse.Commands.CreateAbuseReport;

public sealed record CreateAbuseReportCommand(
    string ShortCode,
    string NormalizedHost,
    string? ReporterIp,
    string? Reason);
