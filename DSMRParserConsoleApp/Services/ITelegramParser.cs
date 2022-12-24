namespace DSMRParserConsoleApp.Services;

internal interface ITelegramParser
{
    Task StartProcessingAsync(CancellationToken cancellationToken);
}