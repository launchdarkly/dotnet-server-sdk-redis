using System;
using System.Collections.Generic;
using System.Net;
using StackExchange.Redis;

namespace LaunchDarkly.Client.Redis
{
    /// <summary>
    /// Builder for a Redis-based implementation of <see cref="IFeatureStore"/>.
    /// Create an instance of the builder by calling <see cref="RedisComponents.RedisFeatureStore"/>;
    /// configure it using the setter methods; then pass the builder to
    /// <see cref="ConfigurationExtensions.WithFeatureStore(Configuration, IFeatureStore)"/>.
    /// </summary>
    public sealed class RedisFeatureStoreBuilder : IFeatureStoreFactory
    {
        private ConfigurationOptions _redisConfig = new ConfigurationOptions();
        private string _prefix = RedisComponents.DefaultPrefix;
        private TimeSpan _cacheExpiration = RedisComponents.DefaultCacheExpiration;

        internal RedisFeatureStoreBuilder()
        {
            _redisConfig.EndPoints.Add(RedisComponents.DefaultRedisEndPoint);
            _redisConfig.ConnectTimeout = (int)RedisComponents.DefaultConnectTimeout.TotalMilliseconds;
            _redisConfig.ResponseTimeout = (int)RedisComponents.DefaultResponseTimeout.TotalMilliseconds;
        }

        /// <summary>
        /// Creates a new <see cref="RedisFeatureStoreBuilder"/> with default properties.
        /// </summary>
        /// <returns>a builder</returns>
        public static RedisFeatureStoreBuilder Default()
        {
            return new RedisFeatureStoreBuilder();
        }

        /// <summary>
        /// Creates a feature store instance based on the currently configured builder.
        /// </summary>
        /// <returns>the feature store</returns>
        public IFeatureStore CreateFeatureStore()
        {
            return new RedisFeatureStore(_redisConfig.Clone(), _prefix, _cacheExpiration);
        }

        // Used for testing only
        internal ConfigurationOptions RedisConfig => _redisConfig;

        /// <summary>
        /// Specifies all Redis configuration options at once.
        /// </summary>
        /// <param name="config">a <see cref="ConfigurationOptions"/> instance</param>
        /// <returns>the same builder instance</returns>
        public RedisFeatureStoreBuilder WithRedisConfiguration(ConfigurationOptions config)
        {
            _redisConfig = config.Clone();
            return this;
        }

        /// <summary>
        /// Specifies a single Redis server by hostname and port.
        /// </summary>
        /// <param name="host">hostname of the Redis server</param>
        /// <param name="port">port of the Redis server</param>
        /// <returns>the same builder instance</returns>
        public RedisFeatureStoreBuilder WithRedisHostAndPort(string host, int port)
        {
            return WithRedisEndPoint(new DnsEndPoint(host, port));
        }

        /// <summary>
        /// Specifies a single Redis server as an EndPoint.
        /// </summary>
        /// <param name="endPoint">location of the Redis server</param>
        /// <returns>the same builder instance</returns>
        public RedisFeatureStoreBuilder WithRedisEndPoint(EndPoint endPoint)
        {
            return WithRedisEndPoints(new List<EndPoint> { endPoint });
        }

        /// <summary>
        /// Specifies a Redis server - and, optionally, other properties including
        /// credentials and database number - using a URI.
        /// </summary>
        /// <param name="uri">the Redis server URI</param>
        /// <returns>the same builder instance</returns>
        public RedisFeatureStoreBuilder WithRedisUri(Uri uri)
        {
            if (uri.Scheme.ToLower() != "redis")
            {
                throw new ArgumentException("URI scheme must be 'redis'");
            }
            WithRedisHostAndPort(uri.Host, uri.Port);
            if (!string.IsNullOrEmpty(uri.UserInfo))
            {
                var parts = uri.UserInfo.Split(':');
                if (parts.Length == 2)
                {
                    // Redis doesn't use the username
                    _redisConfig.Password = parts[1];
                }
                else
                {
                    throw new ArgumentException("Credentials must be in the format ':password'");
                }
            }
            if (!string.IsNullOrEmpty(uri.AbsolutePath) && uri.AbsolutePath != "/")
            {
                var path = uri.AbsolutePath;
                if (path.StartsWith("/"))
                {
                    path = path.Substring(1);
                }
                var dbIndex = Int32.Parse(path);
                _redisConfig.DefaultDatabase = dbIndex;
            }
            return this;
        }

        /// <summary>
        /// Specifies multiple Redis servers as a list of EndPoints.
        /// </summary>
        /// <param name="endPoints">locations of the Redis servers</param>
        /// <returns>the same builder instance</returns>
        public RedisFeatureStoreBuilder WithRedisEndPoints(IList<EndPoint> endPoints)
        {
            _redisConfig.EndPoints.Clear();
            foreach (var ep in endPoints)
            {
                _redisConfig.EndPoints.Add(ep);
            }
            return this;
        }

        /// <summary>
        /// Specifies which database to use within the Redis server. The default is 0.
        /// </summary>
        /// <param name="database">index of the database to use</param>
        /// <returns>the same builder instance</returns>
        public RedisFeatureStoreBuilder WithDatabaseIndex(int database)
        {
            _redisConfig.DefaultDatabase = database;
            return this;
        }

        /// <summary>
        /// Specifies the maximum time to wait for a connection to the Redis server.
        /// </summary>
        /// <param name="timeout">the timeout interval</param>
        /// <returns>the same builder instance</returns>
        public RedisFeatureStoreBuilder WithConnectTimeout(TimeSpan timeout)
        {
            _redisConfig.ConnectTimeout = (int)timeout.TotalMilliseconds;
            return this;
        }

        /// <summary>
        /// Specifies the maximum time to wait for a response from the Redis server.
        /// </summary>
        /// <param name="timeout">the timeout interval</param>
        /// <returns>the same builder instance</returns>
        public RedisFeatureStoreBuilder WithResponseTimeout(TimeSpan timeout)
        {
            _redisConfig.ResponseTimeout = (int)timeout.TotalMilliseconds;
            return this;
        }

        /// <summary>
        /// Specifies the namespace prefix for all keys stored in Redis.
        /// </summary>
        /// <param name="prefix">the namespace prefix</param>
        /// <returns>the same builder instance</returns>
        public RedisFeatureStoreBuilder WithPrefix(string prefix)
        {
            this._prefix = prefix ?? RedisComponents.DefaultPrefix;
            return this;
        }

        /// <summary>
        /// Specifies the amount of time to cache values in a local memory cache before having to
        /// retrieve them again from Redis. If this is zero, no such cache will be used.
        /// </summary>
        /// <param name="cacheExpiration">the length of time to cache locally</param>
        /// <returns>the same builder instance</returns>
        public RedisFeatureStoreBuilder WithCacheExpiration(TimeSpan cacheExpiration)
        {
            this._cacheExpiration = cacheExpiration;
            return this;
        }
    }
}
