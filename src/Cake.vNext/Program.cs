// Create the Cake host
CakeHost cake = CakeHost
    .CreateDefaultBuilder(args)
    .UseTool("nuget:?package=xunit.runner.console&version=2.4.1")
    .UseCakeRecipe()
    .Build();

// Recipe (as extension method), just a convention decided by the Recipe itself.
// Cake does not know about recipes.
cake.AddCakeRecipe();

// Another file, could be extension method also.
// Cake does not know about other files, users just need to make sure to register.
// It's easy to just create a static method and pass the CakeHost there.
AnotherCakeFile.RegisterTasks(cake);

// Everything below here is just using Example Cake file. 
// To port existing Cake file, just make sure to use host and context.
var target = cake.Context.Argument("target", "Test");
var configuration = cake.Context.Argument("configuration", "Release");

cake.Task("Clean")
    .WithCriteria(context => context.HasArgument("rebuild"))
    .Does(context =>
    {
        context.CleanDirectory($"./src/Example/bin/{configuration}");
    });

cake.Task("Build")
    .IsDependentOn("Clean")
    .Does(context =>
    {
        context.DotNetBuild("./src/Example.sln", new DotNetBuildSettings
        {
            Configuration = configuration,
        });
    });

cake.Task("Test")
    .IsDependentOn("Build")
    .Does(context =>
    {
        context.DotNetTest("./src/Example.sln", new DotNetTestSettings
        {
            Configuration = configuration,
            NoBuild = true,
        });
    });

cake.Task("Default")
    .IsDependentOn("Test");

await cake.RunAsync();
