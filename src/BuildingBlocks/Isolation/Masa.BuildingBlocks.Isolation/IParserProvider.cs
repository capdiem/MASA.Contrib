﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace Masa.BuildingBlocks.Isolation;

public interface IParserProvider
{
    string Name { get; }

    Task<bool> ResolveAsync(HttpContext? httpContext, string key, Action<string> action);
}
