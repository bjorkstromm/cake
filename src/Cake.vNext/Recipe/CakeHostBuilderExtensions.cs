using Cake.Hosting;

namespace Cake.Recipe;

public static class CakeHostBuilderExtensions
{
    public static CakeHostBuilder UseCakeRecipe(this CakeHostBuilder builder)
    {
        // Cake Recipe can install any tools needed here, or configure the builder in any way.
        return builder;
    }
}
