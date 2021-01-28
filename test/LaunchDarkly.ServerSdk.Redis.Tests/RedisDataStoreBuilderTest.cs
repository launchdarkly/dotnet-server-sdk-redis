using System;
using System.Collections.Generic;
using System.Net;
using Xunit;

namespace LaunchDarkly.Client.Integrations
{
    public class RedisDataStoreBuilderTest
    {
        [Fact]
        public void DefaultConfigHasDefaultRedisHostAndPort()
        {
            var builder = Redis.DataStore();
            Assert.Collection(builder._redisConfig.EndPoints,
                e => Assert.Equal(new DnsEndPoint("localhost", 6379), e));
        }

        [Fact]
        public void CanSetRedisEndPoint()
        {
            var builder = Redis.DataStore();
            var ep = new DnsEndPoint("test", 9999);
            builder.EndPoint(ep);
            Assert.Collection(builder._redisConfig.EndPoints,
                e => Assert.Equal(ep, e));
        }

        [Fact]
        public void CanSetMultipleRedisEndPoints()
        {
            var builder = Redis.DataStore();
            DnsEndPoint ep1 = new DnsEndPoint("test", 9998);
            DnsEndPoint ep2 = new DnsEndPoint("test", 9999);
            builder.EndPoints(new List<EndPoint> { ep1, ep2 });
            Assert.Collection(builder._redisConfig.EndPoints,
                e => Assert.Equal(ep1, e),
                e => Assert.Equal(ep2, e));
        }

        [Fact]
        public void CanSetRedisHostAndPort()
        {
            var builder = Redis.DataStore();
            builder.HostAndPort("test", 9999);
            Assert.Collection(builder._redisConfig.EndPoints,
                e => Assert.Equal(new DnsEndPoint("test", 9999), e));
        }

        [Fact]
        public void CanSetMinimalRedisUrl()
        {
            var builder = Redis.DataStore();
            builder.Uri(new Uri("redis://test:9999"));
            Assert.Collection(builder._redisConfig.EndPoints,
                e => Assert.Equal(new DnsEndPoint("test", 9999), e));
            Assert.Null(builder._redisConfig.Password);
            Assert.Null(builder._redisConfig.DefaultDatabase);
        }

        [Fact]
        public void CanSetRedisUrlWithPassword()
        {
            var builder = Redis.DataStore();
            builder.Uri(new Uri("redis://:secret@test:9999"));
            Assert.Collection(builder._redisConfig.EndPoints,
                e => Assert.Equal(new DnsEndPoint("test", 9999), e));
            Assert.Equal("secret", builder._redisConfig.Password);
            Assert.Null(builder._redisConfig.DefaultDatabase);
        }

        [Fact]
        public void CanSetRedisUrlWithDatabase()
        {
            var builder = Redis.DataStore();
            builder.Uri(new Uri("redis://@test:9999/8"));
            Assert.Collection(builder._redisConfig.EndPoints,
                e => Assert.Equal(new DnsEndPoint("test", 9999), e));
            Assert.Null(builder._redisConfig.Password);
            Assert.Equal(8, builder._redisConfig.DefaultDatabase);
        }

        [Fact]
        public void CanSetDatabase()
        {
            var builder = Redis.DataStore();
            builder.DatabaseIndex(8);
            Assert.Equal(8, builder._redisConfig.DefaultDatabase);
        }

        [Fact]
        public void CanSetConnectTimeout()
        {
            var builder = Redis.DataStore();
            builder.ConnectTimeout(TimeSpan.FromSeconds(8));
            Assert.Equal(8000, builder._redisConfig.ConnectTimeout);
        }

        [Fact]
        public void CanSetOperationTimeout()
        {
            var builder = Redis.DataStore();
            builder.OperationTimeout(TimeSpan.FromSeconds(8));
            Assert.Equal(8000, builder._redisConfig.SyncTimeout);
        }
    }
}
