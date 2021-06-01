# Change log

All notable changes to the LaunchDarkly .NET SDK Redis integration will be documented in this file. This project adheres to [Semantic Versioning](http://semver.org).

## [1.2.1] - 2021-06-01
### Fixed:
- The library was not fully compliant with the standard usage of Redis keys by other LaunchDarkly SDKs and by the Relay Proxy, as follows: although feature flag data was stored with the correct keys, the wrong key was used for the special value that indicates that the database has been initialized. As a result, if the Relay Proxy had stored data in Redis, the .NET SDK would not detect it, and if the .NET SDK had stored data in Redis, other SDKs might not detect it.

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
