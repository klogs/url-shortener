using Shortener.Application.Interfaces;
using Shortener.Domain.Enums;

namespace Shortener.Application.Domains.Commands.VerifyDomain;

public sealed class VerifyDomainHandler(
    IDomainRepository domains,
    IDomainVerifier verifier,
    TimeProvider time)
{
    public async Task HandleAsync(VerifyDomainCommand cmd, CancellationToken ct = default)
    {
        var domain = await domains.GetByIdAsync(cmd.DomainId, cmd.TenantId, ct)
            ?? throw new InvalidOperationException("Domain not found.");

        if (domain.Status == DomainStatus.Active)
        {
            return; // already verified — idempotent
        }

        if (domain.Status == DomainStatus.Disabled)
        {
            throw new InvalidOperationException("Domain is disabled and cannot be verified.");
        }

        var token = domain.VerificationToken
            ?? throw new InvalidOperationException("Domain has no verification token.");

        var verified = await verifier.VerifyAsync(domain.Host, token, ct);
        if (!verified)
        {
            throw new InvalidOperationException(
                $"Verification failed. Ensure https://{domain.Host}/.well-known/shortener-verify returns the token.");
        }

        domain.MarkVerified(time.GetUtcNow());
        await domains.UpdateAsync(domain, ct);
    }
}
