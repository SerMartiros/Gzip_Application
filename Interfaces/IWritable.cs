using System.IO;

namespace Gzip_Application.Interfaces
{
    public interface IWritable
    {
        void Execute(FileStream toStream, long offset);
    }
}
