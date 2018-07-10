using System;
using System.Net;

namespace LaunchDarkly.Client.Redis
{
    /// <summary>
    /// Contains the factory method for building a Redis implementation of <see cref="IFeatureStore"/>,
    /// as well as default values for the store's properties.
    /// </summary>
    public sealed class RedisComponents
    {
        /// <summary>
        /// The default location of the Redis server: <c>localhost:6379</c>
        /// </summary>
        public static readonly EndPoint DefaultRedisEndPoint = new DnsEndPoint("localhost", 6379);

        /// <summary>
        /// The default value for <see cref="RedisFeatureStoreBuilder.WithPrefix"/>.
        /// </summary>
        public static readonly string DefaultPrefix = "launchdarkly";

        /// <summary>
        /// The default value for <see cref="RedisFeatureStoreBuilder.WithCacheExpiration"/>.
        /// </summary>
        public static readonly TimeSpan DefaultCacheExpiration = TimeSpan.FromSeconds(15);

        /// <summary>
        /// The default value for <see cref="RedisFeatureStoreBuilder.WithConnectTimeout(TimeSpan)"/>.
        /// </summary>
        public static readonly TimeSpan DefaultConnectTimeout = TimeSpan.FromSeconds(2);

        /// <summary>
        /// The default value for <see cref="RedisFeatureStoreBuilder.WithResponseTimeout(TimeSpan)"/>.
        /// </summary>
        public static readonly TimeSpan DefaultResponseTimeout = TimeSpan.FromSeconds(2);

        /// <summary>
        /// Creates a new <see cref="RedisFeatureStoreBuilder"/>.
        /// </summary>
        /// <returns></returns>
        public static RedisFeatureStoreBuilder RedisFeatureStore()
        {
            return new RedisFeatureStoreBuilder();
        }
    }
}
