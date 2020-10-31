using System.IO;

namespace Gzip_Application.Interfaces
{
    interface IFetchable
    {
        byte[] FetchBlock(int index, BinaryReader breader);
    }
}
