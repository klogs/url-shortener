using System.ComponentModel.DataAnnotations;

namespace Shortener.Application.Options;

public sealed class AuthOptions
{
    public const string SectionName = "Auth";

    [Required]
    public string Authority { get; init; } = string.Empty;

    [Required]
    public string ClientId { get; init; } = string.Empty;
}
