// Copyright (c) Kris Penner. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KodeAid.Text.Normalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using MultiTenancyServer.Http.Parsers;
using MultiTenancyServer.Stores;

namespace MultiTenancyServer.Http
{
    /// <summary>
    /// Default tenant request service.
    /// </summary>
    internal class TenancyRequestParser<TTenant> : ITenancyRequestParser<TTenant> where TTenant : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultTenantRequestParser"/> class.
        /// </summary>
        /// <param name="parsers">The parsers.</param>
        /// <param name="store">The tenant store.</param>
        /// <param name="logger">The logger.</param>
        public TenancyRequestParser(IEnumerable<IRequestParser> parsers, ITenantStore<TTenant> store, ILookupNormalizer lookupNormalizer, ILogger<TenancyRequestParser<TTenant>> logger)
        {
            _parsers = parsers ?? throw new ArgumentNullException(nameof(parsers));
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _lookupNormalizer = lookupNormalizer ?? throw new ArgumentNullException(nameof(lookupNormalizer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private readonly IEnumerable<IRequestParser> _parsers;
        private readonly ITenantStore<TTenant> _store;
        private readonly ILookupNormalizer _lookupNormalizer;
        private readonly ILogger _logger;

        /// <summary>
        /// Gets the tenant from the current HTTP request.
        /// </summary>
        /// <param name="httpContext">Current HTTP context for the request.</param>
        /// <returns>The tenant the request is for, otherwise null if undeterministic or not found.</returns>
        public async Task<TTenant> GetTenantFromRequestAsync(HttpContext httpContext, CancellationToken cancellationToken = default)
        {
            var request = httpContext.Request;

            foreach (var parser in _parsers)
            {
                var result = parser.ParseRequest(httpContext);
                if (result != null)
                {
                    var normalizedResult = _lookupNormalizer.Normalize(result);
                    var tenant = await _store.FindByCanonicalNameAsync(normalizedResult, cancellationToken).ConfigureAwait(false);
                    if (tenant != null)
                    {
                        _logger.LogDebug("Tenant {Id} found by {Parser} for value {CanonicalName} in request {HttpRequest}.", await _store.GetTenantIdAsync(tenant, cancellationToken).ConfigureAwait(false), parser.GetType().Name, result, request.GetDisplayUrl());
                        return tenant;
                    }
                    else
                        _logger.LogDebug("Tenant not found by {Parser} for value {CanonicalName} in request {HttpRequest}.", parser.GetType().Name, result, request.GetDisplayUrl());
                }
                else
                    _logger.LogDebug("Tenant not matched by {Parser} in request {HttpRequest}.", parser.GetType().Name, request.GetDisplayUrl());
            }

            return default;
        }
    }
}
