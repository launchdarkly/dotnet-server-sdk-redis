using System;
using LaunchDarkly.Client.SharedTests.FeatureStore;
using StackExchange.Redis;

namespace LaunchDarkly.Client.Redis.Tests
{
    public class RedisFeatureStoreTest : FeatureStoreBaseTests
    {
        override protected IFeatureStore CreateStoreImpl(FeatureStoreCacheConfig caching)
        {
            return RedisFeatureStoreBuilder.Default().WithCaching(caching).
                CreateFeatureStore();
        }

        override protected IFeatureStore CreateStoreImplWithPrefix(string prefix)
        {
            return RedisFeatureStoreBuilder.Default().WithPrefix(prefix)
                .WithCaching(FeatureStoreCacheConfig.Disabled).CreateFeatureStore();
        }

        protected override IFeatureStore CreateStoreImplWithUpdateHook(Action hook)
        {
            return base.CreateStoreImplWithUpdateHook(hook);
        }

        override protected void ClearAllData()
        {
            using (var cxn = ConnectionMultiplexer.Connect("localhost:6379,allowAdmin=true"))
            {
                cxn.GetServer("localhost:6379").FlushDatabase();
            }
        }
    }
}
