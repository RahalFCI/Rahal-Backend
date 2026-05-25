using Polly;
using Polly.Registry;
using Shared.Application.Interfaces;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Shared.Infrastructure
{
    internal class RedisCacheService : ICacheService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _db;
        private readonly ResiliencePipeline _resiliencePipeline;
        private readonly ILogger<RedisCacheService> _logger;

        public RedisCacheService(
            IConnectionMultiplexer redis,
            ResiliencePipelineProvider<string> pipelineProvider,
            ILogger<RedisCacheService> logger)
        {
            _redis = redis;
            _db = redis.GetDatabase();
            _resiliencePipeline = pipelineProvider.GetPipeline("redis");
            _logger = logger;
        }

        public async Task<long?> GetCounterAsync(string key)
        {
            return await _resiliencePipeline.ExecuteAsync<long?>(async (ct) =>
            {
                var value = await _db.StringGetAsync(key);
                if (!value.HasValue)
                    return null;
                return (long)value;
            });
        }

        public async Task<TimeSpan?> GetTimeToLiveAsync(string key)
        {
            return await _resiliencePipeline.ExecuteAsync<TimeSpan?>(async (ct) =>
            {
                return await _db.KeyTimeToLiveAsync(key);
            });
        }

        public async Task<long> IncrementAsync(string key, TimeSpan expiration)
        {
            return await _resiliencePipeline.ExecuteAsync<long>(async (ct) =>
            {
                var count = await _db.StringIncrementAsync(key);
                if (count == 1)
                {
                    await _db.KeyExpireAsync(key, expiration);
                }
                return count;
            });
        }

        public async Task<bool> KeyExistsAsync(string key)
        {
            return await _resiliencePipeline.ExecuteAsync<bool>(async (ct) =>
            {
                return await _db.KeyExistsAsync(key);
            });
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            return await _resiliencePipeline.ExecuteAsync<T?>(async (ct) =>
            {
                var value = await _db.StringGetAsync(key);
                if (!value.HasValue) return default;
                return JsonSerializer.Deserialize<T>((string)value!);
            });
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan expiration)
        {
            await _resiliencePipeline.ExecuteAsync(async (ct) =>
            {
                var serialized = JsonSerializer.Serialize(value);
                await _db.StringSetAsync(key, serialized, expiration);
            });
        }

        public async Task RemoveAsync(string key)
        {
            await _resiliencePipeline.ExecuteAsync(async (ct) =>
            {
                await _db.KeyDeleteAsync(key);
            });
        }

        public async Task RemoveByPrefixAsync(string prefix)
        {
            await _resiliencePipeline.ExecuteAsync(async (ct) =>
            {
                var server = _redis.GetServer(_redis.GetEndPoints().First());
                var keys = server.Keys(pattern: $"{prefix}*").ToArray();
                if (keys.Any())
                    await _db.KeyDeleteAsync(keys);
            });
        }

        public async Task SortedSetAddAsync(string key, string member, double score)
        {
            await _resiliencePipeline.ExecuteAsync(async (ct) =>
            {
                await _db.SortedSetAddAsync(key, member, score);
            });
        }

        public async Task<List<(string Member, double Score)>> SortedSetGetTopAsync(string key, int count)
        {
            return await _resiliencePipeline.ExecuteAsync<List<(string Member, double Score)>>(async (ct) =>
            {
                var results = await _db.SortedSetRangeByRankWithScoresAsync(key, 0, count - 1, Order.Descending);
                return results.Select(r => (r.Element.ToString(), r.Score)).ToList();
            });
        }

        public async Task<long?> SortedSetGetRankAsync(string key, string member)
        {
            return await _resiliencePipeline.ExecuteAsync<long?>(async (ct) =>
            {
                return await _db.SortedSetRankAsync(key, member, Order.Descending);
            });
        }

        public async Task<double?> SortedSetGetScoreAsync(string key, string member)
        {
            return await _resiliencePipeline.ExecuteAsync<double?>(async (ct) =>
            {
                return await _db.SortedSetScoreAsync(key, member);
            });
        }

        public async Task SortedSetRemoveAsync(string key, string member)
        {
            await _resiliencePipeline.ExecuteAsync(async (ct) =>
            {
                await _db.SortedSetRemoveAsync(key, member);
            });
        }
    }
}
