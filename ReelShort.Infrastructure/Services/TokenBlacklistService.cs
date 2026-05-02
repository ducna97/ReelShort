using ReelShort.Application.Interfaces;
using StackExchange.Redis;

namespace ReelShort.Infrastructure.Services;

public class TokenBlacklistService : ITokenBlacklistService
{
    private readonly IDatabase _redis;
    private const string Prefix = "blacklist:";
    
    public TokenBlacklistService(IConnectionMultiplexer connectionMultiplexer)
    {
        _redis = connectionMultiplexer.GetDatabase();
    }

    public async Task BlacklistTokenAsync(string token, TimeSpan expiry)
    {
        await _redis.StringSetAsync($"{Prefix}{token}", "revoked", expiry);
    }

    public async Task<bool> IsBlacklistedAsync(string token)
    {
        return await _redis.KeyExistsAsync($"{Prefix}{token}");
    }
}