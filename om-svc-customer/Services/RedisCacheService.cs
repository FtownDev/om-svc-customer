using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace om_svc_customer.Services
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;

        public RedisCacheService(IDistributedCache cache)
        {
            _cache = cache;
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

        public void InvalidateKeys(List<string> keysToDelete)
        {
            foreach (var key in keysToDelete)
            {
                _cache.Remove(key);
            }
        }
    }
}
