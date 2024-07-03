using Microsoft.Extensions.Hosting;

using Spectre.Console.Cli;
using Spectre.Console.Extensions.Hosting.Infrastructure;

namespace Spectre.Console.Extensions.Hosting.Worker;

public class SpectreConsoleWorker : BackgroundService
{
    private readonly ICommandApp _commandApp;
    private readonly IHostApplicationLifetime _hostLifetime;
    private readonly IArgsProvider _argsProvider;

    public SpectreConsoleWorker
    (
        ICommandApp commandApp,
        IHostApplicationLifetime hostLifetime,
        IArgsProvider? argsProvider = null
    )
    {
        _commandApp = commandApp;
        _hostLifetime = hostLifetime;
        _argsProvider = argsProvider ?? new CommandLineArgsProvider();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var args = _argsProvider.GetArgs();
            Environment.ExitCode = await _commandApp.RunAsync(args);
        }
        finally
        {
            _hostLifetime.StopApplication();
        }
    }
}