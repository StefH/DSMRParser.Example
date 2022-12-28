using AsyncAwaitBestPractices;
using Serilog;
using Stef.Validation;

namespace DSMRParserConsoleApp.Services;

internal class Processor : IProcessor
{
    private readonly IP1Reader _reader;
    private readonly ITelegramParser _parser;

    public Processor(IP1Reader reader, ITelegramParser parser)
    {
        _reader = Guard.NotNull(reader);
        _parser = Guard.NotNull(parser);
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