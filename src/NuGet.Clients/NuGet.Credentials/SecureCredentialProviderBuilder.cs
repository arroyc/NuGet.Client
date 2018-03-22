using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Protocol.Core.Types;
using NuGet.Protocol.Plugins;

namespace NuGet.Credentials
{
    public class SecureCredentialProviderBuilder
    {
        private Common.ILogger _logger;
        public SecureCredentialProviderBuilder(Common.ILogger logger)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<ICredentialProvider>> BuildAll()
        {
           var availablePlugins =  await PluginManager.Instance.FindAvailablePlugins(CancellationToken.None);

            var plugins = new List<ICredentialProvider>();
            foreach(var pluginDiscoveryResult in availablePlugins)
            {
                if (pluginDiscoveryResult.PluginFile.State != PluginFileState.Valid)
                {
                    _logger.LogDebug("Will attempt to use " + pluginDiscoveryResult.PluginFile.Path + " as a credential provider");
                    plugins.Add(new SecurePluginCredentialProvider(pluginDiscoveryResult, _logger));
                }
                else
                {
                    // TODO NK - does this need localized
                    _logger.LogDebug("Skipping " + pluginDiscoveryResult.PluginFile.Path + " as a credential provider. " + pluginDiscoveryResult.Message);
                }
            }

            return plugins;
        }

    }
}
