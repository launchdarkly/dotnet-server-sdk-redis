# LaunchDarkly Server-Side SDK for .NET - Redis integration

[![CircleCI](https://circleci.com/gh/launchdarkly/dotnet-server-sdk-redis.svg?style=svg)](https://circleci.com/gh/launchdarkly/dotnet-server-sdk-redis)

This library provides a Redis-backed persistence mechanism (data store) for the [LaunchDarkly .NET SDK](https://github.com/launchdarkly/dotnet-server-sdk), replacing the default in-memory data store. The underlying Redis client implementation is [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis).

For more information, see also: [Using a persistent data store](https://docs.launchdarkly.com/v2.0/docs/using-a-persistent-feature-store).

Version 3.0.0 and above of this library works with version 6.0.0 and above of the LaunchDarkly .NET SDK. For earlier versions of the SDK, use the latest 1.x release of this library.

It has a dependency on StackExchange.Redis version 2.0.513. If you are using a higher version of StackExchange.Redis, you should install it explicitly as a dependency in your application to override this minimum version.

## Supported .NET versions

This version of the library is built for the following targets:

* .NET Framework 4.6.1: works in .NET Framework of that version or higher.
* .NET Standard 2.0: works in .NET Core 2.x, .NET 5.x, or in a library targeted to .NET Standard 2.x or .NET 5.x.

The .NET build tools should automatically load the most appropriate build of the library for whatever platform your application or library is targeted to.

## Quick setup

This assumes that you have already installed the LaunchDarkly .NET SDK.

1. Use [NuGet](http://docs.nuget.org/docs/start-here/using-the-package-manager-console) to add this package to your project:

        Install-Package LaunchDarkly.ServerSdk.Redis

   (Previous versions of the library had two package names, because StackExchange.Redis had two different packages depending on whether you wanted strong-naming, but StackExchange.Redis 2.x no longer does this so there is only one LaunchDarkly.ServerSdk.Redis package now.)

2. Import the package (note that the namespace is different from the package name):

        using LaunchDarkly.Sdk.Server.Integrations;

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

## Signing

The published version of this assembly is both digitally signed by LaunchDarkly and [strong-named](https://docs.microsoft.com/en-us/dotnet/framework/app-domains/strong-named-assemblies).

Building the code locally in the default Debug configuration does not sign the assembly and does not require a key file.

## Contributing
 
We encourage pull requests and other contributions from the community. Check out our [contributing guidelines](CONTRIBUTING.md) for instructions on how to contribute to this project.

## About LaunchDarkly
 
* LaunchDarkly is a continuous delivery platform that provides feature flags as a service and allows developers to iterate quickly and safely. We allow you to easily flag your features and manage them from the LaunchDarkly dashboard.  With LaunchDarkly, you can:
    * Roll out a new feature to a subset of your users (like a group of users who opt-in to a beta tester group), gathering feedback and bug reports from real-world use cases.
    * Gradually roll out a feature to an increasing percentage of users, and track the effect that the feature has on key metrics (for instance, how likely is a user to complete a purchase if they have feature A versus feature B?).
    * Turn off a feature that you realize is causing performance problems in production, without needing to re-deploy, or even restart the application with a changed configuration file.
    * Grant access to certain features based on user attributes, like payment plan (eg: users on the ‘gold’ plan get access to more features than users in the ‘silver’ plan). Disable parts of your application to facilitate maintenance, without taking everything offline.
* LaunchDarkly provides feature flag SDKs for a wide variety of languages and technologies. Check out [our documentation](https://docs.launchdarkly.com/docs) for a complete list.
* Explore LaunchDarkly
    * [launchdarkly.com](https://www.launchdarkly.com/ "LaunchDarkly Main Website") for more information
    * [docs.launchdarkly.com](https://docs.launchdarkly.com/  "LaunchDarkly Documentation") for our documentation and SDK reference guides
    * [apidocs.launchdarkly.com](https://apidocs.launchdarkly.com/  "LaunchDarkly API Documentation") for our API documentation
    * [blog.launchdarkly.com](https://blog.launchdarkly.com/  "LaunchDarkly Blog Documentation") for the latest product updates
    * [Feature Flagging Guide](https://github.com/launchdarkly/featureflags/  "Feature Flagging Guide") for best practices and strategies
