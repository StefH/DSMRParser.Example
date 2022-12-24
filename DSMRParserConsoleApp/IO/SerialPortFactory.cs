using System.IO.Ports;

namespace DSMRParserConsoleApp.IO;

internal class SerialPortFactory : ISerialPortFactory
{
    private const int BaudRate = 115200;

    public ISerialPort CreateUsingFirstAvailableSerialPort()
    {
        var port = SerialPort.GetPortNames().FirstOrDefault();
        if (port is null)
        {
            throw new NotSupportedException("No Serial Ports are found.");
        }

        return new SerialPortProxy(new SerialPort(port, BaudRate));
    }
}