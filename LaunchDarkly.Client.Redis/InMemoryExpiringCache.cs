using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LaunchDarkly.Client.Redis
{
    /// <summary>
    /// Simple implementation of an in-memory cache with an optional TTL. Expired entries are
    /// purged by a background task.
    /// </summary>
    internal sealed class InMemoryExpiringCache<K, V> : IDisposable
    {
        private static readonly TimeSpan DefaultPurgeInterval = TimeSpan.FromSeconds(30);

        private readonly TimeSpan? _expiration;
        private readonly TimeSpan _purgeInterval;
        private readonly IDictionary<K, CacheEntry<K, V>> _entries = new Dictionary<K, CacheEntry<K, V>>();
        private readonly LinkedList<K> _keysInCreationOrder = new LinkedList<K>();
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private volatile bool _disposed = false;

        public InMemoryExpiringCache(TimeSpan? expiration) : this(expiration, DefaultPurgeInterval) { }

        public InMemoryExpiringCache(TimeSpan? expiration, TimeSpan purgeInterval)
        {
            _expiration = expiration;
            _purgeInterval = purgeInterval;
            if (expiration.HasValue)
            {
                Task.Run(() => PurgeExpiredEntriesAsync());
            }
        }

        public bool TryGetValue(K key, out V valueOut)
        {
            _lock.EnterReadLock();
            try
            {
                if (_entries.TryGetValue(key, out var entry))
                {
                    if (!entry.IsExpired())
                    {
                        valueOut = entry.Value;
                        return true;
                    }
                }
                valueOut = default(V);
                return false;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public V Get(K key)
        {
            if (TryGetValue(key, out var value))
            {
                return value;
            }
            return default(V);
        }
        
        // Note that this implementation does not coalesce requests - if two threads call GetOrAdd with
        // the same key for a value that's not in the cache, createFn will be called twice. This is a
        // known limitation in all of our SDKs, except the Java one which uses Guava's LoadingCache.
        public V GetOrAdd(K key, Func<V> createFn)
        {
            if (TryGetValue(key, out var value))
            {
                return value;
            }
            V newValue = createFn.Invoke();
            Set(key, newValue);
            return newValue;
        }

        public void Set(K key, V value)
        {
            _lock.EnterWriteLock();
            try
            {
                if (_entries.TryGetValue(key, out var oldEntry))
                {
                    _keysInCreationOrder.Remove(oldEntry.Node);
                }
                DateTime? expTime = null;
                if (_expiration.HasValue)
                {
                    expTime = DateTime.Now.Add(_expiration.Value);
                }
                var node = new LinkedListNode<K>(key);
                var entry = new CacheEntry<K, V>(value, expTime, node);
                _entries[key] = entry;
                _keysInCreationOrder.AddLast(node);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            _disposed = true;
        }

        private void PurgeExpiredEntries()
        {
            _lock.EnterWriteLock();
            try
            {
                while (_keysInCreationOrder.Count > 0 &&
                       _entries[_keysInCreationOrder.First.Value].IsExpired())
                {
                    _entries.Remove(_keysInCreationOrder.First.Value);
                    _keysInCreationOrder.RemoveFirst();
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        private async Task PurgeExpiredEntriesAsync()
        {
            while (!_disposed)
            {
                await Task.Delay(_purgeInterval);
                PurgeExpiredEntries();
            }
        }
    }

    internal class CacheEntry<K, V>
    {
        public readonly V Value;
        public readonly DateTime? ExpirationTime;
        public LinkedListNode<K> Node;

        public CacheEntry(V value, DateTime? expirationTime, LinkedListNode<K> node)
        {
            Value = value;
            ExpirationTime = expirationTime;
            Node = node;
        }

        public bool IsExpired()
        {
            return ExpirationTime.HasValue && ExpirationTime.Value.CompareTo(DateTime.Now) <= 0;
        }
    }
}
