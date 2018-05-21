// Copyright (c) Kris Penner. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Http;

namespace MultiTenancyServer.Http.Parsers
{
    public interface IRequestParser
    {
        string ParseRequest(HttpContext httpContext);
    }
}
