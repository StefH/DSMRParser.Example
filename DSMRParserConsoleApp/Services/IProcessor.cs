namespace DSMRParserConsoleApp.Services;

internal interface IProcessor
{
    void Run(CancellationToken cancellationToken);
}