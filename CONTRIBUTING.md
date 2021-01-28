# Contributing to the LaunchDarkly SDK Redis Integration

The source code for this library is [here](https://github.com/launchdarkly/dotnet-server-sdk-redis). We encourage pull-requests and other contributions from the community. Since this library is meant to be used in conjunction with the LaunchDarkly .NET SDK, you may want to look at the [.NET SDK source code](https://github.com/launchdarkly/dotnet-server-sdk) and our [SDK contributor's guide](http://docs.launchdarkly.com/docs/sdk-contributors-guide).

## Submitting bug reports and feature requests
 
The LaunchDarkly SDK team monitors the [issue tracker](https://github.com/launchdarkly/dotnet-server-sdk-redis/issues) in this repository. Bug reports and feature requests specific to this project should be filed in the issue tracker. The SDK team will respond to all newly filed issues within two business days.
 
## Submitting pull requests
 
We encourage pull requests and other contributions from the community. Before submitting pull requests, ensure that all temporary or unintended code is removed. Don't worry about adding reviewers to the pull request; the LaunchDarkly SDK team will add themselves. The SDK team will acknowledge all pull requests within two business days.
 
## Build instructions
 
### Prerequisites

This project has multiple target frameworks as described in [`README.md`](./README.md). The .NET Framework target can be built only in a Windows environment; the others can be built either with or without a Windows environment. Download and install the latest .NET SDK tools first.

The project has a package dependency on `StackExchange.Redis`. The dependency version is intended to be the _minimum_ compatible version; applications are expected to override this with their own dependency on some higher version.

This project has two targets: .NET Standard 2.0 and .NET Framework 4.5.2. In Windows, you can build both; outside of Windows, you will need to [download .NET Core and follow the instructions](https://dotnet.microsoft.com/download) (make sure you have 2.0 or higher) and can only build the .NET Standard target.

### Building

To install all required packages:

```
dotnet restore
```

To build all targets of the project without running any tests:

```
dotnet build src/LaunchDarkly.ServerSdk.Redis
```

Or, to build only one target (in this case .NET Standard 2.0):

```
dotnet build src/LaunchDarkly.ServerSdk.Redis -f netstandard2.0
```

Building the code locally in the default Debug configuration does not sign the assembly and does not require a key file.

### Testing

To run all unit tests, for all targets (this includes .NET Framework, so you can only do this in Windows):

```
dotnet test test/LaunchDarkly.ServerSdk.Redis.Tests
```

Or, to run tests only for the .NET Standard 2.0 target (using the .NET Core 2.1 runtime):

```
dotnet test test/LaunchDarkly.ServerSdk.Redis.Tests -f netcoreapp2.1
```

The tests expect you to have Redis running locally on the default port, 6379. One way to do this is with Docker:

```bash
docker run -p 6379:6379 redis
```
