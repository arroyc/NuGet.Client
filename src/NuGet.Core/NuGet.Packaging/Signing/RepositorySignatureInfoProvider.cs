// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Concurrent;

namespace NuGet.Packaging
{
    public class RepositorySignatureInfoProvider
    {
        private static readonly RepositorySignatureInfoProvider _instance = new RepositorySignatureInfoProvider();
        private ConcurrentDictionary<string, RepositorySignatureInfo> _dict = new ConcurrentDictionary<string, RepositorySignatureInfo>();

        public static RepositorySignatureInfoProvider Instance => _instance;

        private RepositorySignatureInfoProvider()
        {
        }

        public RepositorySignatureInfo GetRepositorySignatureInfo(string source)
        {
            RepositorySignatureInfo repositorySignatureInfo = null;

            _dict.TryGetValue(source, out repositorySignatureInfo);

            return repositorySignatureInfo;
        }

        public void AddRepositorySignatureInfo(string source, RepositorySignatureInfo repositorySignatureInfo)
        {
            _dict[source] = repositorySignatureInfo;
        }
    }
}
