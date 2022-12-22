// https://dotnetcoretutorials.com/2020/11/24/using-channels-in-net-core-part-2-advanced-channels/

using System.Threading.Channels;
using AsyncAwaitBestPractices;
using DSMRParserConsoleApp.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace DSMRParserConsoleApp;

public static class Program
{
    private static readonly CancellationTokenSource CancellationTokenSource = new();

    static void Main(string[] args)
    {
        var cancellationToken = CancellationTokenSource.Token;

        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        Log.Logger.Information("Application Starting");

        var host = CreateHostBuilder(args).Build();

        var reader = host.Services.GetRequiredService<IP1Reader>();
        var parser = host.Services.GetRequiredService<ITelegramParser>();

        reader.StartReadingAsync(cancellationToken).SafeFireAndForget(e =>
        {
            Log.Logger.Fatal(e, "Unable to read.");
            throw new Exception("Unable to read.", e);
        });

        parser.StartProcessingAsync(cancellationToken).SafeFireAndForget(e =>
        {
            Log.Logger.Fatal(e, "Unable to parse.");
            throw new Exception("Unable to parse.", e);
        });

        Log.Logger.Error("Press any key to cancel");
        Console.ReadKey();

        CancellationTokenSource.Cancel();

        Log.Logger.Error("Ending and waiting 1 second to finalize...");

        Thread.Sleep(1000);
    }

    static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args).ConfigureServices((_, services) =>
            {
                var channel = Channel.CreateUnbounded<string>();

                services.AddSingleton(channel.Reader);
                services.AddSingleton(channel.Writer);
                services.AddSingleton<IP1Reader, P1Reader>();
                services.AddSingleton<ITelegramParser, TelegramParser>();
            })
            .UseSerilog()
    ;
}