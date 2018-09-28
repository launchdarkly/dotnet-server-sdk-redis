LaunchDarkly SDK for .NET - Redis integration
=============================================
[![CircleCI](https://circleci.com/gh/launchdarkly/dotnet-client-redis/tree/master.svg?style=svg)](https://circleci.com/gh/launchdarkly/dotnet-client-redis/tree/master)

This library provides a Redis-backed persistence mechanism (feature store) for the [LaunchDarkly .NET SDK](https://github.com/launchdarkly/dotnet-client), replacing the default in-memory feature store. The underlying Redis client implementation is [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis).

The minimum version of the LaunchDarkly .NET SDK for use with this library is 5.2.1.

.NET platform compatibility
---------------------------

This version of the library is compatible with .NET Framework version 4.5 and above, .NET Standard 1.6, and .NET Standard 2.0.

Quick setup
-----------

1. Use [NuGet](http://docs.nuget.org/docs/start-here/using-the-package-manager-console) to add this package to your project:

        Install-Package LaunchDarkly.Client.Redis

   Or, if you require a strong-named assembly (note that this will result in a transitive dependency on StackExchange.Redis.StrongName, rather than StackExchange.Redis):

        Install-Package LaunchDarkly.Client.Redis.StrongName

2. Import the package:

        using LaunchDarkly.Client.Redis;

3. When configuring your `LDClient`, add the Redis feature store:

        Configuration ldConfig = Configuration.Default("YOUR_SDK_KEY")
            .WithFeatureStoreFactory(RedisComponents.RedisFeatureStore());
        LdClient ldClient = new LdClient(ldConfig);

4. Optionally, you can change the Redis configuration by calling methods on the builder returned by `RedisFeatureStore()`:

        Configuration ldConfig = Configuration.Default("YOUR_SDK_KEY")
            .WithFeatureStoreFactory(
                RedisComponents.RedisFeatureStore()
                    .WithRedisHostAndPort("my-redis-host", 6379)
                    .WithConnectTimeout(TimeSpan.FromSeconds(3))
            );
        LdClient ldClient = new LdClient(ldConfig);

Caching behavior
----------------

To reduce traffic to the Redis server, there is an optional in-memory cache that retains the last known data for a configurable amount of time. This is on by default; to turn it off (and guarantee that the latest feature flag data will always be retrieved from Redis for every flag evaluation), configure the builder as follows:

                RedisComponents.RedisFeatureStore()
                    .WithCacheExpiration(TimeSpan.Zero)

About LaunchDarkly
-----------

* LaunchDarkly is a continuous delivery platform that provides feature flags as a service and allows developers to iterate quickly and safely. We allow you to easily flag your features and manage them from the LaunchDarkly dashboard.  With LaunchDarkly, you can:
    * Roll out a new feature to a subset of your users (like a group of users who opt-in to a beta tester group), gathering feedback and bug reports from real-world use cases.
    * Gradually roll out a feature to an increasing percentage of users, and track the effect that the feature has on key metrics (for instance, how likely is a user to complete a purchase if they have feature A versus feature B?).
    * Turn off a feature that you realize is causing performance problems in production, without needing to re-deploy, or even restart the application with a changed configuration file.
    * Grant access to certain features based on user attributes, like payment plan (eg: users on the ‘gold’ plan get access to more features than users in the ‘silver’ plan). Disable parts of your application to facilitate maintenance, without taking everything offline.
* LaunchDarkly provides feature flag SDKs for
    * [Java](http://docs.launchdarkly.com/docs/java-sdk-reference "Java SDK")
    * [JavaScript](http://docs.launchdarkly.com/docs/js-sdk-reference "LaunchDarkly JavaScript SDK")
    * [PHP](http://docs.launchdarkly.com/docs/php-sdk-reference "LaunchDarkly PHP SDK")
    * [Python](http://docs.launchdarkly.com/docs/python-sdk-reference "LaunchDarkly Python SDK")
    * [Python Twisted](http://docs.launchdarkly.com/docs/python-twisted-sdk-reference "LaunchDarkly Python Twisted SDK")
    * [Go](http://docs.launchdarkly.com/docs/go-sdk-reference "LaunchDarkly Go SDK")
    * [Node.JS](http://docs.launchdarkly.com/docs/node-sdk-reference "LaunchDarkly Node SDK")
    * [.NET](http://docs.launchdarkly.com/docs/dotnet-sdk-reference "LaunchDarkly .Net SDK")
    * [Ruby](http://docs.launchdarkly.com/docs/ruby-sdk-reference "LaunchDarkly Ruby SDK")
    * [iOS](http://docs.launchdarkly.com/docs/ios-sdk-reference "LaunchDarkly iOS SDK")
    * [Android](http://docs.launchdarkly.com/docs/android-sdk-reference "LaunchDarkly Android SDK")
* Explore LaunchDarkly
    * [launchdarkly.com](http://www.launchdarkly.com/ "LaunchDarkly Main Website") for more information
    * [docs.launchdarkly.com](http://docs.launchdarkly.com/  "LaunchDarkly Documentation") for our documentation and SDKs
    * [apidocs.launchdarkly.com](http://apidocs.launchdarkly.com/  "LaunchDarkly API Documentation") for our API documentation
    * [blog.launchdarkly.com](http://blog.launchdarkly.com/  "LaunchDarkly Blog Documentation") for the latest product updates
    * [Feature Flagging Guide](https://github.com/launchdarkly/featureflags/  "Feature Flagging Guide") for best practices and strategies
