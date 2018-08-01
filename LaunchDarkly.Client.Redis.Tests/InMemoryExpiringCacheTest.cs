using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Xunit;

namespace LaunchDarkly.Client.Redis.Tests
{
    public class InMemoryExpiringCacheTest
    {
        [Fact]
        public void GetExistingValue()
        {
            var cache = new InMemoryExpiringCache<string, string>(null);
            cache.Set("key", "value");
            Assert.Equal("value", cache.Get("key"));
        }

        [Fact]
        public void TryGetExistingValue()
        {
            var cache = new InMemoryExpiringCache<string, string>(null);
            cache.Set("key", "value");
            Assert.True(cache.TryGetValue("key", out var value));
            Assert.Equal("value", value);
        }

        [Fact]
        public void GetMissingValue()
        {
            var cache = new InMemoryExpiringCache<string, string>(null);
            Assert.Null(cache.Get("key"));
        }

        [Fact]
        public void TryGetMissingValue()
        {
            var cache = new InMemoryExpiringCache<string, string>(null);
            Assert.False(cache.TryGetValue("key", out var value));
        }

        [Fact]
        public void GetOrAddExistingValue()
        {
            var cache = new InMemoryExpiringCache<string, string>(null);
            cache.Set("key", "value1");
            Assert.Equal("value1", cache.GetOrAdd("key", () => "value2"));
        }

        [Fact]
        public void GetOrAddMissingValue()
        {
            var cache = new InMemoryExpiringCache<string, string>(null);
            Assert.Equal("value2", cache.GetOrAdd("key", () => "value2"));
            Assert.Equal("value2", cache.Get("key"));
        }

        [Fact]
        public void EntryCanExpire()
        {
            var cache = new InMemoryExpiringCache<string, string>(TimeSpan.FromMilliseconds(200),
                TimeSpan.FromMilliseconds(25));
            cache.Set("key", "value");
            Assert.Equal("value", cache.Get("key"));
            Thread.Sleep(TimeSpan.FromMilliseconds(250));
            Assert.Null(cache.Get("key"));
        }
    }
}
