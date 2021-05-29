using System.Collections.Generic;
using System.Threading.Tasks;
using LaunchDarkly.Sdk.Server.Interfaces;

using static LaunchDarkly.Sdk.Server.Interfaces.BigSegmentStoreTypes;

namespace LaunchDarkly.Sdk.Server.SharedTests.BigSegmentStore
{
    /// <summary>
    /// A function that takes a prefix string and returns a configured factory for
    /// your implementation of <c>IBigSegmentStore</c>.
    /// </summary>
    /// <remarks>
    /// If the prefix string is null or "", it should use the default prefix defined by the
    /// data store implementation. The factory must include any necessary configuration that
    /// may be appropriate for the test environment (for instance, pointing it to a database
    /// instance that has been set up for the tests).
    /// </remarks>
    /// <param name="prefix">the database prefix</param>
    /// <returns>a configured factory</returns>
    public delegate IBigSegmentStoreFactory StoreFactoryFunc(string prefix);

    /// <summary>
    /// An asynchronous function that removes all data from the underlying
    /// data store for the specified prefix string.
    /// </summary>
    /// <param name="prefix">the database prefix</param>
    /// <returns>an asynchronous task</returns>
    public delegate Task ClearDataAction(string prefix);

    /// <summary>
    /// An asynchronous function that 
    /// </summary>
    /// <param name="prefix">the database prefix</param>
    /// <param name="metadata"></param>
    /// <returns>an asynchronous task</returns>
    public delegate Task SetMetadataAction(string prefix, StoreMetadata metadata);

    /// <summary>
    /// An asynchronous function that 
    /// </summary>
    /// <param name="prefix">the database prefix</param>
    /// <param name="userHashKey"></param>
    /// <param name="includedSegmentRefs"></param>
    /// <param name="excludedSegmentRefs"></param>
    /// <returns>an asynchronous task</returns>
    public delegate Task SetSegmentsAction(string prefix, string userHashKey,
        IEnumerable<string> includedSegmentRefs, IEnumerable<string> excludedSegmentRefs);

    /// <summary>
    /// Configuration for <see cref="BigSegmentStoreBaseTests"/>.
    /// </summary>
    public sealed class BigSegmentStoreTestConfig
    {
        public StoreFactoryFunc StoreFactoryFunc { get; set; }

        public ClearDataAction ClearDataAction { get; set; }

        public SetMetadataAction SetMetadataAction { get; set; }

        public SetSegmentsAction SetSegmentsAction { get; set; }
    }
}
