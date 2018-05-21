// Copyright (c) Kris Penner. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Http;

namespace MultiTenancyServer.Http.Parsers
{
    /// <summary>
    /// The tenant can be identified by a custom domain name that they have configued and mapped.
    /// </summary>
    public class DomainParser : RequestParser
    {
        /// <summary>
        /// Retrieves the value of the full domain hostname from a request.
        /// </summary>
        /// <param name="httpContext">The request to retrieve the full domain hostname from.</param>
        /// <returns>The value of the full domain hostname.</returns>
        public override string ParseRequest(HttpContext httpContext)
        {
            return httpContext.Request.Host.Host;
        }
    }
}
