LaunchDarkly SDK Shared Test Code
=================================
[![CircleCI](https://circleci.com/gh/launchdarkly/dotnet-client-shared-tests.svg?style=svg)](https://circleci.com/gh/launchdarkly/dotnet-client-shared-tests)

This project provides support code for testing LaunchDarkly .NET SDK integrations. Feature store implementations, etc., should use this code whenever possible to ensure consistent test coverage and avoid repetition. An example of a project using this code is [dotnet-client-redis](https://github.com/launchdarkly/dotnet-client-redis).

The code is not published to NuGet, since it isn't of any use in any non-test context. Instead, it's meant to be used as a Git subtree. Add the subtree to your project like this:

    git remote add dotnet-client-shared-tests git@github.com:launchdarkly/dotnet-client-shared-tests.git
    git subtree add --squash --prefix=dotnet-client-shared-tests/ dotnet-client-shared-tests master

Now you can add the project `LaunchDarkly.Client.SharedTests.csproj` within the subtree to your solution, and reference its classes in your tests.

To update the copy of `dotnet-client-shared-tests` in your repository to reflect changes in this one:

    git subtree pull --squash --prefix=dotnet-client-shared-tests/ dotnet-client-shared-tests master
