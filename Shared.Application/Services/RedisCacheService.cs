using Shared.Application.Interfaces;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

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

        public async Task<T?> GetAsync<T>(string key)
        {
            var value = await _db.StringGetAsync(key);
            if (!value.HasValue) return default;
            return JsonSerializer.Deserialize<T>((string)value!);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan expiration)
        {
            var serialized = JsonSerializer.Serialize(value);
            await _db.StringSetAsync(key, serialized, expiration);
        }

        public async Task RemoveAsync(string key)
        {
            await _db.KeyDeleteAsync(key);
        }

        public async Task RemoveByPrefixAsync(string prefix)
        {
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var keys = server.Keys(pattern: $"{prefix}*").ToArray();
            if (keys.Any())
                await _db.KeyDeleteAsync(keys);
        }

        public async Task SortedSetAddAsync(string key, string member, double score)
            => await _db.SortedSetAddAsync(key, member, score);

        public async Task<List<(string Member, double Score)>> SortedSetGetTopAsync(string key, int count)
        {
            var results = await _db.SortedSetRangeByRankWithScoresAsync(key, 0, count - 1, Order.Descending);
            return results.Select(r => (r.Element.ToString(), r.Score)).ToList();
        }

        public async Task<long?> SortedSetGetRankAsync(string key, string member)
            => await _db.SortedSetRankAsync(key, member, Order.Descending);
        
        public async Task<double?> SortedSetGetScoreAsync(string key, string member)
            => await _db.SortedSetScoreAsync(key, member);

        public async Task SortedSetRemoveAsync(string key, string member)
            => await _db.SortedSetRemoveAsync(key, member);
    }
}
