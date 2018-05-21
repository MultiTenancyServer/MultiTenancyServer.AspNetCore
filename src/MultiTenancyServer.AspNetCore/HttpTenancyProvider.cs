// Copyright (c) Kris Penner. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using KodeAid;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MultiTenancyServer.Http
{
    /// <summary>
    /// Default tenant request service.
    /// </summary>
    internal class HttpTenancyProvider<TTenant> : ITenancyProvider<TTenant> where TTenant : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpTenancyProvider"/> class.
        /// </summary>
        /// <param name="httpContext">Context of the current HTTP request.</param>
        /// <param name="tenantRequestParser">Parses the tenant from the current request.</param>
        /// <param name="logger">The logger.</param>
        public HttpTenancyProvider(HttpContext httpContext, ITenancyRequestParser<TTenant> tenantRequestParser, ILogger<HttpTenancyProvider<TTenant>> logger)
        {
            ArgCheck.NotNull(nameof(httpContext), httpContext);
            ArgCheck.NotNull(nameof(tenantRequestParser), tenantRequestParser);
            ArgCheck.NotNull(nameof(logger), logger);
            _httpContext = httpContext;
            _tenantRequestParser = tenantRequestParser;
            _logger = logger;
        }

        private readonly HttpContext _httpContext;
        private readonly ITenancyRequestParser<TTenant> _tenantRequestParser;
        private readonly ILogger _logger;

        /// <summary>
        /// Gets the tenant from the current HTTP request.
        /// </summary>
        /// <param name="httpContext">Current HTTP context for the request.</param>
        /// <returns>The tenant the request is for, otherwise null if undeterministic or not found.</returns>
        public Task<TTenant> GetCurrentTenantAsync(CancellationToken cancellationToken = default)
        {
            return _tenantRequestParser.GetTenantFromRequestAsync(_httpContext, _httpContext.RequestAborted);
        }
    }
}
