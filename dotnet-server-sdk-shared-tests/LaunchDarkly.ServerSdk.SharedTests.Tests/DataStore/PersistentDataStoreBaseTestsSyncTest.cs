﻿using System.Threading.Tasks;
using LaunchDarkly.Sdk.Server.Interfaces;
using Xunit.Abstractions;

namespace LaunchDarkly.Sdk.Server.SharedTests.DataStore
{
    // This runs PersistentDataStoreBaseTests against a mock store implementation that is known to
    // behave as expected, to verify that the test suite logic has the correct expectations.

    public class PersistentDataStoreBaseTestsSyncTest : PersistentDataStoreBaseTests<MockSyncStore>
    {
        private readonly MockDatabase _database = new MockDatabase();

        protected override PersistentDataStoreTestConfig Configuration =>
            new PersistentDataStoreTestConfig
            {
                StoreFactoryFunc = CreateStoreFactory,
                ClearDataAction = ClearAllData,
            };

        public PersistentDataStoreBaseTestsSyncTest(ITestOutputHelper testOutput) : base(testOutput) { }

        private IPersistentDataStoreFactory CreateStoreFactory(string prefix) =>
            new MockSyncStoreFactory { Database = _database, Prefix = prefix };

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async Task ClearAllData(string prefix) =>
            _database.Clear(prefix);
#pragma warning restore CS1998

        private class MockSyncStoreFactory : IPersistentDataStoreFactory
        {
            internal MockDatabase Database { get; set; }
            internal string Prefix { get; set; }

            public IPersistentDataStore CreatePersistentDataStore(LdClientContext context) =>
                new MockSyncStore(Database, Prefix);
        }
    }
}
