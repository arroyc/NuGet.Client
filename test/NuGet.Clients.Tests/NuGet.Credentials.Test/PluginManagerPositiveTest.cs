using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json.Linq;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Protocol.Plugins;
using NuGet.Versioning;

namespace NuGet.Credentials.Test
{

    internal sealed class PositiveTestExpectation
    {
        internal IEnumerable<OperationClaim> OperationClaims { get; }
        public string PackageSourceRepository { get; }
        public JObject ServiceIndex { get; }
        public ConnectionOptions ClientConnectionOptions { get; }
        public SemanticVersion PluginVersion { get; }

        internal PositiveTestExpectation(
            string serviceIndexJson,
            string sourceUri,
            IEnumerable<OperationClaim> operationClaims,
            ConnectionOptions options,
            SemanticVersion pluginVersion
            )
        {
            var serviceIndex = string.IsNullOrEmpty(serviceIndexJson)
                ? null : new ServiceIndexResourceV3(JObject.Parse(serviceIndexJson), DateTime.UtcNow);

            OperationClaims = operationClaims;
            PackageSourceRepository = sourceUri;
            ClientConnectionOptions = options;
            PluginVersion = pluginVersion;
        }
    }

    internal sealed class PluginManagerPositiveTest : IDisposable
    {
        private const string _pluginPathsEnvironmentVariable = "NUGET_PLUGIN_PATHS";
        private const string _pluginRequestTimeoutEnvironmentVariable = "NUGET_PLUGIN_REQUEST_TIMEOUT_IN_SECONDS";
        private const string _pluginHandshakeTimeoutEnvironmentVariable = "NUGET_PLUGIN_HANDSHAKE_TIMEOUT_IN_SECONDS";
        private const string _pluginIdleTimeoutEnvironmentVariable = "NUGET_PLUGIN_IDLE_TIMEOUT_IN_SECONDS";

        private readonly Mock<IConnection> _connection;
        private readonly PositiveTestExpectation _expectations;
        private readonly Mock<IPluginFactory> _factory;
        private readonly Mock<IPlugin> _plugin;
        private readonly Mock<IPluginDiscoverer> _pluginDiscoverer;
        private readonly Mock<IEnvironmentVariableReader> _reader;

        internal PluginManager PluginManager { get; }


        private void EnsureAllEnvironmentVariablesAreCalled(string pluginFilePath)
        {
            _reader.Setup(x => x.GetEnvironmentVariable(
                    It.Is<string>(value => value == _pluginPathsEnvironmentVariable)))
                .Returns(pluginFilePath);
            _reader.Setup(x => x.GetEnvironmentVariable(
                    It.Is<string>(value => value == _pluginRequestTimeoutEnvironmentVariable)))
                        .Returns("RequestTimeout");
            _reader.Setup(x => x.GetEnvironmentVariable(
                    It.Is<string>(value => value == _pluginIdleTimeoutEnvironmentVariable)))
                        .Returns("IdleTimeout");
            _reader.Setup(x => x.GetEnvironmentVariable(
                    It.Is<string>(value => value == _pluginHandshakeTimeoutEnvironmentVariable)))
                        .Returns("HandshakeTimeout");
        }

        private void EnsureDiscovererIsCalled(string pluginFilePath, PluginFileState pluginFileState)
        {
            _pluginDiscoverer.Setup(x => x.Dispose());
            _pluginDiscoverer.Setup(x => x.DiscoverAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[]
                    {
                            new PluginDiscoveryResult(new PluginFile(pluginFilePath, pluginFileState))
                    });

        }

