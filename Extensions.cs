using System.IO;

namespace Gzip_Application
{
    public static class Extensions
    {
        public static void Copy(this Stream input, Stream output, int bufferSize = Helper.blockSize)
        {
            var buffer = new byte[bufferSize];
            int readCount;
            while ((readCount = input.Read(buffer, 0, buffer.Length)) > 0)
                output.Write(buffer, 0, readCount);
        }
    }
}
