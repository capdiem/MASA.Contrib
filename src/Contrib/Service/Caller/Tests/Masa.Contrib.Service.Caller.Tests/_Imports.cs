// Copyright (c) MASA Stack All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

global using Masa.BuildingBlocks.Configuration;
global using Masa.BuildingBlocks.Development.DaprStarter;
global using Masa.BuildingBlocks.Service.Caller;
global using Masa.Contrib.Service.Caller.DaprClient;
global using Masa.Contrib.Service.Caller.HttpClient;
global using Masa.Contrib.Service.Caller.Tests.Queries;
global using Masa.Contrib.Service.Caller.Tests.Requesties;
global using Masa.Contrib.Service.Caller.Tests.Response;
global using Microsoft.AspNetCore.Builder;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using Microsoft.VisualStudio.TestTools.UnitTesting;
global using Moq;
global using Moq.Protected;
global using System.Net;
global using System.Net.Http.Json;
global using System.Net.NetworkInformation;
global using System.Reflection;
global using System.Runtime.ExceptionServices;
global using System.Text;
global using System.Text.Json;
global using System.Text.Json.Serialization;
global using System.Xml.Serialization;
