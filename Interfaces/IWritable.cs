using System.IO;

namespace Gzip_Application.Interfaces
{
    interface IWritable
    {
        void WriteBlock(FileStream toStream, long offset);
    }
}
