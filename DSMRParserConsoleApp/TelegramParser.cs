using System.Threading.Channels;
using DSMRParser;
using DSMRParserConsoleApp.Interfaces;
using Microsoft.Extensions.Logging;
using Stef.Validation;

namespace DSMRParserConsoleApp;

internal class TelegramParser : ITelegramParser
{
    private readonly ILogger<TelegramParser> _logger;
    private readonly Interfaces.IDSMRTelegramParser _parser;
    private readonly ChannelReader<string> _reader;

    public TelegramParser(ILogger<TelegramParser> logger, ChannelReader<string> reader)
    {
        _logger = Guard.NotNull(logger);
        _reader =  Guard.NotNull(reader);

        _parser = new DSMRTelegramParserProxy(new DSMRTelegramParser());
    }

    public async Task StartProcessingAsync(CancellationToken cancellationToken)
    {
        await foreach (var message in _reader.ReadAllAsync(cancellationToken))
        {
            _logger.LogInformation("{name} Thread = {ManagedThreadId}", nameof(TelegramParser), Thread.CurrentThread.ManagedThreadId);
            
            if (_parser.TryParse(message, out var tel))
            {
                _logger.LogInformation("PowerDelivered = {power}", tel.PowerDelivered);
            }
            else
            {
                _logger.LogError("Invalid");
            }
        }
    }
}