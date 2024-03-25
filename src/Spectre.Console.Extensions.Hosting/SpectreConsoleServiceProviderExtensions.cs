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
using Spectre.Console.Cli;
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
        var overridble = new Overridable<IArgsProvider>(argsProvider);
        serviceProvider.Replace(new ServiceDescriptor(typeof(Overridable<IArgsProvider>), overridble));

        serviceProvider.TryAddSingleton<IArgsProvider>(sp => sp.GetRequiredService<Overridable<IArgsProvider>>().Value);

        return serviceProvider;
    }

    public static IServiceProvider UseSpectreConsoleArgs(this IServiceProvider serviceProvider, params string[] args)
    {
        var overridable = serviceProvider.GetService<Overridable<IArgsProvider>>();
        if (overridable == null)
        {
            throw new InvalidOperationException("The IArgsProvider was not registered through the ``Spectre.Console.Extensions.Hosting`` package.");
        }

        overridable.Override(new StoredArgsProvider(args));

        return serviceProvider;
    }
}

public class Overridable<T>
{
    public T Value { get; private set; }

    public Overridable(T value)
    {
        Value = value;
    }

    public void Override(T value)
    {
        Value = value;
    }
}

public static class SpectreConsoleConfiurationExtensions
{
    public static void UseArgs(this IConfigurator configurator, params string[] args)
    {
        if (configurator is not ConfigurationWrapper wrapper)
        {
            throw new InvalidOperationException("The IConfigurator was not created through the ``Spectre.Console.Extensions.Hosting`` package.");
        }
        wrapper.UseArgs(args);
    }
}
