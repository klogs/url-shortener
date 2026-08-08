using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Shortener.Application.Interfaces;
using Shortener.Application.Options;

namespace Shortener.Infrastructure.Caching;

internal sealed class RedisRedirectRateLimiter(
    IConnectionMultiplexer redis,
    IOptions<RateLimitOptions> options,
    ILogger<RedisRedirectRateLimiter> logger) : IRedirectRateLimiter
{
    private readonly IDatabase _db = redis.GetDatabase();
    private readonly int _limit = options.Value.RedirectPerMinutePerIp;

    // Atomic sliding-window via Lua — all-or-nothing with a single round-trip.
    // KEYS[1] = key; ARGV[1] = now (ms); ARGV[2] = window (ms); ARGV[3] = limit; ARGV[4] = unique member
    private const string LuaScript = """
        local key = KEYS[1]
        local now    = tonumber(ARGV[1])
        local window = tonumber(ARGV[2])
        local limit  = tonumber(ARGV[3])
        local member = ARGV[4]
        redis.call('ZREMRANGEBYSCORE', key, '-inf', now - window)
        local count = redis.call('ZCARD', key)
        if count >= limit then return 0 end
        redis.call('ZADD', key, now, member)
        redis.call('PEXPIRE', key, window + 1000)
        return 1
        """;

    public async Task<bool> IsAllowedAsync(string ip, CancellationToken ct)
    {
        try
        {
            var key = (RedisKey)$"rl:redirect:{ip}";
            var nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var member = $"{nowMs}_{RandomNumberGenerator.GetHexString(8)}";

            var result = await _db.ScriptEvaluateAsync(LuaScript,
                keys: [key],
                values:
                [
                    (RedisValue)nowMs.ToString(),
                    (RedisValue)"60000",
                    (RedisValue)_limit.ToString(),
                    (RedisValue)member
                ]);

            return (int)result == 1;
        }
        catch (RedisException ex)
        {
            // Redis failure → allow through; rate-limit data loss is acceptable over service outage
            logger.LogWarning(ex, "Redis rate-limit check failed for {Ip}; allowing request", ip);
            return true;
        }
    }
}
