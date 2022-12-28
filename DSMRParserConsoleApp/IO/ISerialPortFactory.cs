namespace DSMRParserConsoleApp.IO;

internal interface ISerialPortFactory
{
    ISerialPort CreateUsingFirstAvailableUSBSerialPort();
}