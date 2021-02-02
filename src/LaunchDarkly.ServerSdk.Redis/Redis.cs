using System;
using System.Net;
using LaunchDarkly.Sdk.Server.Interfaces;

namespace LaunchDarkly.Sdk.Server.Integrations
{
    /// <summary>
    /// Integration between the LaunchDarkly SDK and Redis.
    /// </summary>
    public static class Redis
    {
        /// <summary>
        /// The default location of the Redis server: <c>localhost:6379</c>
        /// </summary>
        public static readonly EndPoint DefaultRedisEndPoint = new DnsEndPoint("localhost", 6379);

        /// <summary>
        /// The default value for <see cref="RedisDataStoreBuilder.Prefix"/>.
        /// </summary>
        public static readonly string DefaultPrefix = "launchdarkly";

        /// <summary>
        /// The default value for <see cref="RedisDataStoreBuilder.ConnectTimeout(TimeSpan)"/>.
        /// </summary>
        public static readonly TimeSpan DefaultConnectTimeout = TimeSpan.FromSeconds(5);

        /// <summary>
        /// The default value for <see cref="RedisDataStoreBuilder.OperationTimeout(TimeSpan)"/>.
        /// </summary>
        public static readonly TimeSpan DefaultOperationTimeout = TimeSpan.FromSeconds(3);

        /// <summary>
        /// Returns a builder object for creating a Redis-backed data store.
        /// </summary>
        /// <remarks>
        /// This object can be modified with <see cref="RedisDataStoreBuilder"/> methods for any desired
        /// custom Redis options. Then, pass it to <see cref="Components.PersistentDataStore(IPersistentDataStoreFactory)"/>
        /// and set any desired caching options. Finally, pass the result to <see cref="ConfigurationBuilder.DataStore(IDataStoreFactory)"/>.
        /// </remarks>
        /// <example>
        /// <code>
        ///     var config = Configuration.Builder("sdk-key")
        ///         .DataStore(
        ///             Components.PersistentDataStore(
        ///                 Redis.DataStore().Url("redis://my-redis-host")
        ///             ).CacheSeconds(15)
        ///         )
        ///         .Build();
        /// </code>
        /// </example>
        /// <returns>a data store configuration object</returns>
        public static RedisDataStoreBuilder DataStore() => new RedisDataStoreBuilder();
    }
}
