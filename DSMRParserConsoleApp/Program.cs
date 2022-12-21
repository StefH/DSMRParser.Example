// https://dotnetcoretutorials.com/2020/11/24/using-channels-in-net-core-part-2-advanced-channels/

using System.Threading.Channels;
using DSMRParserConsoleApp;
using DSMRParserConsoleApp.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

var cancellationTokenSource = new CancellationTokenSource();
var cancellationToken = cancellationTokenSource.Token;

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


Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

Log.Logger.Information("Application Starting");

var host = CreateHostBuilder(args).Build();

var reader = host.Services.GetRequiredService<IP1Reader>();
var parser = host.Services.GetRequiredService<ITelegramParser>();

#pragma warning disable CS4014
reader.StartReadingAsync(cancellationToken);

parser.StartProcessingAsync(cancellationToken);
#pragma warning restore CS4014

Log.Logger.Error("Press any key to cancel");
Console.ReadKey();

cancellationTokenSource.Cancel();

Log.Logger.Error("Ending and waiting 1 second to finalize...");

await Task.Delay(1000);