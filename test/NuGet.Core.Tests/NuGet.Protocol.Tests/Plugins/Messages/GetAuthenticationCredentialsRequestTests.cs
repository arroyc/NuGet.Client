using System;
using System.Collections.Generic;
using System.Text;
using NuGet.Protocol.Plugins.Messages;
using Xunit;

namespace NuGet.Protocol.Tests.Plugins.Messages
{
    public class GetAuthenticationCredentialsRequestTests
    {

        [Fact]
        public void Constructor_ThrowsForNullOrEmptyPackageSourceRepository()
        {
            Uri uri = null;
            var exception = Assert.Throws<ArgumentNullException>(
                () => new GetAuthenticationCredentialsRequest(
                    uri,
                    false,
                    false
                    ));
            Assert.Equal("uri", exception.ParamName);
        }

    }
}
