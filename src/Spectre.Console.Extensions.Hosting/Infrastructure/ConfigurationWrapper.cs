// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console.Cli;
using Spectre.Console.Cli.Help;

namespace Spectre.Console.Extensions.Hosting.Infrastructure;

internal class ConfigurationWrapper : IConfigurator
{
    private readonly IConfigurator _configurator;
    private readonly Overridable<IArgsProvider> _argsProvider;

    public ConfigurationWrapper(IConfigurator configurator, Overridable<IArgsProvider> argsProvider)
    {
        _configurator = configurator;
        _argsProvider = argsProvider;
    }

    public ICommandAppSettings Settings => _configurator.Settings;

    public ICommandConfigurator AddAsyncDelegate<TSettings>(string name, Func<CommandContext, TSettings, Task<int>> func) where TSettings : CommandSettings
    {
        return _configurator.AddAsyncDelegate(name, func);
    }

    public IBranchConfigurator AddBranch<TSettings>(string name, Action<IConfigurator<TSettings>> action) where TSettings : CommandSettings
    {
        return _configurator.AddBranch(name, action);
    }

    public ICommandConfigurator AddDelegate<TSettings>(string name, Func<CommandContext, TSettings, int> func) where TSettings : CommandSettings
    {
        return _configurator.AddDelegate(name, func);
    }

    public void AddExample(params string[] args)
    {
        _configurator.AddExample(args);
    }

    public void SetHelpProvider(IHelpProvider helpProvider)
    {
        _configurator.SetHelpProvider(helpProvider);
    }

    public void SetHelpProvider<T>() where T : IHelpProvider
    {
        _configurator.SetHelpProvider<T>();
    }

    ICommandConfigurator IConfigurator.AddCommand<TCommand>(string name)
    {
        return _configurator.AddCommand<TCommand>(name);
    }

    public void UseArgs(params string[] args)
    {
        _argsProvider.Override(new StoredArgsProvider(args));
    }
}
