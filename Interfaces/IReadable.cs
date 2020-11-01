using System.IO;

namespace Gzip_Application.Interfaces
{
    public interface IReadable
    {
        byte[] Execute(int index, BinaryReader breader);
    }
}
