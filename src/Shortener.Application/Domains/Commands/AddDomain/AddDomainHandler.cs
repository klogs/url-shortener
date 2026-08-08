using Shortener.Application.Interfaces;
using Shortener.Domain.Entities;

namespace Shortener.Application.Domains.Commands.AddDomain;

public sealed class AddDomainHandler(IDomainRepository domains, TimeProvider time)
{
    public async Task<AddDomainResult> HandleAsync(AddDomainCommand cmd, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmd.Host);

        var normalizedHost = TenantDomain.NormalizeHost(cmd.Host);

        var existing = await domains.GetByNormalizedHostAsync(normalizedHost, ct);
        if (existing is not null)
        {
            throw new InvalidOperationException($"Host '{normalizedHost}' is already registered.");
        }

        var isFirst = (await domains.ListByTenantAsync(cmd.TenantId, ct)).Count == 0;
        var domain = TenantDomain.Create(cmd.TenantId, cmd.Host, isDefault: isFirst, time.GetUtcNow());

        await domains.InsertAsync(domain, ct);

        return new AddDomainResult(
            domain.Id,
            domain.Host,
            domain.NormalizedHost,
            domain.Status.ToString(),
            domain.IsDefault,
            domain.VerificationToken!);
    }
}
