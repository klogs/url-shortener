using Shortener.Application.Interfaces;
using Shortener.Application.Links.Queries.ResolveRedirect;
using Shortener.Domain.Entities;
using Shortener.Domain.Enums;
using Shortener.UnitTests.Fakes;

namespace Shortener.UnitTests.Links;

public sealed class ResolveRedirectHandlerTests
{
    private const string Host = "example.com";
    private const string Code = "abc1234";

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid DomainId = Guid.NewGuid();

    private static ResolveRedirectHandler CreateHandler(
        IShortLinkRepository links,
        IRedirectCache cache,
        ILinkVariantRepository? variants = null,
        TimeProvider? time = null)
        => new(links, variants ?? new FakeLinkVariantRepository(), cache, time ?? TimeProvider.System);

    private static ShortLink ActiveLink(DateTimeOffset? expiresAt = null)
        => ShortLink.CreateAnonymous(TenantId, DomainId, Code, "https://dest.example.com",
            expiresAt ?? DateTimeOffset.UtcNow.AddDays(7), DateTimeOffset.UtcNow);

    [Fact]
    public async Task HandleAsync_CacheMiss_ActiveLink_ReturnsRedirect()
    {
        var links = new FakeShortLinkRepository();
        var cache = new FakeRedirectCache();
        await links.InsertAsync(ActiveLink(), default);

        var handler = CreateHandler(links, cache);
        var result = await handler.HandleAsync(new ResolveRedirectQuery(Host, Code));

        Assert.Equal(RedirectOutcome.Redirect, result.Outcome);
        Assert.Equal("https://dest.example.com", result.DestinationUrl);
    }

    [Fact]
    public async Task HandleAsync_CacheMiss_ExpiredLink_ReturnsExpired()
    {
        var expiredAt = DateTimeOffset.UtcNow.AddMinutes(-1);
        var links = new FakeShortLinkRepository();
        var cache = new FakeRedirectCache();
        await links.InsertAsync(ActiveLink(expiredAt), default);

        var handler = CreateHandler(links, cache);
        var result = await handler.HandleAsync(new ResolveRedirectQuery(Host, Code));

        Assert.Equal(RedirectOutcome.Expired, result.Outcome);
    }

    [Fact]
    public async Task HandleAsync_CacheMiss_NoLink_ReturnsNotFound()
    {
        var links = new FakeShortLinkRepository();
        var cache = new FakeRedirectCache();

        var handler = CreateHandler(links, cache);
        var result = await handler.HandleAsync(new ResolveRedirectQuery(Host, Code));

        Assert.Equal(RedirectOutcome.NotFound, result.Outcome);
    }

    [Fact]
    public async Task HandleAsync_CacheHit_ActiveEntry_ReturnsRedirect()
    {
        var links = new FakeShortLinkRepository();
        var cache = new FakeRedirectCache();
        cache.Seed(Host, Code, new CachedRedirect(Guid.NewGuid(), "https://cached.example.com",
            "Active", null, 302));

        var handler = CreateHandler(links, cache);
        var result = await handler.HandleAsync(new ResolveRedirectQuery(Host, Code));

        Assert.Equal(RedirectOutcome.Redirect, result.Outcome);
        Assert.Equal("https://cached.example.com", result.DestinationUrl);
    }

    [Fact]
    public async Task HandleAsync_CacheHit_ExpiredEntry_ReturnsExpired()
    {
        var links = new FakeShortLinkRepository();
        var cache = new FakeRedirectCache();
        // Cache hit with expiry in the past
        cache.Seed(Host, Code, new CachedRedirect(Guid.NewGuid(), "https://cached.example.com",
            "Active", DateTimeOffset.UtcNow.AddMinutes(-5), 302));

        var handler = CreateHandler(links, cache);
        var result = await handler.HandleAsync(new ResolveRedirectQuery(Host, Code));

        Assert.Equal(RedirectOutcome.Expired, result.Outcome);
    }

    [Fact]
    public async Task HandleAsync_CacheHit_NegativeSentinel_ReturnsNotFound()
    {
        var links = new FakeShortLinkRepository();
        var cache = new FakeRedirectCache();
        cache.Seed(Host, Code, new CachedRedirect(Guid.Empty, string.Empty, "NotFound", null, 0));

        var handler = CreateHandler(links, cache);
        var result = await handler.HandleAsync(new ResolveRedirectQuery(Host, Code));

        Assert.Equal(RedirectOutcome.NotFound, result.Outcome);
    }

    [Fact]
    public async Task HandleAsync_CacheHit_DisabledStatus_ReturnsDisabled()
    {
        var links = new FakeShortLinkRepository();
        var cache = new FakeRedirectCache();
        cache.Seed(Host, Code, new CachedRedirect(Guid.NewGuid(), "https://dest.example.com",
            "Disabled", null, 302));

        var handler = CreateHandler(links, cache);
        var result = await handler.HandleAsync(new ResolveRedirectQuery(Host, Code));

        Assert.Equal(RedirectOutcome.Disabled, result.Outcome);
    }
}
