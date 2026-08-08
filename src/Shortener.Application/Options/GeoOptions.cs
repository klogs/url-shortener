namespace Shortener.Application.Options;

public sealed class GeoOptions
{
    public const string SectionName = "Geo";

    public string DatabasePath { get; init; } = string.Empty;
}
