// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGet.Configuration
{
    /// <summary>
    /// NuGet package signing client policy setting.
    /// Accept is the default policy.
    /// </summary>
    public enum ClientPolicy
    {
        Accept = 0,
        Require
    }
}
