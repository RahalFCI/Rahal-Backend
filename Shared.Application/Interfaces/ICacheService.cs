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
    }
}
