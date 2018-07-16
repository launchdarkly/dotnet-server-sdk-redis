using System;
using System.Collections.Generic;
using System.Text;

#if TARGET_NETSTANDARD
using Microsoft.Extensions.Caching.Memory;
#else
using System.Runtime.Caching;
#endif

namespace LaunchDarkly.Client.Redis
{
    /// <summary>
    /// Simple wrapper around whichever .NET or .NET Standard cache class is available.
    /// </summary>
    class InMemoryCache
    {
#if TARGET_NETSTANDARD
        private readonly MemoryCache _cache;
#else
        private readonly System.Runtime.Caching.ObjectCache _cache;
#endif
        private readonly TimeSpan? _expiration;

        public InMemoryCache(string name, TimeSpan? expiration)
        {
            _expiration = expiration;
#if TARGET_NETSTANDARD
            _cache = new MemoryCache(new MemoryCacheOptions());
#else
            _cache = new System.Runtime.Caching.MemoryCache(name);
#endif
        }

        public T GetOrAdd<T>(string key, Func<T> createFn)
        {
#if TARGET_NETSTANDARD
            return _cache.GetOrCreate(key, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = _expiration;
                return createFn.Invoke();
            });
#else
            T item = (T)_cache[key];
            if (item == null)
            {
                item = createFn.Invoke();
                _cache.Set(key, item, MakeExpirationPolicy());
            }
            return item;
#endif
        }

        public void Set<T>(string key, T value)
        {
#if TARGET_NETSTANDARD
            if (_expiration.HasValue)
            {
                _cache.Set(key, value, _expiration.Value);
            }
            else
            {
                _cache.Set(key, value);
            }
#else
            _cache.Set(key, value, MakeExpirationPolicy());
#endif
        }

#if TARGET_NETSTANDARD
#else
        private System.Runtime.Caching.CacheItemPolicy MakeExpirationPolicy()
        {
            if (_expiration.HasValue)
            {
                return new System.Runtime.Caching.CacheItemPolicy
                {
                    AbsoluteExpiration = DateTime.Now.Add(_expiration.Value)
                };
            }
            return new System.Runtime.Caching.CacheItemPolicy();
        }
#endif
    }
}
