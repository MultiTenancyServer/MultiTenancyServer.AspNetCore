// Copyright (c) Kris Penner. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MultiTenancyServer.Http
{
    /// <summary>
    /// Implements tenant request logic.
    /// </summary>
    public interface ITenancyRequestParser<TTenant> where TTenant : class
    {
        /// <summary>
        /// Gets the tenant from the current HTTP request.
        /// </summary>
        /// <param name="httpContext">HTTP context for the current request.</param>
        /// <returns>The tenant the request is for, otherwise null if undeterministic or not found.</returns>
        Task<TTenant> GetTenantFromRequestAsync(HttpContext httpContext, CancellationToken cancellationToken = default);
    }
}
