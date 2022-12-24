using DSMRParserConsoleApp.Interfaces;

namespace DSMRParserConsoleApp.IO;

internal interface ISerialPortFactory
{
    ISerialPort CreateUsingFirstAvailableSerialPort();
}