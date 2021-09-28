The [`LaunchDarkly.ServerSdk.Redis`](https://nuget.org/packages/LaunchDarkly.ServerSdk.Redis) package provides a Redis-backed persistence mechanism (data store) for the [LaunchDarkly .NET SDK](https://github.com/launchdarkly/dotnet-server-sdk), replacing the default in-memory data store. The underlying Redis client implementation is [`StackExchange.Redis`](https://github.com/StackExchange/StackExchange.Redis).

For more information, see also: [Using Redis as a persistent feature store](https://docs.launchdarkly.com/sdk/features/storing-data/redis#net).

Version 3.0.0 and above of this library works with version 6.0.0 and above of the LaunchDarkly .NET SDK. For earlier versions of the SDK, use the latest 1.x release of this library.

It has a dependency on `StackExchange.Redis` version 2.0.513. If you are using a higher version of `StackExchange.Redis`, you should install it explicitly as a dependency in your application to override this minimum version.

The entry point for using this integration is the **<xref:LaunchDarkly.Sdk.Server.Integrations.Redis>** class in <xref:LaunchDarkly.Sdk.Server.Integrations>.

## Quick setup

This assumes that you have already installed the LaunchDarkly .NET SDK.

1. Add the NuGet package [`LaunchDarkly.ServerSdk.Redis`](https://nuget.org/packages/LaunchDarkly.ServerSdk.Redis) to your project.

   (Previous versions of the library had two package names, because StackExchange.Redis had two different packages depending on whether you wanted strong-naming, but StackExchange.Redis 2.x no longer does this so there is only one `LaunchDarkly.ServerSdk.Redis` package now.)

2. Import the package (note that the namespace is different from the package name):

```csharp
        using LaunchDarkly.Sdk.Server.Integrations;
```

3. When configuring your `LdClient`, add the Redis data store as a `PersistentDataStore`. You may specify any custom Redis options using the methods of `RedisDataStoreBuilder`. For instance, to customize the Redis URI:

```csharp
        var ldConfig = Configuration.Default("YOUR_SDK_KEY")
            .DataStore(
                Components.PersistentDataStore(
                    Redis.DataStore().Uri("redis://my-redis-host")
                )
            )
            .Build();
        var ldClient = new LdClient(ldConfig);
```

By default, the store will try to connect to a local Redis instance on port 6379.

## Caching behavior

The LaunchDarkly SDK has a standard caching mechanism for any persistent data store, to reduce database traffic. This is configured through the SDK's `PersistentDataStoreBuilder` class as described in the SDK documentation. For instance, to specify a cache TTL of 5 minutes:

```csharp
        var config = Configuration.Default("YOUR_SDK_KEY")
            .DataStore(
                Components.PersistentDataStore(
                    Redis.DataStore().Uri("redis://my-redis-host")
                ).CacheTime(TimeSpan.FromMinutes(5))
            )
            .Build();
```
