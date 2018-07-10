using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using Common.Logging;
using LazyCache;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace LaunchDarkly.Client.Redis
{
    internal sealed class RedisFeatureStore : IFeatureStore
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RedisFeatureStore));
        private static readonly string InitKey = "$initialized$";

        private readonly ConnectionMultiplexer _redis;
        private readonly IAppCache _cache;
        private readonly TimeSpan _cacheExpiration;
        private readonly IAppCache _initCache;
        private readonly string _prefix;

        // This event handler is used for unit testing only
        public class WillUpdateEventArgs : EventArgs
        {
            public string BaseKey { get; set; }
            public string ItemKey { get; set; }
        }
        public event EventHandler<WillUpdateEventArgs> WillUpdate;

        internal RedisFeatureStore(ConfigurationOptions redisConfig, string prefix, TimeSpan cacheExpiration)
        {
            Log.Info("Creating Redis feature store using Redis server(s) at [" +
                String.Join(", ", redisConfig.EndPoints) + "]");
            _redis = ConnectionMultiplexer.Connect(redisConfig);

            _prefix = prefix;

            _cacheExpiration = cacheExpiration;
            if (_cacheExpiration.TotalMilliseconds > 0)
            {
                _cache = new CachingService(new MemoryCache(typeof(RedisFeatureStore).AssemblyQualifiedName));
            }
            else
            {
                _cache = null;
            }

            _initCache = new CachingService(new MemoryCache(typeof(RedisFeatureStore).AssemblyQualifiedName + "-init"));
        }
        
        /// <summary>
        /// <see cref="IFeatureStore.Initialized"/>
        /// </summary>
        public bool Initialized()
        {
            // The cache takes care of both coalescing multiple simultaneous requests and memoizing
            // the result, so we'll only ever query Redis once for this (if at all - the Redis query will
            // be skipped if the cache was explicitly set by init()).
            return _initCache.GetOrAdd<bool>(InitKey, GetInitedState);
        }

        /// <summary>
        /// <see cref="IFeatureStore.Init"/>
        /// </summary>
        public void Init(IDictionary<IVersionedDataKind, IDictionary<string, IVersionedData>> items)
        {
            IDatabase db = _redis.GetDatabase();
            ITransaction txn = db.CreateTransaction();
            foreach (KeyValuePair<IVersionedDataKind, IDictionary<string, IVersionedData>> collection in items)
            {
                string key = ItemsKey(collection.Key);
                txn.KeyDeleteAsync(key);
                foreach (KeyValuePair<string, IVersionedData> item in collection.Value)
                {
                    txn.HashSetAsync(key, item.Key, JsonConvert.SerializeObject(item.Value));
                }
            }
            txn.StringSetAsync(_prefix, "");
            txn.Execute();
            _initCache.Add(InitKey, true);
            if (_cache != null)
            {
                foreach (KeyValuePair<IVersionedDataKind, IDictionary<string, IVersionedData>> collection in items)
                {
                    foreach (KeyValuePair<string, IVersionedData> item in collection.Value)
                    {
                        _cache.Add(CacheKey(collection.Key, item.Key), item.Value, _cacheExpiration);
                    }
                }
            }
        }

        /// <summary>
        /// <see cref="IFeatureStore.Get"/>
        /// </summary>
        public T Get<T>(VersionedDataKind<T> kind, String key) where T : class, IVersionedData
        {
            T item;
            if (_cache != null)
            {
                item = _cache.GetOrAdd(CacheKey(kind, key), () =>
                    {
                        T result;
                        TryGetFromRedis(_redis.GetDatabase(), kind, key, out result);
                        return result;
                    }, _cacheExpiration);
            }
            else
            {
                TryGetFromRedis(_redis.GetDatabase(), kind, key, out item);
            }
            if (item != null && item.Deleted)
            {
                return null;
            }
            return item;
        }

        /// <summary>
        /// <see cref="IFeatureStore.All"/>
        /// </summary>
        public IDictionary<string, T> All<T>(VersionedDataKind<T> kind) where T : class, IVersionedData
        {
            IDatabase db = _redis.GetDatabase();
            HashEntry[] allEntries = db.HashGetAll(ItemsKey(kind));
            Dictionary<string, T> result = new Dictionary<string, T>();
            foreach (HashEntry entry in allEntries)
            {
                T item = JsonConvert.DeserializeObject<T>(entry.Value);
                if (!item.Deleted)
                {
                    result[item.Key] = item;
                }
            }
            return result;
        }

        /// <summary>
        /// <see cref="IFeatureStore.Upsert"/>
        /// </summary>
        public void Upsert<T>(VersionedDataKind<T> kind, T item) where T : IVersionedData
        {
            UpdateItemWithVersioning(kind, item);
        }

        /// <summary>
        /// <see cref="IFeatureStore.Delete"/>
        /// </summary>
        public void Delete<T>(VersionedDataKind<T> kind, string key, int version) where T : IVersionedData
        {
            T deletedItem = kind.MakeDeletedItem(key, version);
            UpdateItemWithVersioning(kind, deletedItem);
        }
        
        private void UpdateItemWithVersioning<T>(VersionedDataKind<T> kind, T newItem) where T : IVersionedData
        {
            IDatabase db = _redis.GetDatabase();
            string baseKey = ItemsKey(kind);
            while (true)
            {
                string oldJson = db.HashGet(baseKey, newItem.Key);
                T oldItem = (oldJson == null) ? default(T) : JsonConvert.DeserializeObject<T>(oldJson);
                int oldVersion = (oldJson == null) ? -1 : oldItem.Version;
                if (oldVersion >= newItem.Version)
                {
                    Log.DebugFormat("Attempted to {} key: {} version: {} with a version that is" +
                        " the same or older: {} in \"{}\"",
                        newItem.Deleted ? "delete" : "update",
                        newItem.Key, oldVersion, newItem.Version, kind.GetNamespace());
                    if (_cache != null)
                    {
                        _cache.Add(CacheKey(kind, newItem.Key), oldItem, _cacheExpiration);
                    }
                    return;
                }

                // this hook is used only in unit tests
                WillUpdate?.Invoke(null, new WillUpdateEventArgs { BaseKey = baseKey, ItemKey = newItem.Key });

                ITransaction txn = db.CreateTransaction();
                txn.AddCondition(oldJson == null ? Condition.HashNotExists(baseKey, newItem.Key) :
                    Condition.HashEqual(baseKey, newItem.Key, oldJson));

                txn.HashSetAsync(baseKey, newItem.Key, JsonConvert.SerializeObject(newItem));

                bool success = txn.Execute();
                if (!success)
                {
                    // the watch was triggered, we should retry
                    Log.Debug("Concurrent modification detected, retrying");
                    continue;
                }
                
                if (_cache != null)
                {
                    _cache.Add(CacheKey(kind, newItem.Key), newItem, _cacheExpiration);
                }
                return;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _redis.Dispose();
            }
        }

        private bool TryGetFromRedis<T>(IDatabase db, VersionedDataKind<T> kind, string key, out T result) where T : IVersionedData
        {
            string json = db.HashGet(ItemsKey(kind), key);
            if (json == null)
            {
                Log.DebugFormat("[get] Key: {0} not found in \"{1}\"", key, kind.GetNamespace());
                result = default(T);
                return false;
            }
            result = JsonConvert.DeserializeObject<T>(json);
            return true;
        }

        private string ItemsKey(IVersionedDataKind kind)
        {
            return _prefix + ":" + kind.GetNamespace();
        }

        private string CacheKey(IVersionedDataKind kind, string key)
        {
            return kind.GetNamespace() + ":" + key;
        }

        private bool GetInitedState()
        {
            IDatabase db = _redis.GetDatabase();
            return db.KeyExists(_prefix);
        }
    }
}
