using Cake.Core;
using Cake.Hosting;
using Cake.Common.Diagnostics;

namespace Cake.Recipe;

public static class CakeHostExtensions
{
    public static CakeHost AddCakeRecipe(this CakeHost host)
    {
        // Cake recipe can add whatever tasks here.
        host.Task("From-Recipe")
            .IsDependeeOf("Clean")
            .Does(ctx => ctx.Information("I'm running before clean from recipe..."));

        return host;
    }
}
