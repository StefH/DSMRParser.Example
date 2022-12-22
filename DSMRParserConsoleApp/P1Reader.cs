using System.IO.Ports;
using System.Text;
using System.Threading.Channels;
using DSMRParserConsoleApp.Interfaces;
using Microsoft.Extensions.Logging;
using Stef.Validation;

namespace DSMRParserConsoleApp;

internal class P1Reader : IP1Reader
{
    private const int BaudRate = 115200;
    private const string NewLine = "\n";

    private readonly ChannelWriter<string> _writer;
    private readonly ILogger<P1Reader> _logger;
    private readonly StringBuilder _stringBuilder;

    public P1Reader(ILogger<P1Reader> logger, ChannelWriter<string> writer)
    {
        _logger = Guard.NotNull(logger);
        _writer = Guard.NotNull(writer);

        _stringBuilder = new(1024);
    }

    public async Task StartReadingAsync(CancellationToken cancellationToken = default)
    {
        var port = SerialPort.GetPortNames().FirstOrDefault();
        if (port is null)
        {
            throw new NotSupportedException("No Serial Ports are found.");
        }
        
        var serialPort = new SerialPort(port, BaudRate);

        await Task.Run(async () =>
        {
            await Task.Delay(500, cancellationToken); // Just wait some time

            _logger.LogInformation("{name} Thread = {ManagedThreadId} : Opening Stream on {port} @ {baudrate}.", nameof(P1Reader), Thread.CurrentThread.ManagedThreadId, port, serialPort.BaudRate);

            serialPort.Open();

            _logger.LogInformation("{name} Thread = {ManagedThreadId} : Reading Lines", nameof(P1Reader), Thread.CurrentThread.ManagedThreadId);

            while (!cancellationToken.IsCancellationRequested && serialPort.ReadLine() is { } line)
            {
                _logger.LogDebug(line);

                if (line.StartsWith('/'))
                {
                    _stringBuilder.Clear();
                }

                _stringBuilder.Append(line);
                _stringBuilder.Append(NewLine);

                if (line.StartsWith('!'))
                {
                    await _writer.WriteAsync(_stringBuilder.ToString(), cancellationToken);

                    _stringBuilder.Clear();
                }
            }

            _logger.LogInformation("{name} Thread = {ManagedThreadId} : CancellationRequested, completing channel and closing stream.", nameof(P1Reader), Thread.CurrentThread.ManagedThreadId);

            _writer.Complete();

            serialPort.Close();
            serialPort.Dispose();

        }, cancellationToken);
    }
}