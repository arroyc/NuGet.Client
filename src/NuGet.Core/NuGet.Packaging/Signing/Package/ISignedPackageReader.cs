// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Packaging.Core;

namespace NuGet.Packaging.Signing
{
    /// <summary>
    /// A readonly package that can provide signatures and a sign manifest from a package.
    /// </summary>
    public interface ISignedPackageReader : IDisposable
    {
        /// <summary>
        /// Return true if the package is needed to be repository signed.
        /// </summary>
        bool RequiredRepoSign { get; }

        bool PackageSignatureVerified { get; }

        IEnumerable<IRepositoryCertInfo> RepositoryCertInfos { get; }

        /// <summary>
        /// Get package signature.
        /// </summary>
        /// <remarks>Returns a null if the package is unsigned.</remarks>
        Task<PrimarySignature> GetPrimarySignatureAsync(CancellationToken token);

        /// <summary>
        /// Check if a package contains signing information.
        /// </summary>
        /// <returns>True if the package is signed.</returns>
        Task<bool> IsSignedAsync(CancellationToken token);

        /// <summary>
        /// Gets the hash of an archive to be embedded in the package signature.
        /// </summary>
        Task<byte[]> GetArchiveHashAsync(HashAlgorithmName hashAlgorithm, CancellationToken token);

        /// <summary>
        /// Checks for the integrity of a package
        /// </summary>
        /// <param name="signatureContent">SignatureContent with expected hash value and hash algorithm used</param>
        /// <returns></returns>
        Task ValidateIntegrityAsync(SignatureContent signatureContent, CancellationToken token);
    }
}
