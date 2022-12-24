namespace DSMRParserConsoleApp;

internal interface IProcessor
{
    void Run(CancellationToken cancellationToken);
}