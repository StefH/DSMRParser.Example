using System.IO.Ports;
using System.Runtime.InteropServices;

namespace DSMRParserConsoleApp.IO;

internal class SerialPortFactory : ISerialPortFactory
{
    private const int BaudRate = 115200;

    private readonly Func<string, bool> _portPredicate = port => RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? port.Contains("USB") : port.Contains("COM");

    public ISerialPort CreateUsingFirstAvailableUSBSerialPort()
    {
        var port = SerialPort.GetPortNames().FirstOrDefault(_portPredicate);
        if (port is null)
        {
            throw new NotSupportedException("No Serial USB Ports are found.");
        }

        return new SerialPortProxy(new SerialPort(port, BaudRate));
    }
}