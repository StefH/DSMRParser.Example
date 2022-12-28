using System.Threading.Channels;
using DSMRParser.Models;
using Microsoft.Extensions.Logging;
using Stef.Validation;

namespace DSMRParserConsoleApp.Services;

internal class TelegramParser : ITelegramParser
{
    private readonly ILogger<TelegramParser> _logger;
    private readonly IDSMRTelegramParserProxy _parserProxy;
    private readonly ChannelReader<string> _reader;

    public TelegramParser(ILogger<TelegramParser> logger, ChannelReader<string> reader, IDSMRTelegramParserProxy parserProxy)
    {
        _logger = Guard.NotNull(logger);
        _reader = Guard.NotNull(reader);
        _parserProxy = Guard.NotNull(parserProxy);
    }

    public async Task StartProcessingAsync(CancellationToken cancellationToken)
    {
        await foreach (var message in _reader.ReadAllAsync(cancellationToken))
        {
            _logger.LogInformation("{name} Thread = {ManagedThreadId}", nameof(TelegramParser), Thread.CurrentThread.ManagedThreadId);

            if (_parserProxy.TryParse(message, out var tel))
            {
                _logger.LogInformation("PowerDelivered = {power}", tel.PowerDelivered);
            }
            else
            {
                _logger.LogDebug("Unable to parse '{message}'.", message);
                _logger.LogWarning("Unable to parse the {Telegram}.", nameof(Telegram));
            }
        }
    }
}