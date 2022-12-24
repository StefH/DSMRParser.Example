using System.Text;
using System.Threading.Channels;
using DSMRParserConsoleApp.IO;
using Microsoft.Extensions.Logging;
using Stef.Validation;

namespace DSMRParserConsoleApp.Services;

internal class P1Reader : IP1Reader
{
    private readonly ILogger<P1Reader> _logger;
    private readonly ISerialPortFactory _factory;
    private readonly ChannelWriter<string> _writer;

    private readonly StringBuilder _stringBuilder;

    public P1Reader(ILogger<P1Reader> logger, ISerialPortFactory factory, ChannelWriter<string> writer)
    {
        _logger = Guard.NotNull(logger);
        _factory = Guard.NotNull(factory);
        _writer = Guard.NotNull(writer);

        _stringBuilder = new(1024);
    }

    public async Task StartReadingAsync(CancellationToken cancellationToken = default)
    {
        var serialPort = _factory.CreateUsingFirstAvailableSerialPort();

        await Task.Run(async () =>
        {
            await Task.Delay(500, cancellationToken); // Just wait some time

            _logger.LogInformation("{name} Thread = {ManagedThreadId} : Opening Stream on {port} @ {baudrate}.", nameof(P1Reader), Thread.CurrentThread.ManagedThreadId, serialPort.PortName, serialPort.BaudRate);

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
                _stringBuilder.Append('\n');

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