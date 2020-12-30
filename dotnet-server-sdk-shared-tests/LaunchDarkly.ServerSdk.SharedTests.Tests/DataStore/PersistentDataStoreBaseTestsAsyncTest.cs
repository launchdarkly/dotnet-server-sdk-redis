using System.Threading.Tasks;
using LaunchDarkly.Sdk.Server.Interfaces;
using Xunit.Abstractions;

namespace LaunchDarkly.Sdk.Server.SharedTests.DataStore
{
    // This runs PersistentDataStoreBaseTests against a mock store implementation that is known to
    // behave as expected, to verify that the test suite logic has the correct expectations.

    public class PersistentDataStoreBaseTestsAsyncTest : PersistentDataStoreBaseTests
    {
        private readonly MockDatabase _database = new MockDatabase();

        protected override PersistentDataStoreTestConfig Configuration =>
            new PersistentDataStoreTestConfig
            {
                StoreAsyncFactoryFunc = CreateStoreFactory,
                ClearDataAction = ClearAllData,
            };

        public PersistentDataStoreBaseTestsAsyncTest(ITestOutputHelper testOutput) : base(testOutput) { }

        private IPersistentDataStoreAsyncFactory CreateStoreFactory(string prefix) =>
            new MockAsyncStoreFactory { Database = _database, Prefix = prefix };

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async Task ClearAllData(string prefix) =>
            _database.Clear(prefix);
#pragma warning restore CS1998

        private class MockAsyncStoreFactory : IPersistentDataStoreAsyncFactory
        {
            internal MockDatabase Database { get; set; }
            internal string Prefix { get; set; }

            public IPersistentDataStoreAsync CreatePersistentDataStore(LdClientContext context) =>
                new MockAsyncStore(Database, Prefix);
        }
    }
}
