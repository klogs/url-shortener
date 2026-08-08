using Shortener.Application.Interfaces;

namespace Shortener.Application.Domains.Commands.RemoveDomain;

public sealed class RemoveDomainHandler(IDomainRepository domains)
{
    public async Task HandleAsync(RemoveDomainCommand cmd, CancellationToken ct = default)
    {
        var domain = await domains.GetByIdAsync(cmd.DomainId, cmd.TenantId, ct)
            ?? throw new InvalidOperationException("Domain not found.");

        if (domain.IsDefault)
        {
            throw new InvalidOperationException("Cannot remove the default domain.");
        }

        domain.Disable();
        await domains.UpdateAsync(domain, ct);
    }
}
