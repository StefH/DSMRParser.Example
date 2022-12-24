using System.IO.Ports;

namespace DSMRParserConsoleApp.IO;

[ProxyInterfaceGenerator.Proxy(typeof(SerialPort))]
public partial interface ISerialPort : IDisposable
{
}