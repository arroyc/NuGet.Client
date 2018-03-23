// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace NuGet.Configuration
{
    public class ClientPolicyProvider : IClientPolicyProvider
    {
        private Settings _settings;

        public ClientPolicyProvider(Settings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public ClientPolicy LoadClientPolicy()
        {
            var policy = ClientPolicy.Accept;
            var policyString = SettingsUtility.GetConfigValue(_settings, ConfigurationConstants.SignatureValidationMode);

            if (!string.IsNullOrEmpty(policyString))
            {
                Enum.TryParse(policyString, ignoreCase: true, result: out policy);
            }

            return policy;
        }

        public void SaveClientPolicy(ClientPolicy policy)
        {
            SettingsUtility.SetConfigValue(_settings, ConfigurationConstants.SignatureValidationMode, policy.ToString());
        }

        public void DeleteClientPolicy()
        {
            SettingsUtility.DeleteConfigValue(_settings, ConfigurationConstants.SignatureValidationMode);
        }
    }
}
