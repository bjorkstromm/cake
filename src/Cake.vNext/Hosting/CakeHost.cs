using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.Scripting;
using Microsoft.Extensions.Hosting;

namespace Cake.Hosting;

public sealed class CakeHost : ScriptHost, IHost
{
    public static CakeHostBuilder CreateDefaultBuilder(string[] args)
    {
        var builder = new CakeHostBuilder();
        builder.ConfigureDefaults(args);

        return builder;
    }

    private readonly IExecutionStrategy _executionStrategy;
    private readonly ICakeReportPrinter _reportPrinter;
    private readonly IHost _host;
    private readonly ICakeLog _log;

    public CakeHost(
        ICakeEngine engine,
        ICakeContext context,
        IExecutionStrategy executionStrategy,
        ICakeReportPrinter reportPrinter,
        IHost host)
        : base(engine, context)
    {
        _executionStrategy = executionStrategy;
        _reportPrinter = reportPrinter;
        _host = host;
    }

    public IServiceProvider Services => _host.Services;

    public void Dispose() => _host.Dispose();

    public Task StartAsync(CancellationToken cancellationToken = default) => RunTargetAsync("default");


    public Task StopAsync(CancellationToken cancellationToken = default) => System.Threading.Tasks.Task.CompletedTask;

    public override async Task<CakeReport> RunTargetAsync(string target)
    {
        Settings.SetTarget(target);

        var report = await Engine.RunTargetAsync(Context, _executionStrategy, Settings).ConfigureAwait(false);
        if (report != null && !report.IsEmpty)
        {
            _reportPrinter.Write(report);
        }

        return report;
    }
}
