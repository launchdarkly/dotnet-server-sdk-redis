﻿using System.Reflection;
using System.Runtime.CompilerServices;

// Allow unit tests to see internal classes
[assembly: InternalsVisibleTo("LaunchDarkly.Client.Redis.Tests")]

// Allow mock/proxy objects in unit tests to access internal classes
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2, PublicKey=" +
"0024000004800000940000000602000000240000525341310004000001000100c547cac37abd99" +
"c8db225ef2f6c8a3602f3b3606cc9891605d02baa56104f4cfc0734aa39b93bf7852f7d9266654" +
"753cc297e7d2edfe0bac1cdcf9f717241550e0a7b191195b7667bb4f64bcb8e2121380fd1d9d46" +
"ad2d92d2d15605093924cceaf74c4861eff62abf69b9291ed0a340e113be11e6a7d3113e92484c" +
"f7045cc7")]