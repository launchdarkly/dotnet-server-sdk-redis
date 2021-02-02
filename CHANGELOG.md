# Change log

All notable changes to the LaunchDarkly .NET SDK Redis integration will be documented in this file. This project adheres to [Semantic Versioning](http://semver.org).

## [2.0.0] - 2021-02-01
This release updates the third-party dependency on `StackExchange.Redis` to use the 2.x version of that library. For details about how `StackExchange.Redis` 2.x differs from the 1.x versions, see its [release notes](https://stackexchange.github.io/StackExchange.Redis/ReleaseNotes.html).

This version of `LaunchDarkly.ServerSdk.Redis` requires version 5.14 or higher of the LaunchDarkly .NET SDK (`LaunchDarkly.ServerSdk`). It supports both the older configuration API used in previous versions, and the newer configuration API that was introduced in version 5.14 of the SDK and in the [1.2.0](https://github.com/launchdarkly/dotnet-server-sdk-redis/releases/tag/1.2.0) release of this package. Using the newer API (see `LaunchDarkly.Client.Integrations.Redis` in this package) is preferable because that is how configuration will work in the 6.0 release of the SDK.

### Added:
- The `OperationTimeout` configuration property, which corresponds to `SyncTimeout` in the `StackExchange.Redis` API.

### Changed:
- The minimum version of `StackExchange.Redis` is now 2.0.513.
- The minimum version of `LaunchDarkly.ServerSdk` is now 5.14.0.
- There is no longer a separate `LaunchDarkly.ServerSdk.Redis.StrongName` package that is the strong-named version; instead, there is just `LaunchDarkly.ServerSdk.Redis` which is always strong-named. That distinction was previously necessary because the `StackExchange.Redis` package had both strong-named and non-strong-named versions, which is no longer the case.
- The lowest compatible version of .NET Framework is now 4.6.1 (because that is the lowest version supported by `StackExchange.Redis` 2.x). The package still has a .NET Standard 2.0 target as well.

### Removed:
- The `ResponseTimeout` configuration property, which is no longer supported by `StackExchange.Redis`.

## [1.2.0] - 2021-01-26
### Added:
- New classes `LaunchDarkly.Client.Integrations.Redis` and `LaunchDarkly.Client.Integrations.RedisDataStoreBuilder`, which serve the same purpose as the previous classes but are designed to work with the newer persistent data store API introduced in .NET SDK 5.14.0.

### Deprecated:
- The old API in the `LaunchDarkly.Client.Redis` namespace.

## [1.1.1] - 2019-05-13
### Changed:
- Corresponding to the SDK package name change from `LaunchDarkly.Client` to `LaunchDarkly.ServerSdk`, this package is now called `LaunchDarkly.ServerSdk.Redis` (or `LaunchDarkly.ServerSdk.Redis.StrongName`). The functionality of the package, including the namespaces and class names, has not changed.

## [1.1.0] - 2019-01-14
### Added
- `RedisFeatureStoreBuilder.WithCaching` is the new way to configure local caching behavior, using the new SDK class `FeatureStoreCacheConfig`. This allows you to specify a limit on the number of cached items, which was not previously possible. Future releases of the SDK may add more caching parameters, which will then be automatically supported by this library.
- The assemblies in this package now have Authenticode signatures.

### Changed
- The minimum LaunchDarkly.Client version for use with this library is now 5.6.1.

### Deprecated
- `RedisFeatureStoreBuilder.WithCacheExpiration`

## [1.0.0] - 2018-09-28

Initial release.
