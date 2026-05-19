using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Application.Interfaces
{
    public interface ICacheService
    {
        Task<long> IncrementAsync(string key, TimeSpan expiration);
        Task<long?> GetCounterAsync(string key);
        Task<TimeSpan?> GetTimeToLiveAsync(string key);
        Task<bool> KeyExistsAsync(string key);
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan expiration);
        Task RemoveAsync(string key);
        Task RemoveByPrefixAsync(string prefix);
        Task SortedSetAddAsync(string key, string member, double score);
        Task<List<(string Member, double Score)>> SortedSetGetTopAsync(string key, int count);
        Task<long?> SortedSetGetRankAsync(string key, string member);
        Task<double?> SortedSetGetScoreAsync(string key, string member);
        Task SortedSetRemoveAsync(string key, string member);
    }
}
