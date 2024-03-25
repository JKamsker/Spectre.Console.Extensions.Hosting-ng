// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console.Cli;
using Spectre.Console.Extensions.Hosting.Infrastructure;

namespace Spectre.Console.Extensions.Hosting.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void ServiceCollection_Contains_IArgsProvider_When_UseDefaultSpectreConsoleArgs_Is_Called()
    {
        // Arrange
        var host = new HostBuilder()
            .UseSpectreConsole(_ => { })
            .Build();

        // Act
        var services = host.Services;
        var service = services.GetService<IArgsProvider>();

        // Assert
        Assert.NotNull(service);
        Assert.Equal(typeof(CommandLineArgsProvider), service.GetType());
    }

    [Fact]
    public void ServiceCollection_Contains_Stored_IArgsProvider()
    {
        // Arrange
        var host = new HostBuilder()
            .UseSpectreConsole(_ => { })
            .ConfigureServices((_, collection) =>
            {
                collection.UseSpectreConsoleArgs("test", "args");
            })
            .Build();

        // Act
        var services = host.Services;
        var service = services.GetService<IArgsProvider>();

        // Assert
        Assert.NotNull(service);
        Assert.Equal(typeof(StoredArgsProvider), service.GetType());
        Assert.Equal(new[] { "test", "args" }, service.GetArgs());
    }

    [Fact]
    public void CommandApp_Values_Are_Injected_Via_ConfigureServices()
    {
        new HostBuilder()
            .UseSpectreConsole(command =>
            {
                command.AddCommand<TestCommand>("test");
                command.PropagateExceptions();
            })
            .ConfigureServices((_, collection) => collection.UseSpectreConsoleArgs("test", "-n", "abc"))
            .UseConsoleLifetime()
            .Build().Run();
        Assert.Equal(0, Environment.ExitCode);
    }

    [Fact]
    public void CommandApp_Values_Are_Injected_Via_Command_Configuration()
    {
        new HostBuilder()
             .UseSpectreConsole(command =>
             {
                 command.AddCommand<TestCommand>("test");
                 command.PropagateExceptions();
                 command.UseArgs("test", "-n", "abc");
             })
             .UseConsoleLifetime()
             .Build().Run();

        Assert.Equal(0, Environment.ExitCode);
    }

    [Fact]
    public void CommandApp_Different_Values_Result_In_Different_ExitCode()
    {
        new HostBuilder()
            .UseSpectreConsole(command =>
            {
                command.AddCommand<TestCommand>("test");
                command.PropagateExceptions();
            })
            .ConfigureServices((_, collection) => collection.UseSpectreConsoleArgs("test", "-n", "def"))
            .UseConsoleLifetime()
            .Build().Run();

        Assert.Equal(1, Environment.ExitCode);
    }

    public class TestSettings : CommandSettings
    {
        // --name abc
        [CommandOption("-n|--name <NAME>")]
        public string? Name { get; set; }
    }

    public class TestCommand : Command<TestSettings>
    {
        public override int Execute(CommandContext context, TestSettings settings)
        {
            if (settings.Name != "abc")
            {
                throw new InvalidOperationException("Name is not abc");
            }
            return 0;
        }
    }
}
