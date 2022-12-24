// https://dotnetcoretutorials.com/2020/11/24/using-channels-in-net-core-part-2-advanced-channels/

using System.Threading.Channels;
using DSMRParser;
using DSMRParserConsoleApp.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace DSMRParserConsoleApp;

public static class Program
{
    private static readonly CancellationTokenSource CancellationTokenSource = new();

    private static void Main(string[] args)
    {
        var cancellationToken = CancellationTokenSource.Token;

        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        Log.Logger.Information("Application Starting");

        var host = CreateHostBuilder(args).Build();

        var processor = host.Services.GetRequiredService<IProcessor>();
        try
        {
            processor.Run(cancellationToken);
        }
        catch
        {
            Cancel();
            return;
        }

        Log.Logger.Error("Press any key to cancel");
        Console.ReadKey();

        Cancel();
    }

    private static void Cancel()
    {
        CancellationTokenSource.Cancel();

        Log.Logger.Error("Stopping application and waiting 1 second to finalize...");

        Thread.Sleep(1000);
    }

    static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args).ConfigureServices((_, services) =>
            {
                var channel = Channel.CreateUnbounded<string>();

                services.AddSingleton(channel.Reader);
                services.AddSingleton(channel.Writer);

                services.AddSingleton<IDSMRTelegramParserProxy>(new DSMRTelegramParserProxy(new DSMRTelegramParser()));
                services.AddSingleton<IP1Reader, P1Reader>();
                services.AddSingleton<ITelegramParser, TelegramParser>();
                services.AddSingleton<IProcessor, Processor>();
            })
            .UseSerilog()
    ;
}