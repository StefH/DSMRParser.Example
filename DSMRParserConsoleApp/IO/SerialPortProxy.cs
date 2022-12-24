namespace DSMRParserConsoleApp.IO;

public partial class SerialPortProxy
{
    public void Dispose()
    {
        _Instance.Dispose();
    }
}