        private void EnsureBasicPluginSetupCalls()
        {
            _connection.Setup(x => x.Dispose());

            _connection.Setup(x => x.SendRequestAndReceiveResponseAsync<MonitorNuGetProcessExitRequest, MonitorNuGetProcessExitResponse>(
                    It.Is<MessageMethod>(m => m == MessageMethod.MonitorNuGetProcessExit),
                    It.IsNotNull<MonitorNuGetProcessExitRequest>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MonitorNuGetProcessExitResponse(MessageResponseCode.Success));

            _connection.Setup(x => x.SendRequestAndReceiveResponseAsync<InitializeRequest, InitializeResponse>(
                    It.Is<MessageMethod>(m => m == MessageMethod.Initialize),
                    It.IsNotNull<InitializeRequest>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new InitializeResponse(MessageResponseCode.Success));

        }

        private void EnsurePluginSetupCalls()
        {

            _plugin.Setup(x => x.Dispose());
            _plugin.SetupGet(x => x.Connection)
                .Returns(_connection.Object);
            _plugin.SetupGet(x => x.Id)
                                .Returns("id");
        }
        private void EnsureFactorySetupCalls(string pluginFilePath)
        {
            _factory.Setup(x => x.Dispose());
            _factory.Setup(x => x.GetOrCreateAsync(
                    It.Is<string>(p => p == pluginFilePath),
                    It.IsNotNull<IEnumerable<string>>(),
                    It.IsNotNull<IRequestHandlers>(),
                    It.IsNotNull<ConnectionOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(_plugin.Object);
        }

        internal PluginManagerPositiveTest(
                            string pluginFilePath,
                            PluginFileState pluginFileState,
                PositiveTestExpectation expectations
            )
        {
            _expectations = expectations;

            _reader = new Mock<IEnvironmentVariableReader>(MockBehavior.Strict);
            EnsureAllEnvironmentVariablesAreCalled(pluginFilePath);

            _pluginDiscoverer = new Mock<IPluginDiscoverer>(MockBehavior.Strict);
            EnsureDiscovererIsCalled(pluginFilePath, pluginFileState);

            _connection = new Mock<IConnection>(MockBehavior.Strict);
            EnsureBasicPluginSetupCalls();

            _plugin = new Mock<IPlugin>(MockBehavior.Strict);
            EnsurePluginSetupCalls();

            _factory = new Mock<IPluginFactory>(MockBehavior.Strict);
            EnsureFactorySetupCalls(pluginFilePath);

            // Setup connection
            _connection.SetupGet(x => x.Options)
                .Returns(expectations.ClientConnectionOptions);

            _connection.SetupGet(x => x.ProtocolVersion)
                            .Returns(expectations.PluginVersion);

            // Setup expectations
            _connection.Setup(x => x.SendRequestAndReceiveResponseAsync<GetOperationClaimsRequest, GetOperationClaimsResponse>(
                    It.Is<MessageMethod>(m => m == MessageMethod.GetOperationClaims),
                    It.Is<GetOperationClaimsRequest>(
                        g => g.PackageSourceRepository == expectations.PackageSourceRepository),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetOperationClaimsResponse(expectations.OperationClaims.ToArray()));

            if (expectations.OperationClaims.Any())
            {
                _connection.Setup(x => x.SendRequestAndReceiveResponseAsync<SetCredentialsRequest, SetCredentialsResponse>(
                        It.Is<MessageMethod>(m => m == MessageMethod.SetCredentials),
                        It.Is<SetCredentialsRequest>(
                            g => g.PackageSourceRepository == expectations.PackageSourceRepository),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new SetCredentialsResponse(MessageResponseCode.Success));
            }

            PluginManager = PluginManager.Instance;
            PluginManager.Reinitialize(
                _reader.Object,
                                new Lazy<IPluginDiscoverer>(() => _pluginDiscoverer.Object),
                                (TimeSpan idleTimeout) => _factory.Object);
        }


        public void Dispose()
        {
            PluginManager.Dispose();
            GC.SuppressFinalize(this);

            _reader.Verify();
            _pluginDiscoverer.Verify();

            _connection.Verify(x => x.SendRequestAndReceiveResponseAsync<GetOperationClaimsRequest, GetOperationClaimsResponse>(
                It.Is<MessageMethod>(m => m == MessageMethod.GetOperationClaims),
                It.Is<GetOperationClaimsRequest>(
                    g => g.PackageSourceRepository == null), // The source repository should be null in the context of credential plugins
                It.IsAny<CancellationToken>()), Times.Once());

            var expectedSetCredentialsRequestCalls = _expectations.OperationClaims.Any()
                ? Times.Once() : Times.Never();

            _connection.Verify(x => x.SendRequestAndReceiveResponseAsync<SetCredentialsRequest, SetCredentialsResponse>(
                It.Is<MessageMethod>(m => m == MessageMethod.SetCredentials),
                It.Is<SetCredentialsRequest>(
                    g => g.PackageSourceRepository == null),
                It.IsAny<CancellationToken>()), expectedSetCredentialsRequestCalls);

            _plugin.Verify();
            _factory.Verify();
        }
    }

}