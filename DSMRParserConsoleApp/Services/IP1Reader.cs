namespace DSMRParserConsoleApp.Services;

internal interface IP1Reader
{
    Task StartReadingAsync(CancellationToken cancellationToken);
}