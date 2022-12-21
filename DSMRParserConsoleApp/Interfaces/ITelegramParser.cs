namespace DSMRParserConsoleApp.Interfaces;

internal interface ITelegramParser
{
    Task StartProcessingAsync(CancellationToken cancellationToken);
}