// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGet.Configuration
{
    public interface IClientPolicyProvider
    {
        /// <summary>
        /// Loads NuGet package signing client policy.
        /// </summary>
        /// <returns>ClientPolicy.</returns>
        ClientPolicy LoadClientPolicy();

        /// <summary>
        /// Saves NuGet package signing client policy.
        /// </summary>
        /// <param name="policy">ClientPolicy to be saved.</param>
        void SaveClientPolicy(ClientPolicy policy);

        /// <summary>
        /// Removes the NuGet package signing client policy.
        /// </summary>
        void DeleteClientPolicy();
    }
}
