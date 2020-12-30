using System;
using System.Threading.Tasks;
using LaunchDarkly.Sdk.Server.Interfaces;
using LaunchDarkly.Sdk.Server.SharedTests.DataStore;
using StackExchange.Redis;
using Xunit.Abstractions;

namespace LaunchDarkly.Sdk.Server.Integrations
{
    public class RedisDataStoreTest : PersistentDataStoreBaseTests
    {
        protected override PersistentDataStoreTestConfig Configuration =>
            new PersistentDataStoreTestConfig
            {
                StoreFactoryFunc = MakeStoreFactory,
                ClearDataAction = ClearAllData,
                SetConcurrentModificationHookAction = SetUpdateHook
            };

        public RedisDataStoreTest(ITestOutputHelper testOutput) : base(testOutput) { }

        private IPersistentDataStoreFactory MakeStoreFactory(string prefix)
        {
            return Redis.DataStore().Prefix(prefix);
        }

        private Task ClearAllData(string prefix)
        {
            using (var cxn = ConnectionMultiplexer.Connect("localhost:6379,allowAdmin=true"))
            {
                cxn.GetServer("localhost:6379").FlushDatabase();
            }
            return Task.CompletedTask;
        }

        private void SetUpdateHook(object store, Action hook)
        {
            (store as RedisDataStoreImpl)._updateHook = hook;
        }
    }
}
