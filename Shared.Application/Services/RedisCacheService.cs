using Shared.Application.Interfaces;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Application.Services
{
    internal class RedisCacheService : ICacheService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _db;

        public RedisCacheService(IConnectionMultiplexer redis)
        {
            _redis = redis;
            _db = redis.GetDatabase();
        }
        public async Task<long?> GetCounterAsync(string key)
        {
            var value = await _db.StringGetAsync(key);

            if (!value.HasValue)
                return null;

            return (long)value;
        }

        public async Task<TimeSpan?> GetTimeToLiveAsync(string key)
        {
            return await _db.KeyTimeToLiveAsync(key);
        }

        public async Task<long> IncrementAsync(string key, TimeSpan expiration)
        {
            var count = await _db.StringIncrementAsync(key);

            // Set expiration only on first increment (when count is 1)
            if (count == 1)
            {
                await _db.KeyExpireAsync(key, expiration);
            }

            return count;
        }

        public async Task<bool> KeyExistsAsync(string key)
        {
            return await _db.KeyExistsAsync(key);
        }
    }
}
