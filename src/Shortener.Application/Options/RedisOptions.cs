using System.ComponentModel.DataAnnotations;

namespace Shortener.Application.Options;

public sealed class RedisOptions
{
    public const string SectionName = "Redis";

    [Required]
    public string ConnectionString { get; init; } = string.Empty;

    public int RedirectCacheTtlSeconds { get; init; } = 3600;
    public int NegativeCacheTtlSeconds { get; init; } = 30;
}
