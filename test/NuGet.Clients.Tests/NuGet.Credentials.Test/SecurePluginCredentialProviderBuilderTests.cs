using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json.Linq;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Protocol.Plugins;
using Xunit;

namespace NuGet.Credentials.Test
{
    public class SecurePluginCredentialProviderBuilderTests
    {
        private const string _pluginHandshakeTimeoutEnvironmentVariable = "NUGET_PLUGIN_HANDSHAKE_TIMEOUT_IN_SECONDS";
        private const string _pluginIdleTimeoutEnvironmentVariable = "NUGET_PLUGIN_IDLE_TIMEOUT_IN_SECONDS";
        private const string _pluginPathsEnvironmentVariable = "NUGET_PLUGIN_PATHS";
        private const string _pluginRequestTimeoutEnvironmentVariable = "NUGET_PLUGIN_REQUEST_TIMEOUT_IN_SECONDS";
        private const string _sourceUri = "https://unit.test";

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task TryCreate_ReturnsFalseForNullOrEmptyEnvironmentVariable(string pluginsPath)
        {
            var test = new PluginResourceProviderNegativeTest(
                serviceIndexJson: "{}",
                sourceUri: _sourceUri,
                pluginsPath: pluginsPath);

            var result = await test.Provider.TryCreate(test.SourceRepository, CancellationToken.None);

            Assert.False(result.Item1);
            Assert.Null(result.Item2);
        }

        [Fact]
        public async Task TryCreate_ReturnsFalseIfNoServiceIndexResourceV3()
        {
            var test = new PluginResourceProviderNegativeTest(
                serviceIndexJson: null,
                sourceUri: _sourceUri);

            var result = await test.Provider.TryCreate(test.SourceRepository, CancellationToken.None);

            Assert.False(result.Item1);
            Assert.Null(result.Item2);
        }

        [Theory]
        [InlineData("\\unit\test")]
        [InlineData("file:///C:/unit/test")]
        public async Task TryCreate_ReturnsFalseIfPackageSourceIsNotHttpOrHttps(string sourceUri)
        {
            var test = new PluginResourceProviderNegativeTest(
                serviceIndexJson: "{}",
                sourceUri: sourceUri);

            var result = await test.Provider.TryCreate(test.SourceRepository, CancellationToken.None);

            Assert.False(result.Item1);
            Assert.Null(result.Item2);
        }

        private sealed class PluginResourceProviderNegativeTest
        {
            internal PluginResourceProvider Provider { get; }
            internal SourceRepository SourceRepository { get; }

            internal PluginResourceProviderNegativeTest(string serviceIndexJson, string sourceUri, string pluginsPath = "a")
            {
                var serviceIndex = string.IsNullOrEmpty(serviceIndexJson)
                    ? null : new ServiceIndexResourceV3(JObject.Parse(serviceIndexJson), DateTime.UtcNow);

                SourceRepository = CreateSourceRepository(serviceIndex, sourceUri);

                var reader = new Mock<IEnvironmentVariableReader>(MockBehavior.Strict);

                reader.Setup(x => x.GetEnvironmentVariable(
                        It.Is<string>(value => value == _pluginPathsEnvironmentVariable)))
                    .Returns(pluginsPath);
                reader.Setup(x => x.GetEnvironmentVariable(
                        It.Is<string>(value => value == _pluginRequestTimeoutEnvironmentVariable)))
                    .Returns("b");
                reader.Setup(x => x.GetEnvironmentVariable(
                        It.Is<string>(value => value == _pluginIdleTimeoutEnvironmentVariable)))
                    .Returns("c");
                reader.Setup(x => x.GetEnvironmentVariable(
                        It.Is<string>(value => value == _pluginHandshakeTimeoutEnvironmentVariable)))
                    .Returns("d");

                var pluginDiscoverer = new Mock<IPluginDiscoverer>(MockBehavior.Strict);
                var pluginDiscoveryResults = GetPluginDiscoveryResults(pluginsPath);

                pluginDiscoverer.Setup(x => x.DiscoverAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(pluginDiscoveryResults);

                Provider = new PluginResourceProvider();

                PluginManager.Instance.Reinitialize(
                    reader.Object,
                    new Lazy<IPluginDiscoverer>(() => pluginDiscoverer.Object),
                    (TimeSpan idleTimeout) => Mock.Of<IPluginFactory>());
            }

            private static IEnumerable<PluginDiscoveryResult> GetPluginDiscoveryResults(string pluginPaths)
            {
                var results = new List<PluginDiscoveryResult>();

                if (string.IsNullOrEmpty(pluginPaths))
                {
                    return results;
                }

                foreach (var path in pluginPaths.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var state = path == "a" ? PluginFileState.Valid : PluginFileState.InvalidEmbeddedSignature;
                    var file = new PluginFile(path, state);

                    results.Add(new PluginDiscoveryResult(file));
                }

                return results;
            }


            private static SourceRepository CreateSourceRepository(
                ServiceIndexResourceV3 serviceIndexResource,
                string sourceUri)
            {
                var packageSource = new PackageSource(sourceUri);
                var provider = new Mock<ServiceIndexResourceV3Provider>();

                provider.Setup(x => x.Name)
                    .Returns(nameof(ServiceIndexResourceV3Provider));
                provider.Setup(x => x.ResourceType)
                    .Returns(typeof(ServiceIndexResourceV3));

                var tryCreateResult = new Tuple<bool, INuGetResource>(serviceIndexResource != null, serviceIndexResource);

                provider.Setup(x => x.TryCreate(It.IsAny<SourceRepository>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(tryCreateResult));

                return new SourceRepository(packageSource, new[] { provider.Object });
            }
        }

    }
}
