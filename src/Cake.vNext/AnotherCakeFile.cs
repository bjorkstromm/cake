using Cake.Core;
using Cake.Hosting;
using Cake.Common.Diagnostics;

namespace Cake;

internal class AnotherCakeFile
{
    public static void RegisterTasks(CakeHost host)
    {
        // Cake recipe can add whatever tasks here.
        host.Task("From-Another-File")
            .IsDependeeOf("Build")
            .Does(ctx => ctx.Information("I'm running before build from another cake file..."));
    }
}
