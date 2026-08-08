using Shortener.Application.Interfaces;

namespace Shortener.Application.Domains.Queries.ListDomains;

public sealed class ListDomainsHandler(IDomainRepository domains)
{
    public async Task<IReadOnlyList<DomainDto>> HandleAsync(ListDomainsQuery query, CancellationToken ct = default)
    {
        var list = await domains.ListByTenantAsync(query.TenantId, ct);
        return list.Select(d => new DomainDto(
            d.Id,
            d.Host,
            d.NormalizedHost,
            d.Status.ToString(),
            d.IsDefault,
            d.VerificationToken,
            d.VerifiedAt,
            d.CreatedAtUtc)).ToList();
    }
}
