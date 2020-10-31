using System.IO;

namespace Gzip_Application.Interfaces
{
    interface IReadable
    {
        byte[] ReadBlock(int index, BinaryReader breader);
    }
}
