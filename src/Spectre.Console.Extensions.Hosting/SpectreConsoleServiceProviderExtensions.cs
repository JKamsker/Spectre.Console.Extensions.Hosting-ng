// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Spectre.Console.Extensions.Hosting.Infrastructure;

namespace Spectre.Console.Extensions.Hosting;

public static class SpectreConsoleServiceCollectionExtensions
{
    public static IServiceCollection UseDefaultSpectreConsoleArgs(this IServiceCollection serviceProvider)
    {
        // Replace IArgsProvider with a default implementation
        serviceProvider.UseSpectreConsoleArgs(new CommandLineArgsProvider());
        return serviceProvider;
    }

    public static IServiceCollection UseSpectreConsoleArgs(this IServiceCollection serviceProvider, params string[] args)
    {
        // Replace IArgsProvider with a custom implementation
        serviceProvider.UseSpectreConsoleArgs(new StoredArgsProvider(args));
        return serviceProvider;
    }

    public static IServiceCollection UseSpectreConsoleArgs(this IServiceCollection serviceProvider, IArgsProvider argsProvider)
    {
        // Replace IArgsProvider with a custom implementation
        serviceProvider.Replace(new ServiceDescriptor(typeof(IArgsProvider), argsProvider));
        return serviceProvider;
    }
}
