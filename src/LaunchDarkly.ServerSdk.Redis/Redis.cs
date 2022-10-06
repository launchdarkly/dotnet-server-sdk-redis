using System;
using System.Net;

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
        /// <para>
        /// This can be used either for the main data store that holds feature flag data, or for the big
        /// segment store, or both. If you are using both, they do not have to have the same parameters. For
        /// instance, in this example the main data store uses a Redis host called "host1" and the big
        /// segment store uses a Redis host called "host2":
        /// </para>
        /// <code>
        ///     var config = Configuration.Builder("sdk-key")
        ///         .DataStore(
        ///             Components.PersistentDataStore(
        ///                 Redis.DataStore().Uri("redis://host1:6379")
        ///             )
        ///         )
        ///         .BigSegments(
        ///             Components.BigSegments(
        ///                 Redis.DataStore().Uri("redis://host2:6379")
        ///             )
        ///         )
        ///         .Build();
        /// </code>
        /// <para>
        /// Note that the builder is passed to one of two methods,
        /// <see cref="Components.PersistentDataStore(Subsystems.IComponentConfigurer{Subsystems.IPersistentDataStore})"/> or
        /// <see cref="Components.BigSegments(Subsystems.IComponentConfigurer{Subsystems.IBigSegmentStore})"/>, depending on the context in
        /// which it is being used. This is because each of those contexts has its own additional
        /// configuration options that are unrelated to the Redis options. For instance, the
        /// <see cref="Components.PersistentDataStore(Subsystems.IComponentConfigurer{Subsystems.IPersistentDataStore})"/> builder
        /// has options for caching:
        /// </para>
        /// <code>
        ///     var config = Configuration.Builder("sdk-key")
        ///         .DataStore(
        ///             Components.PersistentDataStore(
        ///                 Redis.DataStore().Uri("redis://my-redis-host")
        ///             ).CacheSeconds(15)
        ///         )
        ///         .Build();
        /// </code>
        /// </remarks>
        /// <returns>a data store configuration object</returns>
        public static RedisDataStoreBuilder DataStore() => new RedisDataStoreBuilder();
    }
}
