using Microsoft.Extensions.Options;
using Shortener.Application.Interfaces;
using Shortener.Application.Options;
using Shortener.Domain.Entities;
using Shortener.Domain.Enums;

namespace Shortener.Application.Links.Commands.CreateLink;

public sealed class CreateLinkHandler(
    IShortLinkRepository links,
    IDomainRepository domains,
    IShortCodeGenerator codeGenerator,
    ICaptchaVerifier captchaVerifier,
    IUrlBlocklist urlBlocklist,
    IOptions<ShortenerOptions> shortenerOptions,
    TimeProvider time)
{
    private const int MaxCollisionRetries = 3;

    public async Task<CreateLinkResult> HandleAsync(CreateLinkCommand cmd, CancellationToken ct = default)
    {
        if (cmd.IsAnonymous)
        {
            var verified = await captchaVerifier.VerifyAsync(cmd.CaptchaToken ?? string.Empty, cmd.ClientIp, ct);
            if (!verified)
            {
                throw new InvalidOperationException("CAPTCHA verification failed.");
            }
        }

        ValidateDestinationUrl(cmd.DestinationUrl);

        if (urlBlocklist.IsBlocked(cmd.DestinationUrl))
        {
            throw new InvalidOperationException("This destination URL is not permitted.");
        }

        var domain = await domains.GetByIdAsync(cmd.DomainId, cmd.TenantId, ct)
            ?? throw new InvalidOperationException($"Domain {cmd.DomainId} not found for tenant.");

        var shortCode = await ResolveShortCodeAsync(cmd, ct);
        var now = time.GetUtcNow();

        ShortLink link;
        if (cmd.IsAnonymous)
        {
            var expiresAt = now.AddDays(shortenerOptions.Value.AnonymousExpirationDays);
            link = ShortLink.CreateAnonymous(cmd.TenantId, cmd.DomainId, shortCode, cmd.DestinationUrl, expiresAt, now);
        }
        else
        {
            link = ShortLink.CreateAuthenticated(
                cmd.TenantId, cmd.DomainId, shortCode, cmd.DestinationUrl,
                cmd.CreatedBy!.Value, cmd.ExpiresAt,
                RedirectType.Found, now);
        }

        await links.InsertAsync(link, ct);

        var shortUrl = $"https://{domain.Host}/{shortCode}";
        return new CreateLinkResult(link.Id, link.ShortCode, shortUrl, link.DestinationUrl, link.Status.ToString(), link.ExpiresAt);
    }

    private async Task<string> ResolveShortCodeAsync(CreateLinkCommand cmd, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(cmd.Alias))
        {
            var alias = cmd.Alias.Trim();
            if (await links.AliasExistsAsync(cmd.DomainId, alias, ct))
            {
                throw new InvalidOperationException("Alias already taken.");
            }

            return alias;
        }

        for (var attempt = 0; attempt < MaxCollisionRetries; attempt++)
        {
            var code = codeGenerator.Generate();
            if (!await links.AliasExistsAsync(cmd.DomainId, code, ct))
            {
                return code;
            }
        }

        throw new InvalidOperationException("Failed to generate a unique short code. Please retry.");
    }

    private static void ValidateDestinationUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException("Destination URL must be a valid http or https URL.");
        }
    }
}
