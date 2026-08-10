using Shortener.Application.Interfaces;

namespace Shortener.Application.Domains.Commands.SetDefaultDomain;

public sealed class SetDefaultDomainHandler(IDomainRepository domains)
{
    public async Task HandleAsync(SetDefaultDomainCommand cmd, CancellationToken ct = default)
        => await domains.SetDefaultAsync(cmd.DomainId, cmd.TenantId, ct);
}
