using System.Text;
using System.Threading.Channels;
using DSMRParserConsoleApp.Interfaces;
using Microsoft.Extensions.Logging;
using RJCP.IO.Ports;
using Stef.Validation;

namespace DSMRParserConsoleApp;

internal class P1Reader : IP1Reader
{
    private const string CRLF = "\r\n";

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
        var port = SerialPortStream.GetPortNames().First();
        await using var serialPortStream = new SerialPortStream(port, 115200);
        using var streamReader = new StreamReader(serialPortStream);

        await Task.Run(async () =>
        {
            await Task.Delay(500, cancellationToken); // Just wait some time

            _logger.LogInformation("{name} Thread = {ManagedThreadId} : Opening Stream on {port} @ {baudrate}", nameof(P1Reader), Thread.CurrentThread.ManagedThreadId, port, serialPortStream.BaudRate);

            serialPortStream.Open();

            _logger.LogInformation("{name} Thread = {ManagedThreadId} : Reading Lines", nameof(P1Reader), Thread.CurrentThread.ManagedThreadId);

            while (!cancellationToken.IsCancellationRequested && await streamReader.ReadLineAsync(cancellationToken) is { } line)
            {
                _logger.LogDebug(line);

                if (line.StartsWith('/'))
                {
                    _stringBuilder.Clear();
                }

                _stringBuilder.Append(line);
                _stringBuilder.Append(CRLF);

                if (line.StartsWith('!'))
                {
                    await _writer.WriteAsync(_stringBuilder.ToString(), cancellationToken);

                    _stringBuilder.Clear();
                }
            }

            _logger.LogInformation("{name} Thread = {ManagedThreadId} : CancellationRequested, completing channel and closing stream", nameof(P1Reader), Thread.CurrentThread.ManagedThreadId);

            _writer.Complete();

            serialPortStream.Close();
            await serialPortStream.DisposeAsync();
        }, cancellationToken);
    }
}