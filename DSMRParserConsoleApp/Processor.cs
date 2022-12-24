using AsyncAwaitBestPractices;
using DSMRParserConsoleApp.Interfaces;
using Serilog;

namespace DSMRParserConsoleApp;

internal class Processor : IProcessor
{
    private readonly IP1Reader _reader;
    private readonly ITelegramParser _parser;

    public Processor(IP1Reader reader, ITelegramParser parser)
    {
        _reader = reader;
        _parser = parser;
    }

    public void Run(CancellationToken cancellationToken)
    {
        _reader.StartReadingAsync(cancellationToken).SafeFireAndForget(e =>
        {
            Log.Logger.Fatal(e, "Unable to read.");
            throw new Exception("Unable to read.", e);
        });

        _parser.StartProcessingAsync(cancellationToken).SafeFireAndForget(e =>
        {
            Log.Logger.Fatal(e, "Unable to parse.");
            throw new Exception("Unable to parse.", e);
        });
    }
}