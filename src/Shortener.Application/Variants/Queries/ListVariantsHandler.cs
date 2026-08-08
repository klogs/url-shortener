using Shortener.Application.Interfaces;

namespace Shortener.Application.Variants.Queries;

public sealed class ListVariantsHandler(ILinkVariantRepository variants)
{
    public async Task<IReadOnlyList<VariantDto>> HandleAsync(ListVariantsQuery query, CancellationToken ct = default)
    {
        var items = await variants.ListByLinkAsync(query.LinkId, query.TenantId, ct);
        return items
            .Select(v => new VariantDto(v.Id, v.LinkId, v.Label, v.DestinationUrl, v.Weight, v.CreatedAtUtc))
            .ToList();
    }
}
