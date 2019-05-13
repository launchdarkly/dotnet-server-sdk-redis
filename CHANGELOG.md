# Change log

All notable changes to the LaunchDarkly .NET SDK Redis integration will be documented in this file. This project adheres to [Semantic Versioning](http://semver.org).

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
