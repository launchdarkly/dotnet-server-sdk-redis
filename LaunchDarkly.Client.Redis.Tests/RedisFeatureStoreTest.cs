using System;
using System.Collections.Generic;
using Xunit;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace LaunchDarkly.Client.Redis.Tests
{
    public class RedisFeatureStoreTest : IDisposable
    {
        public class TestData : IVersionedData
        {
            public string Key { get; set; }
            public int Version { get; set; }
            public string Value { get; set; }
            public bool Deleted { get; set; }
        }

        class TestDataKind : VersionedDataKind<TestData>
        {
            public override string GetNamespace()
            {
                return "test";
            }

            public override TestData MakeDeletedItem(string key, int version)
            {
                return new TestData { Key = key, Version = version, Deleted = true };
            }

            public override Type GetItemType()
            {
                return typeof(TestData);
            }

            public override string GetStreamApiPath()
            {
                throw new NotImplementedException();
            }
        }

        static readonly TestDataKind TestKind = new TestDataKind();
        const string Prefix = "test-prefix";

        RedisFeatureStore store;

        TestData item1 = new TestData { Key = "foo", Version = 10 };
        TestData item2 = new TestData { Key = "bar", Version = 10 };

        public RedisFeatureStoreTest()
        {
            store = (RedisFeatureStore)
                RedisFeatureStoreBuilder.Default().WithPrefix(Prefix).CreateFeatureStore();
        }

        public void Dispose()
        {
            store.Dispose();
        }

        protected void InitStore()
        {
            IDictionary<string, IVersionedData> items = new Dictionary<string, IVersionedData>();
            items[item1.Key] = item1;
            items[item2.Key] = item2;
            IDictionary<IVersionedDataKind, IDictionary<string, IVersionedData>> allData =
                new Dictionary<IVersionedDataKind, IDictionary<string, IVersionedData>>();
            allData[TestKind] = items;
            store.Init(allData);
        }

        [Fact]
        public void StoreInitializedAfterInit()
        {
            InitStore();
            Assert.True(store.Initialized());
        }

        [Fact]
        public void GetExistingItem()
        {
            InitStore();
            var result = store.Get(TestKind, item1.Key);
            Assert.Equal(item1.Value, result.Value);
        }

        [Fact]
        public void GetNonexistingItem()
        {
            InitStore();
            var result = store.Get(TestKind, "biz");
            Assert.Null(result);
        }

        [Fact]
        public void GetUsesCachedValueIfAvailable()
        {
            InitStore();
            var initialItem = store.Get(TestKind, item1.Key);
            Assert.Equal(item1.Value, initialItem.Value);
            
            using (ConnectionMultiplexer otherClient = ConnectionMultiplexer.Connect("localhost:6379"))
            {
                var modifiedItem = new TestData
                {
                    Key = item1.Key,
                    Version = item1.Version,
                    Value = "different"
                };
                otherClient.GetDatabase().HashSet(Prefix + ":" + TestKind.GetNamespace(),
                    item1.Key, JsonConvert.SerializeObject(modifiedItem));

                var cachedItem = store.Get(TestKind, item1.Key);
                Assert.Equal(initialItem.Value, cachedItem.Value);
            }
        }

        [Fact]
        public void GetAlwaysHitsRedisIfCacheIsDisabled()
        {
            using (var noCacheStore =
                RedisFeatureStoreBuilder.Default()
                    .WithPrefix(Prefix)
                    .WithCacheExpiration(TimeSpan.Zero)
                    .CreateFeatureStore())
            {
                noCacheStore.Upsert(TestKind, item1);
                var initialItem = noCacheStore.Get(TestKind, item1.Key);
                Assert.Equal(item1.Value, initialItem.Value);

                using (ConnectionMultiplexer otherClient = ConnectionMultiplexer.Connect("localhost:6379"))
                {
                    var modifiedItem = new TestData
                    {
                        Key = item1.Key,
                        Version = item1.Version,
                        Value = "different"
                    };
                    otherClient.GetDatabase().HashSet(Prefix + ":" + TestKind.GetNamespace(),
                        item1.Key, JsonConvert.SerializeObject(modifiedItem));

                    var uncachedItem = store.Get(TestKind, item1.Key);
                    Assert.Equal(modifiedItem.Value, uncachedItem.Value);
                }
            }
        }

        [Fact]
        public void GetAllItems()
        {
            InitStore();
            var result = store.All(TestKind);
            Assert.Equal(2, result.Count);
            Assert.Equal(item1.Key, result[item1.Key].Key);
            Assert.Equal(item2.Key, result[item2.Key].Key);
        }

        [Fact]
        public void UpsertWithNewerVersion()
        {
            InitStore();
            var newVer = new TestData { Key = item1.Key, Version = item1.Version + 1, Value = "new" };
            store.Upsert(TestKind, newVer);
            var result = store.Get(TestKind, item1.Key);
            Assert.Equal(newVer.Value, result.Value);
        }

        [Fact]
        public void UpsertWithSameVersion()
        {
            InitStore();
            var newVer = new TestData { Key = item1.Key, Version = item1.Version, Value = "new" };
            store.Upsert(TestKind, newVer);
            var result = store.Get(TestKind, item1.Key);
            Assert.Equal(item1.Value, result.Value);
        }

        [Fact]
        public void UpsertWithOlderVersion()
        {
            InitStore();
            var newVer = new TestData { Key = item1.Key, Version = item1.Version - 1, Value = "new" };
            store.Upsert(TestKind, newVer);
            var result = store.Get(TestKind, item1.Key);
            Assert.Equal(item1.Value, result.Value);
        }

        [Fact]
        public void UpsertNewItem()
        {
            InitStore();
            var newItem = new TestData { Key = "biz", Version = 99 };
            store.Upsert(TestKind, newItem);
            var result = store.Get(TestKind, newItem.Key);
            Assert.Equal(newItem.Key, result.Key);
        }

        [Fact]
        public void DeleteWithNewerVersion()
        {
            InitStore();
            store.Delete(TestKind, item1.Key, item1.Version + 1);
            Assert.Null(store.Get(TestKind, item1.Key));
        }

        [Fact]
        public void DeleteWithSameVersion()
        {
            InitStore();
            store.Delete(TestKind, item1.Key, item1.Version);
            Assert.NotNull(store.Get(TestKind, item1.Key));
        }

        [Fact]
        public void DeleteWithOlderVersion()
        {
            InitStore();
            store.Delete(TestKind, item1.Key, item1.Version - 1);
            Assert.NotNull(store.Get(TestKind, item1.Key));
        }

        [Fact]
        public void DeleteUnknownItem()
        {
            InitStore();
            store.Delete(TestKind, "biz", 11);
            Assert.Null(store.Get(TestKind, "biz"));
        }

        [Fact]
        public void UpsertOlderVersionAfterDelete()
        {
            InitStore();
            store.Delete(TestKind, item1.Key, item1.Version + 1);
            store.Upsert(TestKind, item1);
            Assert.Null(store.Get(TestKind, item1.Key));
        }

        [Fact]
        public void UpsertRaceConditionAgainstExternalClientWithLowerVersion()
        {
            using (ConnectionMultiplexer otherClient = ConnectionMultiplexer.Connect("localhost:6379"))
            {
                InitStore();
                int oldVersion = item1.Version;
                int slightlyNewerVersion = oldVersion + 1;
                int muchNewerVersion = oldVersion + 2;

                var competingItem = new TestData { Key = item1.Key, Version = slightlyNewerVersion };
                store.WillUpdate += MakeConcurrentModifier(otherClient, competingItem);

                var attemptToUpsertThisItem = new TestData { Key = item1.Key, Version = muchNewerVersion };
                store.Upsert(TestKind, attemptToUpsertThisItem);

                var result = store.Get(TestKind, item1.Key);
                Assert.Equal(muchNewerVersion, result.Version);
            }
        }

        [Fact]
        public void HandlesUpsertRaceConditionAgainstExternalClientWithHigherVersion()
        {
            using (ConnectionMultiplexer otherClient = ConnectionMultiplexer.Connect("localhost:6379"))
            {
                InitStore();
                int oldVersion = item1.Version;
                int slightlyNewerVersion = oldVersion + 1;
                int muchNewerVersion = oldVersion + 2;

                var competingItem = new TestData { Key = item1.Key, Version = muchNewerVersion };
                store.WillUpdate += MakeConcurrentModifier(otherClient, competingItem);

                var attemptToUpsertThisItem = new TestData { Key = item1.Key, Version = slightlyNewerVersion };
                store.Upsert(TestKind, attemptToUpsertThisItem);

                var result = store.Get(TestKind, item1.Key);
                Assert.Equal(muchNewerVersion, result.Version);
            }
        }

        private EventHandler MakeConcurrentModifier(
            ConnectionMultiplexer otherClient, TestData competingItem)
        {
            return (sender, args) =>
            {
                otherClient.GetDatabase().HashSet(Prefix + ":" + TestKind.GetNamespace(),
                    competingItem.Key, JsonConvert.SerializeObject(competingItem));
            };
        }
    }
}
