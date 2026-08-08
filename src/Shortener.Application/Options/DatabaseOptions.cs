using System.ComponentModel.DataAnnotations;

namespace Shortener.Application.Options;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    [Required]
    public string ConnectionString { get; init; } = string.Empty;
}
