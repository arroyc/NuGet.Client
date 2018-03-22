// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json.Linq;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Protocol.Plugins;
using NuGet.Test.Utility;
using Xunit;

namespace NuGet.Credentials.Test
{
    public class SecurePluginCredentialProviderTests
    {
        
        private const string _sourceUri = "https://unit.test";


        [Fact]
        public void Create_ThrowsForNullPlugin()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => new SecurePluginCredentialProvider(null, NullLogger.Instance));

            Assert.Equal("pluginDiscoveryResult", exception.ParamName);
        }

        [Fact]
        public void Create_ThrowsForNullLogger()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => new SecurePluginCredentialProvider(CreatePluginDiscoveryResult(), null));

            Assert.Equal("logger", exception.ParamName);
        }

        [Fact]
        public void Create_ThrowsForInvalidPlugin()
        {
            var exception = Assert.Throws<ArgumentException>(
                () => new SecurePluginCredentialProvider(CreatePluginDiscoveryResult(PluginFileState.InvalidFilePath), NullLogger.Instance));
        }

        [Fact]
        public void Type_IsICredentialProvider()
        {
            var provider = new SecurePluginCredentialProvider(CreatePluginDiscoveryResult(), NullLogger.Instance);
            Assert.True(provider is ICredentialProvider);
        }

        [Fact]
        public void Provider_IdContainsPath()
        {
            var pluginResult = CreatePluginDiscoveryResult();
            var provider = new SecurePluginCredentialProvider(pluginResult, NullLogger.Instance);
            Assert.Contains(pluginResult.PluginFile.Path, provider.Id);
        }


        private static PluginDiscoveryResult CreatePluginDiscoveryResult(PluginFileState pluginState = PluginFileState.Valid)
        {
            return new PluginDiscoveryResult(new PluginFile(@"C:\random\path\plugin.exe", pluginState));
        }


        // TODO NK

        [PlatformFact(Platform.Windows)]
        public async Task TryCreate_ReturnsValidCredentials()
        {

            var expectation = new PositiveTestExpectation(null, null, new[] { OperationClaim.DownloadPackage }, ConnectionOptions.CreateDefault(), Protocol.Plugins.ProtocolConstants.CurrentVersion);

            using (var test = new PluginManagerPositiveTest(
                pluginFilePath: "a",
                pluginFileState: PluginFileState.Valid,
                expectations: expectation))
            {
                var discoveryResult = new PluginDiscoveryResult(new PluginFile("a", PluginFileState.Valid));
                var provider = new SecurePluginCredentialProvider(discoveryResult, NullLogger.Instance);

                provider.GetAsync(new Uri("bla"), null, CredentialRequestType.Unauthorized, "bla", false, true, CancellationToken.None);

                //var result = await test.Provider.TryCreate(expectations[0].SourceRepository, CancellationToken.None);

                //Assert.True(result.Item1);
                //Assert.IsType<PluginResource>(result.Item2);

                //var pluginResource = (PluginResource)result.Item2;
                //var pluginResult = await pluginResource.GetPluginAsync(
                //    OperationClaim.DownloadPackage,
                //    CancellationToken.None);

                //Assert.NotNull(pluginResult);
                //Assert.NotNull(pluginResult.Plugin);
                //Assert.NotNull(pluginResult.PluginMulticlientUtilities);
            }
        }

        //[PlatformFact(Platform.Windows)]
        //public async Task TryCreate_QueriesPluginForEachPackageSourceRepository()
        //{
        //    var expectations = new[]
        //        {
        //            new PositiveTestExpectation("{\"serviceIndex\":1}", "https://1.unit.test", new [] { OperationClaim.DownloadPackage }),
        //            new PositiveTestExpectation("{\"serviceIndex\":2}", "https://2.unit.test", Enumerable.Empty<OperationClaim>()),
        //            new PositiveTestExpectation("{\"serviceIndex\":3}", "https://3.unit.test", new [] { OperationClaim.DownloadPackage })
        //        };

        //    using (var test = new PluginResourceProviderPositiveTest(
        //        pluginFilePath: "a",
        //        pluginFileState: PluginFileState.Valid,
        //        expectations: expectations))
        //    {
        //        IPlugin firstPluginResult = null;
        //        IPlugin thirdPluginResult = null;

        //        for (var i = 0; i < expectations.Length; ++i)
        //        {
        //            var expectation = expectations[i];
        //            var result = await test.Provider.TryCreate(expectation.SourceRepository, CancellationToken.None);

        //            Assert.True(result.Item1);
        //            Assert.IsType<PluginResource>(result.Item2);

        //            var pluginResource = (PluginResource)result.Item2;
        //            var pluginResult = await pluginResource.GetPluginAsync(
        //                OperationClaim.DownloadPackage,
        //                CancellationToken.None);

        //            switch (i)
        //            {
        //                case 0:
        //                    firstPluginResult = pluginResult.Plugin;
        //                    break;

        //                case 1:
        //                    Assert.Null(pluginResult);
        //                    break;

        //                case 2:
        //                    thirdPluginResult = pluginResult.Plugin;
        //                    break;
        //            }
        //        }

        //        Assert.NotNull(firstPluginResult);
        //        Assert.Same(firstPluginResult, thirdPluginResult);
        //    }
        //}
    }
}