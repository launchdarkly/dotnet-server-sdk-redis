using System;
using System.Collections.Generic;
using System.Net;
using Xunit;

namespace LaunchDarkly.Client.Redis.Tests
{
    public class RedisFeatureStoreBuilderTest
    {
        [Fact]
        public void DefaultConfigHasDefaultRedisHostAndPort()
        {
            RedisFeatureStoreBuilder builder = new RedisFeatureStoreBuilder();
            Assert.Equal(1, builder.RedisConfig.EndPoints.Count);
            Assert.Equal(new DnsEndPoint("localhost", 6379), builder.RedisConfig.EndPoints[0]);
        }

        [Fact]
        public void CanSetRedisEndPoint()
        {
            RedisFeatureStoreBuilder builder = new RedisFeatureStoreBuilder();
            DnsEndPoint ep = new DnsEndPoint("test", 9999);
            builder.WithRedisEndPoint(ep);
            Assert.Equal(1, builder.RedisConfig.EndPoints.Count);
            Assert.Equal(ep, builder.RedisConfig.EndPoints[0]);
        }

        [Fact]
        public void CanSetMultipleRedisEndPoints()
        {
            RedisFeatureStoreBuilder builder = new RedisFeatureStoreBuilder();
            DnsEndPoint ep1 = new DnsEndPoint("test", 9998);
            DnsEndPoint ep2 = new DnsEndPoint("test", 9999);
            builder.WithRedisEndPoints(new List<EndPoint> { ep1, ep2 });
            Assert.Equal(2, builder.RedisConfig.EndPoints.Count);
            Assert.Equal(ep1, builder.RedisConfig.EndPoints[0]);
            Assert.Equal(ep2, builder.RedisConfig.EndPoints[1]);
        }

        [Fact]
        public void CanSetRedisHostAndPort()
        {
            RedisFeatureStoreBuilder builder = new RedisFeatureStoreBuilder();
            builder.WithRedisHostAndPort("test", 9999);
            Assert.Equal(1, builder.RedisConfig.EndPoints.Count);
            Assert.Equal(new DnsEndPoint("test", 9999), builder.RedisConfig.EndPoints[0]);
        }

        [Fact]
        public void CanSetMinimalRedisUrl()
        {
            RedisFeatureStoreBuilder builder = new RedisFeatureStoreBuilder();
            builder.WithRedisUri(new Uri("redis://test:9999"));
            Assert.Equal(1, builder.RedisConfig.EndPoints.Count);
            Assert.Equal(new DnsEndPoint("test", 9999), builder.RedisConfig.EndPoints[0]);
            Assert.False(builder.RedisConfig.Ssl);
            Assert.Null(builder.RedisConfig.Password);
            Assert.Null(builder.RedisConfig.DefaultDatabase);
        }

        [Fact]
        public void CanSetRedisUrlWithPassword()
        {
            RedisFeatureStoreBuilder builder = new RedisFeatureStoreBuilder();
            builder.WithRedisUri(new Uri("redis://:secret@test:9999"));
            Assert.Equal(1, builder.RedisConfig.EndPoints.Count);
            Assert.Equal(new DnsEndPoint("test", 9999), builder.RedisConfig.EndPoints[0]);
            Assert.Equal("secret", builder.RedisConfig.Password);
            Assert.Null(builder.RedisConfig.DefaultDatabase);
        }

        [Fact]
        public void CanSetRedisUrlWithDatabase()
        {
            RedisFeatureStoreBuilder builder = new RedisFeatureStoreBuilder();
            builder.WithRedisUri(new Uri("redis://@test:9999/8"));
            Assert.Equal(1, builder.RedisConfig.EndPoints.Count);
            Assert.Equal(new DnsEndPoint("test", 9999), builder.RedisConfig.EndPoints[0]);
            Assert.Null(builder.RedisConfig.Password);
            Assert.Equal(8, builder.RedisConfig.DefaultDatabase);
        }

        [Fact]
        public void CanSetSecureRedisUrl()
        {
            RedisFeatureStoreBuilder builder = new RedisFeatureStoreBuilder();
            builder.WithRedisUri(new Uri("rediss://@test:9999"));
            Assert.Equal(1, builder.RedisConfig.EndPoints.Count);
            Assert.Equal(new DnsEndPoint("test", 9999), builder.RedisConfig.EndPoints[0]);
            Assert.True(builder.RedisConfig.Ssl);
            Assert.Null(builder.RedisConfig.Password);
            Assert.Null(builder.RedisConfig.DefaultDatabase);
        }

        [Fact]
        public void CanSetDatabase()
        {
            RedisFeatureStoreBuilder builder = new RedisFeatureStoreBuilder();
            builder.WithDatabaseIndex(8);
            Assert.Equal(8, builder.RedisConfig.DefaultDatabase);
        }

        [Fact]
        public void CanSetConnectTimeout()
        {
            RedisFeatureStoreBuilder builder = new RedisFeatureStoreBuilder();
            builder.WithConnectTimeout(TimeSpan.FromSeconds(8));
            Assert.Equal(8000, builder.RedisConfig.ConnectTimeout);
        }

        [Fact]
        public void CanSetResponseTimeout()
        {
            RedisFeatureStoreBuilder builder = new RedisFeatureStoreBuilder();
            builder.WithResponseTimeout(TimeSpan.FromSeconds(8));
            Assert.Equal(8000, builder.RedisConfig.ResponseTimeout);
        }
    }
}
