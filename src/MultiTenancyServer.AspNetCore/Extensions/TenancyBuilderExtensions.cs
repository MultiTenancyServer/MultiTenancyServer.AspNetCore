// Copyright (c) Kris Penner. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using KodeAid;
using Microsoft.Extensions.Configuration;
using MultiTenancyServer.Configuration.DependencyInjection;
using MultiTenancyServer.Http.Parsers;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class TenancyBuilderExtensions
    {
        /// <summary>
        /// Helper functions for parsing the tenant from an HTTP request.
        /// </summary>
        /// <typeparam name="TTenant">The type representing a tenant.</typeparam>
        /// <typeparam name="TKey">The type of the primary key for a tenant.</typeparam>
        public static TenancyBuilder<TTenant, TKey> AddRequestParsers<TTenant, TKey>(this TenancyBuilder<TTenant, TKey> builder, Action<ICollection<IRequestParser>> parsers)
            where TTenant : class
            where TKey : IEquatable<TKey>
        {
            var p = new List<IRequestParser>();
            parsers(p);
            builder.Services.AddSingleton(p.AsEnumerable());
            return builder;
        }

        /// <summary>
        /// Adds a <see cref="DomainParser"/> to the collection of parsers for detecting the current tenant's canonical name by a custom domain name.
        /// </summary>
        /// <param name="parsers">Collection of parsers to add the <see cref="DomainParser"/> to.</param>
        /// <returns><paramref name="parsers"/> for fluent API.</returns>
        public static ICollection<IRequestParser> AddDomainParser(this ICollection<IRequestParser> parsers)
        {
            ArgCheck.NotNull(nameof(parsers), parsers);
            parsers.Add(new DomainParser());
            return parsers;
        }

        /// <summary>
        /// Adds a <see cref="HeaderParser"/> to the collection of parsers for detecting the current tenant's canonical name by an HTTP header.
        /// Eg. use "X-TENANT" for matching on X-TENANT = tenant1
        /// </summary>
        /// <param name="parsers">Collection of parsers to add the <see cref="HeaderParser"/> to.</param>
        /// <param name="headerName">The HTTP header name which will contain the tenant's canonical name of the request.</param>
        /// <returns><paramref name="parsers"/> for fluent API.</returns>
        public static ICollection<IRequestParser> AddHeaderParser(this ICollection<IRequestParser> parsers, string headerName)
        {
            ArgCheck.NotNull(nameof(parsers), parsers);
            ArgCheck.NotNullOrEmpty(nameof(headerName), headerName);
            parsers.Add(new HeaderParser() { HeaderName = headerName });
            return parsers;
        }

        /// <summary>
        /// Adds a <see cref="HostParser"/> to the collection of parsers for detecting the current tenant's canonical name by a sub-domain host based on a parent domain.
        /// Eg: use ".tenants.multitenancyserver.io" to match on "tenant1.tenants.multitenancyserver.io"
        /// </summary>
        /// <param name="parsers">Collection of parsers to add the <see cref="HostParser"/> to.</param>
        /// <param name="parentHostSuffix">The parent hostname suffix which will contain the tenant's canonical name as its only sub-domain hostname of the request.</param>
        /// <returns><paramref name="parsers"/> for fluent API.</returns>
        public static ICollection<IRequestParser> AddHostParserForParent(this ICollection<IRequestParser> parsers, string parentHostSuffix)
        {
            ArgCheck.NotNull(nameof(parsers), parsers);
            ArgCheck.NotNullOrEmpty(nameof(parentHostSuffix), parentHostSuffix);
            parsers.AddHostParser($@"^([a-z0-9-]+){Regex.Escape(parentHostSuffix).Replace(@"\*", @"[a-z0-9-]+")}$");
            return parsers;
        }

        /// <summary>
        /// Adds a <see cref="HostParser"/> to the collection of parsers for detecting the current tenant's canonical name by using a regular expression on the request's hostname.
        /// Eg: use @"^([a-z0-9][a-z0-9-]*[a-z0-9])(?:\.[a-z][a-z])?\.tenants\.multitenancyserver\.io$" for 
        /// matching on tenant1.eu.tenants.multitenancyserver.io where '.eu.' is an optional and dynamic two letter region code.
        /// The first group capture of a successful match is used, use anonymouse groups (?:) to avoid unwanted captures.
        /// </summary>
        /// <param name="parsers">Collection of parsers to add the <see cref="HostParser"/> to.</param>
        /// <param name="hostPattern">A regular expression to retreive the tenant canonical name from the full hostname (domain) of the request.</param>
        /// <returns><paramref name="parsers"/> for fluent API.</returns>
        public static ICollection<IRequestParser> AddHostParser(this ICollection<IRequestParser> parsers, string hostPattern)
        {
            ArgCheck.NotNull(nameof(parsers), parsers);
            ArgCheck.NotNullOrEmpty(nameof(hostPattern), hostPattern);
            parsers.Add(new HostParser() { HostPattern = hostPattern });
            return parsers;
        }

        /// <summary>
        /// Adds a <see cref="PathParser"/> to the collection of parsers for detecting the current tenant's canonical name by a child path based on a parent path.
        /// Eg: use "/tenants/" for matching on multitenancyserver.io/tenants/tenant1
        /// </summary>
        /// <param name="parsers">Collection of parsers to add the <see cref="PathParser"/> to.</param>
        /// <param name="parentHostSuffix">The parent path prefix which will contain the tenant's canonical name as its child path segment of the request.</param>
        /// <returns><paramref name="parsers"/> for fluent API.</returns>
        public static ICollection<IRequestParser> AddPathParserForParent(this ICollection<IRequestParser> parsers, string parentPathPrefix)
        {
            ArgCheck.NotNull(nameof(parsers), parsers);
            ArgCheck.NotNullOrEmpty(nameof(parentPathPrefix), parentPathPrefix);
            parsers.AddPathParser($@"^{Regex.Escape(parentPathPrefix).Replace(@"\*", @"[a-z0-9-]+")}([a-z0-9._~!$&'()*+,;=:@%-]+)(?:$|[#/?].*$)");
            return parsers;
        }

        /// <summary>
        /// Adds a <see cref="PathParser"/> to the collection of parsers for detecting the current tenant's canonical name by using a regular expression on the request's path.
        /// Eg: use "^/tenants/([a-z0-9]+)(?:[/]?)$" for matching on multitenancyserver.io/tenants/tenant1 or multitenancyserver.io/tenants/tenant1/
        /// </summary>
        /// <param name="parsers">Collection of parsers to add the <see cref="PathParser"/> to.</param>
        /// <param name="pathPattern">A regular expression to retreive the tenant canonical name from the path of the request.</param>
        /// <returns><paramref name="parsers"/> for fluent API.</returns>
        public static ICollection<IRequestParser> AddPathParser(this ICollection<IRequestParser> parsers, string pathPattern)
        {
            ArgCheck.NotNull(nameof(parsers), parsers);
            ArgCheck.NotNullOrEmpty(nameof(pathPattern), pathPattern);
            parsers.Add(new PathParser() { PathPattern = pathPattern });
            return parsers;
        }

        /// <summary>
        /// Adds a <see cref="QueryParser"/> to the collection of parsers for detecting the current tenant's canonical name by a query string parameter.
        /// Eg: use "tenant" for matching on ?tenant=tenant1
        /// </summary>
        /// <param name="parsers">Collection of parsers to add the <see cref="QueryParser"/> to.</param>
        /// <param name="headerName">The query string parameter name of the tenant canonical name.</param>
        /// <returns><paramref name="parsers"/> for fluent API.</returns>
        public static ICollection<IRequestParser> AddQueryParser(this ICollection<IRequestParser> parsers, string queryName)
        {
            ArgCheck.NotNull(nameof(parsers), parsers);
            ArgCheck.NotNullOrEmpty(nameof(queryName), queryName);
            parsers.Add(new QueryParser() { QueryName = queryName });
            return parsers;
        }
    }
}
