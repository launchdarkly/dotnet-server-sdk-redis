using System;
using System.Threading.Tasks;
using LaunchDarkly.Logging;
using LaunchDarkly.Sdk.Server.Interfaces;
using LaunchDarkly.Sdk.Server.SharedTests.DataStore;
using StackExchange.Redis;
using Xunit;
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

        [Fact]
        public void LogMessageAtStartup()
        {
            var logCapture = Logs.Capture();
            var logger = logCapture.Logger("BaseLoggerName"); // in real life, the SDK will provide its own base log name
            var context = new LdClientContext(new BasicConfiguration("", false, logger),
                LaunchDarkly.Sdk.Server.Configuration.Default(""));
            using (Redis.DataStore().Prefix("my-prefix").CreatePersistentDataStore(context))
            {
                Assert.Collection(logCapture.GetMessages(),
                    m =>
                    {
                        Assert.Equal(LogLevel.Info, m.Level);
                        Assert.Equal("BaseLoggerName.DataStore.Redis", m.LoggerName);
                        Assert.Equal("Using Redis data store at localhost:6379 with prefix \"my-prefix\"",
                            m.Text);
                    });
            }
        }
    }
}
