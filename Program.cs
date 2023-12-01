global using Microsoft.Extensions.Configuration;
global using Microsoft.SemanticKernel;
global using System.ComponentModel;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.Planners;
using Microsoft.SemanticKernel.Planning;

namespace SemanticKernelConsole;

internal class Program
{
    public static IKernel SemanticKernel { get; private set; }

    static async Task Main(string[] args)
    {
        IConfiguration cfg = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", true, true)
            .Build();

        string Key = cfg[nameof(Key)];
        string Endpoint = cfg[nameof(Endpoint)];
        string Model = cfg[nameof(Model)];

        SemanticKernel = Kernel.Builder
        .WithAzureOpenAIChatCompletionService(Model, Endpoint, Key)
        .Build();

        SemanticKernel.ImportFunctions(new MathPlugin(), nameof(MathPlugin));
        SemanticKernel.ImportFunctions(new WeatherPlugin(), nameof(WeatherPlugin));

        Console.WriteLine(await ExecutePlannerAsync("If my investment of 2130.23 dollars increased by 23%, how much would I have after I spent $5 on a latte?"));
        Console.WriteLine(await ExecutePlannerAsync("\nHow is weather in Copenhagen?"));
        Console.WriteLine(await ExecutePlannerAsync("\nHow is weather in Riyadh?"));
    }

    private static async Task<string> ExecutePlannerAsync(string ask)
    {
        Plan plan = await new SequentialPlanner(SemanticKernel).CreatePlanAsync(ask);
        KernelResult result = await SemanticKernel.RunAsync(plan);
        return result.GetValue<string>()!.Trim();
    }
}
