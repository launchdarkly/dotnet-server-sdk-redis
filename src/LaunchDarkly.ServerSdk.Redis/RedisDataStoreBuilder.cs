using System;
using System.Collections.Generic;
using System.Net;
using LaunchDarkly.Sdk.Server.Interfaces;
using StackExchange.Redis;

namespace LaunchDarkly.Sdk.Server.Integrations
{
    /// <summary>
    /// A <a href="http://en.wikipedia.org/wiki/Builder_pattern">builder</a> for configuring the
    /// Redis-based persistent data store.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Obtain an instance of this class by calling <see cref="Redis.DataStore"/>. After calling its methods
    /// to specify any desired custom settings, wrap it in a <see cref="PersistentDataStoreBuilder"/>
    /// by calling <see cref="Components.PersistentDataStore(IPersistentDataStoreFactory)"/>, then pass
    /// the result into the SDK configuration with <see cref="ConfigurationBuilder.DataStore(IDataStoreFactory)"/>.
    /// You do not need to call <see cref="CreatePersistentDataStore(LdClientContext)"/> yourself to build
    /// the actual data store; that will be done by the SDK.
    /// </para>
    /// <para>
    /// Builder calls can be chained, for example:
    /// </para>
    /// <code>
    ///     var config = Configuration.Builder("sdk-key")
    ///         .DataStore(
    ///             Components.PersistentDataStore(
    ///                 Redis.DataStore()
    ///                     .Uri(new Uri("redis://my-redis-host"))
    ///                     .Database(1)
    ///                 )
    ///                 .CacheSeconds(15)
    ///             )
    ///         .Build();
    /// </code>
    /// </remarks>
    public sealed class RedisDataStoreBuilder : IPersistentDataStoreFactory
    {
        internal ConfigurationOptions _redisConfig = new ConfigurationOptions();
        internal string _prefix = Redis.DefaultPrefix;

        internal RedisDataStoreBuilder()
        {
            _redisConfig.EndPoints.Add(Redis.DefaultRedisEndPoint);
            _redisConfig.ConnectTimeout = (int)Redis.DefaultConnectTimeout.TotalMilliseconds;
            _redisConfig.ResponseTimeout = (int)Redis.DefaultResponseTimeout.TotalMilliseconds;
        }

        /// <summary>
        /// Specifies all Redis configuration options at once.
        /// </summary>
        /// <param name="config">a <see cref="ConfigurationOptions"/> instance</param>
        /// <returns>the builder</returns>
        public RedisDataStoreBuilder RedisConfiguration(ConfigurationOptions config)
        {
            _redisConfig = config.Clone();
            return this;
        }

        /// <summary>
        /// Specifies a single Redis server by hostname and port.
        /// </summary>
        /// <param name="host">hostname of the Redis server</param>
        /// <param name="port">port of the Redis server</param>
        /// <returns>the builder</returns>
        public RedisDataStoreBuilder HostAndPort(string host, int port) =>
            EndPoint(new DnsEndPoint(host, port));

        /// <summary>
        /// Specifies a single Redis server as an EndPoint.
        /// </summary>
        /// <param name="endPoint">location of the Redis server</param>
        /// <returns>the builder</returns>
        public RedisDataStoreBuilder EndPoint(EndPoint endPoint) =>
            EndPoints(new List<EndPoint> { endPoint });

        /// <summary>
        /// Shortcut for calling <see cref="Uri(System.Uri)"/> with a string.
        /// </summary>
        /// <param name="uri">the Redis server URI as a string</param>
        /// <returns>the builder</returns>
        /// <seealso cref="Uri(System.Uri)"/>
        public RedisDataStoreBuilder Uri(string uri) => Uri(new Uri(uri));

        /// <summary>
        /// Specifies a Redis server - and, optionally, other properties including
        /// credentials and database number - using a URI.
        /// </summary>
        /// <param name="uri">the Redis server URI</param>
        /// <returns>the builder</returns>
        /// <seealso cref="Uri(string)"/>
        public RedisDataStoreBuilder Uri(Uri uri)
        {
            if (uri.Scheme.ToLower() != "redis")
            {
                throw new ArgumentException("URI scheme must be 'redis'");
            }
            HostAndPort(uri.Host, uri.Port);
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
        /// <returns>the builder</returns>
        public RedisDataStoreBuilder EndPoints(IList<EndPoint> endPoints)
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
        /// <returns>the builder</returns>
        public RedisDataStoreBuilder DatabaseIndex(int database)
        {
            _redisConfig.DefaultDatabase = database;
            return this;
        }

        /// <summary>
        /// Specifies the maximum time to wait for a connection to the Redis server.
        /// </summary>
        /// <param name="timeout">the timeout interval</param>
        /// <returns>the builder</returns>
        public RedisDataStoreBuilder ConnectTimeout(TimeSpan timeout)
        {
            _redisConfig.ConnectTimeout = (int)timeout.TotalMilliseconds;
            return this;
        }

        /// <summary>
        /// Specifies the maximum time to wait for data on the Redis connection.
        /// </summary>
        /// <param name="timeout">the timeout interval</param>
        /// <returns>the builder</returns>
        /// <seealso cref="OperationTimeout(TimeSpan)"/>
        public RedisDataStoreBuilder ResponseTimeout(TimeSpan timeout)
        {
            _redisConfig.ResponseTimeout = (int)timeout.TotalMilliseconds;
            return this;
        }

        /// <summary>
        /// Specifies the maximum time to wait for each synchronous Redis operation to complete.
        /// If you are seeing timeout errors - which could result from either an overburdened
        /// Redis server, or an unusually large operation such as storing a very large feature
        /// flag - you may want to increase this setting.
        /// </summary>
        /// <param name="timeout">the timeout interval</param>
        /// <returns>the builder</returns>
        /// <seealso cref="ResponseTimeout(TimeSpan)"/>
        public RedisDataStoreBuilder OperationTimeout(TimeSpan timeout)
        {
            _redisConfig.SyncTimeout = (int)timeout.TotalMilliseconds;
            return this;
        }

        /// <summary>
        /// Specifies the namespace prefix for all keys stored in Redis.
        /// </summary>
        /// <param name="prefix">the namespace prefix, or null to use <see cref="Redis.DefaultPrefix"/></param>
        /// <returns>the builder</returns>
        public RedisDataStoreBuilder Prefix(string prefix)
        {
            _prefix = string.IsNullOrEmpty(prefix) ? Redis.DefaultPrefix : prefix;
            return this;
        }

        /// <summary>
        /// Allows you to modify any of the configuration options supported by StackExchange.Redis
        /// directly. The current configuration will be passed to your Action, which can modify it
        /// in any way.
        /// </summary>
        /// <example>
        /// <code>
        ///     Redis.DataStore()
        ///         .RedisConfigChanges((config) => {
        ///             config.Ssl = true;
        ///         })
        /// </code>
        /// </example>
        /// <param name="modifyConfig"></param>
        /// <returns></returns>
        public RedisDataStoreBuilder RedisConfigChanges(Action<ConfigurationOptions> modifyConfig)
        {
            modifyConfig.Invoke(_redisConfig);
            return this;
        }

        /// <inheritdoc/>
        public IPersistentDataStore CreatePersistentDataStore(LdClientContext context) =>
            new RedisDataStoreImpl(_redisConfig, _prefix, context.Basic.Logger.SubLogger("DataStore.Redis"));
    }
}
