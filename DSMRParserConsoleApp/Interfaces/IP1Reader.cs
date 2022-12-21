namespace DSMRParserConsoleApp.Interfaces;

internal interface IP1Reader
{
    Task StartReadingAsync(CancellationToken cancellationToken);
}