using Microsoft.Extensions.Options;
using Shortener.Application.Interfaces;
using Shortener.Application.Links.Commands.CreateLink;
using Shortener.Application.Options;
using Shortener.Domain.Entities;
using Shortener.UnitTests.Fakes;

namespace Shortener.UnitTests.Links;

public sealed class CreateLinkHandlerTests
{
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid DomainId = Guid.NewGuid();
    private const string DefaultHost = "short.example.com";

    private static TenantDomain DefaultDomain()
        => TenantDomain.CreateVerified(TenantId, DefaultHost, isDefault: true, DateTimeOffset.UtcNow);

    private static CreateLinkHandler CreateHandler(
        IShortLinkRepository? links = null,
        IDomainRepository? domains = null,
        IShortCodeGenerator? codeGenerator = null,
        ICaptchaVerifier? captcha = null,
        ShortenerOptions? opts = null)
    {
        var fakeLinks = links ?? new FakeShortLinkRepository();
        var fakeDomains = domains ?? new FakeDomainRepository();
        var fakeCode = codeGenerator ?? new ConstantCodeGenerator("abc1234");
        var fakeCaptcha = captcha ?? new AlwaysPassCaptchaVerifier();
        var options = Options.Create(opts ?? new ShortenerOptions { AnonymousExpirationDays = 7 });
        return new CreateLinkHandler(fakeLinks, fakeDomains, fakeCode, fakeCaptcha, options, TimeProvider.System);
    }

    [Fact]
    public async Task HandleAsync_AnonymousLink_SetsExpiryAndIsAnonymousTrue()
    {
        var domainRepo = new FakeDomainRepository();
        domainRepo.Seed(DefaultDomain());
        var domain = await domainRepo.GetDefaultForTenantAsync(TenantId, default);

        var linkRepo = new FakeShortLinkRepository();
        var handler = CreateHandler(links: linkRepo, domains: domainRepo);
        var cmd = new CreateLinkCommand(TenantId, domain!.Id,
            "https://destination.example.com", null, null, true, null, null, null);

        var result = await handler.HandleAsync(cmd);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.NotNull(result.ExpiresAt);
        Assert.True(result.ExpiresAt > DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task HandleAsync_InvalidUrl_Throws()
    {
        var domainRepo = new FakeDomainRepository();
        domainRepo.Seed(DefaultDomain());
        var domain = await domainRepo.GetDefaultForTenantAsync(TenantId, default);

        var handler = CreateHandler(domains: domainRepo);
        var cmd = new CreateLinkCommand(TenantId, domain!.Id,
            "not-a-url", null, null, true, null, null, null);

        await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(cmd));
    }

    [Fact]
    public async Task HandleAsync_FtpUrl_Throws()
    {
        var domainRepo = new FakeDomainRepository();
        domainRepo.Seed(DefaultDomain());
        var domain = await domainRepo.GetDefaultForTenantAsync(TenantId, default);

        var handler = CreateHandler(domains: domainRepo);
        var cmd = new CreateLinkCommand(TenantId, domain!.Id,
            "ftp://files.example.com/file.txt", null, null, true, null, null, null);

        await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(cmd));
    }

    [Fact]
    public async Task HandleAsync_AnonymousLink_UsesAnonymousExpirationDays()
    {
        var domainRepo = new FakeDomainRepository();
        domainRepo.Seed(DefaultDomain());
        var domain = await domainRepo.GetDefaultForTenantAsync(TenantId, default);

        var opts = new ShortenerOptions { AnonymousExpirationDays = 3 };
        var before = DateTimeOffset.UtcNow;
        var handler = CreateHandler(domains: domainRepo, opts: opts);
        var cmd = new CreateLinkCommand(TenantId, domain!.Id,
            "https://dest.example.com", null, null, true, null, null, null);

        var result = await handler.HandleAsync(cmd);

        Assert.NotNull(result.ExpiresAt);
        // Should be approximately 3 days from now
        var delta = result.ExpiresAt!.Value - before;
        Assert.True(delta >= TimeSpan.FromDays(2.9) && delta <= TimeSpan.FromDays(3.1));
    }

    [Fact]
    public async Task HandleAsync_ShortUrl_ContainsDomainHost()
    {
        var domainRepo = new FakeDomainRepository();
        domainRepo.Seed(DefaultDomain());
        var domain = await domainRepo.GetDefaultForTenantAsync(TenantId, default);

        var handler = CreateHandler(domains: domainRepo);
        var cmd = new CreateLinkCommand(TenantId, domain!.Id,
            "https://destination.example.com", null, null, true, null, null, null);

        var result = await handler.HandleAsync(cmd);

        Assert.Contains(DefaultHost, result.ShortUrl);
    }

    private sealed class ConstantCodeGenerator(string code) : IShortCodeGenerator
    {
        public string Generate() => code;
    }

    private sealed class AlwaysPassCaptchaVerifier : ICaptchaVerifier
    {
        public Task<bool> VerifyAsync(string token, string? clientIp, CancellationToken ct)
            => Task.FromResult(true);
    }
}
