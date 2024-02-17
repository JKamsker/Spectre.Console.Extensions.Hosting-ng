using System.Threading;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Spectre.Console.Cli;
using Spectre.Console.Extensions.Hosting.Infrastructure;

namespace Spectre.Console.Extensions.Hosting.Worker;

public class SpectreConsoleWorker : IHostedService
{
    private readonly ICommandApp _commandApp;
    private readonly IHostApplicationLifetime _hostLifetime;
    private readonly IArgsProvider _argsProvider;
    private readonly ILogger<SpectreConsoleWorker> _logger;
    private int _exitCode;
    private Task? _startupTask;

    public SpectreConsoleWorker
    (
        ILogger<SpectreConsoleWorker> logger,
        ICommandApp commandApp,
        IHostApplicationLifetime hostLifetime,
        IArgsProvider? argsProvider = null
    )
    {
        _logger = logger;
        _commandApp = commandApp;
        _hostLifetime = hostLifetime;
        _argsProvider = argsProvider ?? new CommandLineArgsProvider();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _startupTask = WaitAndStartAsync(cancellationToken);
        // If the task completed synchronously, await it in order to bubble potential cancellation/failure to the caller
        // Otherwise, return, allowing application startup to complete
        if (_startupTask.IsCompleted)
        {
            await _startupTask.ConfigureAwait(false);
        }
    }

    private async Task WaitAndStartAsync(CancellationToken startupCancellationToken)
    {
        // Wait until:
        // 1. The application is started
        // 2. The application is stopping due to a cancellation request

        using var combinedCancellationSource = CancellationTokenSource.CreateLinkedTokenSource(startupCancellationToken, _hostLifetime.ApplicationStarted);
        await Task.Delay(Timeout.InfiniteTimeSpan, combinedCancellationSource.Token) // Wait "indefinitely", until startup completes or is aborted
            .ContinueWith(_ => { }, TaskContinuationOptions.OnlyOnCanceled) // Without an OperationCanceledException on cancellation
            .ConfigureAwait(false);

        if (startupCancellationToken.IsCancellationRequested)
        {
            return;
        }

        await StartCommandAsync();
    }

    private async Task StartCommandAsync()
    {
        try
        {
            var args = _argsProvider.GetArgs();
            _exitCode = await _commandApp.RunAsync(args);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred");
            _exitCode = 1;
        }
        finally
        {
            _hostLifetime.StopApplication();
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        // Stopped without having been started
        if (_startupTask is null)
        {
            Environment.ExitCode = _exitCode;
            return;
        }

        // Wait until any ongoing startup logic has finished or the graceful shutdown period is over
        await Task.WhenAny(_startupTask, Task.Delay(Timeout.Infinite, cancellationToken)).ConfigureAwait(false);

        Environment.ExitCode = _exitCode;
    }
}