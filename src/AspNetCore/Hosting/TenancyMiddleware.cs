// Copyright (c) Kris Penner. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using KodeAid;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MultiTenancyServer.Options;

namespace MultiTenancyServer.Hosting
{
    internal class TenancyMiddleware<TTenant> where TTenant : class
    {
        public TenancyMiddleware(RequestDelegate next, TenancyOptions options, ILogger<TenancyMiddleware<TTenant>> logger)
        {
            ArgCheck.NotNull(nameof(next), next);
            ArgCheck.NotNull(nameof(options), options);
            ArgCheck.NotNull(nameof(logger), logger);
            _next = next;
            _options = options;
            _logger = logger;
        }

        private readonly RequestDelegate _next;
        private readonly TenancyOptions _options;
        private readonly ILogger _logger;

        public async Task InvokeAsync(HttpContext httpContext, ITenancyContext<TTenant> tenancyContext, ITenancyProvider<TTenant> tenancyProvider)
        {
            tenancyContext.Tenant = await tenancyProvider.GetCurrentTenantAsync(httpContext.RequestAborted).ConfigureAwait(false);
            await _next(httpContext).ConfigureAwait(false);
        }
    }
}
