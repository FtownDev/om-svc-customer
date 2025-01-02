using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using StackExchange.Redis;
using System.Reflection;
using System.Text.Json;

namespace om_svc_customer.Services
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly IDatabase _database;
        private readonly string cachePrefix = "Customers_";

        public RedisCacheService(IDistributedCache cache, IConnectionMultiplexer connectionMultiplexer)
        {
            _cache = cache;
            _database = connectionMultiplexer.GetDatabase();    
            if(_database == null)
            {
                throw new ArgumentNullException("Unable to get Redis Database instance.");
            }
        }

        public T? GetData<T>(string key)
        {
            T? retval;

            var data = _cache.GetString(key);

            if (data == null)
            {
                retval = default;
            }
            else
            {
                retval = JsonSerializer.Deserialize<T>(data);
            }

            return retval;
        }

        public void SetData<T>(string key, T data, int timeoutLength = 1)
        {
            var options = new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(timeoutLength)
            };

            _cache.SetString(key, JsonSerializer.Serialize(data), options);
        }

        public async Task InvalidateKeys(List<string> keysToDelete)
        {
            foreach (var key in keysToDelete)
            {
                await InvalidateByPattern(key);
            }
        }

        private async Task InvalidateByPattern(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
                throw new ArgumentException("Pattern cannot be null or empty", nameof(pattern));

            string searchPattern = pattern.EndsWith("*") ? cachePrefix + pattern :  cachePrefix + pattern + "*";
            var server = _database.Multiplexer.GetServer(_database.Multiplexer.GetEndPoints().First());

            // Get and delete all matching keys
            var keys = server.Keys(pattern: searchPattern).ToArray();
            if (keys.Any())
            {
                await _database.KeyDeleteAsync(keys);
            }
        }
    }
}
