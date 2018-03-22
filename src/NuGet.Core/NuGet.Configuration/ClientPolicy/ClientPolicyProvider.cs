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
            throw new NotImplementedException();
        }

        public void SaveClientPolicy(ClientPolicy policy)
        {
            throw new NotImplementedException();
        }

        public void DeleteClientPolicy()
        {
            throw new NotImplementedException();
        }
    }
}
