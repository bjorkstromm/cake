using Cake.Core.Configuration;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Cake.Core.Tooling;

namespace Cake.Hosting;

public class CakeHostBuilder : HostBuilder
{
    public new CakeHost Build()
    {
        var host = base.Build();

        var fileSystem = new FileSystem();
        var data = new CakeDataService();
        var environment = new CakeEnvironment(
            new CakePlatform(),
            new CakeRuntime()
            );
        var console = new CakeConsole(environment);
        var log = new CakeBuildLog(console);
        var globber = new Globber(fileSystem, environment);
        var arguments = new CakeArguments(ArgumentParser.GetParsedCommandLine());
        var configuration = new CakeConfiguration(new Dictionary<string, string>());
        var tools = new ToolLocator(
                    environment,
                    new ToolRepository(environment),
                    new ToolResolutionStrategy(
                        fileSystem,
                        environment,
                        globber,
                        configuration,
                        log
                        )
                    );
        var context = new CakeContext(
                fileSystem,
                environment,
                globber,
                log,
                arguments,
                new ProcessRunner(fileSystem, environment, log, tools, configuration),
                new WindowsRegistry(),
                tools,
                data,
                configuration
            );

        var engine = new CakeEngine(data, log);
        var executionStrategy = new DefaultExecutionStrategy(log);
        var reportPrinter = new CakeReportPrinter(console, context);

        return new CakeHost(
            engine,
            context,
            executionStrategy,
            reportPrinter,
            host);
    }

    internal CakeHostBuilder UseTool(string expression)
    {
        // TODO: Implement
        return this;
    }

    internal class ArgumentParser
    {
        public static ILookup<string, string> GetParsedCommandLine()
            => ParseCommandLine()
                .ToLookup(
                    key => key.Key,
                    value => value.Value,
                    StringComparer.OrdinalIgnoreCase
                );

        private static IEnumerable<KeyValuePair<string, string>> ParseCommandLine()
        {
            // Naive PoC  :)
            var args = Environment.GetCommandLineArgs();
            for (int index = 0, peek = 1; index < args.Length; index++, peek++)
            {
                var arg = args[index];
                if (arg.FirstOrDefault() != '-')
                    continue;

                var key = string.Concat(arg.SkipWhile(c => c == '-').TakeWhile(c => c != '='));
                var value = string.Concat(arg.SkipWhile(c => c != '=').Skip(1));
                if (string.IsNullOrEmpty(value) && (peek < args.Length) && args[peek].FirstOrDefault() != '-')
                {
                    index = peek;
                    value = args[peek];
                }

                yield return new KeyValuePair<string, string>(key, value.Trim('"'));
            }
        }
    }
}
