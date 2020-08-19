using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TimeAPI.API.Cache
{
    public class InMemoryCacheService : ICacheService, IDisposable
    {
        private readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

        public Task<string> GetCacheValueAsync(string key)
        {
            return Task.FromResult(_cache.Get<string>(key));
        }

        public Task SetCacheValueAsync(string key, string value)
        {
            _cache.Set(key, value);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            dispose(true);
            GC.SuppressFinalize(this);
        }

        private void dispose(bool disposing)
        {
            if (!disposing)
            {
                if (disposing)
                {
                    if (_cache != null)
                    {
                        _cache.Dispose();

                    }
                }
            }
        }
    }
}
