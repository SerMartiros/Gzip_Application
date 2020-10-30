using System;
using System.IO;
using System.IO.Compression;

namespace Gzip_Application
{
    class BlockOperation
    {
        private byte[] _block;
        private Object _locker = new Object();
        public BlockOperation(byte[] block = null)
        {
            _block = block;
        }
        public byte[] FetchBlock(int index, BinaryReader breader)
        {
            lock (_locker)
            {
                byte[] buffer = new byte[1024 * 1024];
                long offset = 0;
                if (index == 0)
                {
                    offset = 0;
                }
                else
                {
                    offset = buffer.Length * index;
                }
                using (MemoryStream ms = new MemoryStream())
                {
                    breader.BaseStream.Seek(offset, SeekOrigin.Begin);
                    int readcount = breader.Read(buffer, 0, buffer.Length);
                    ms.Write(buffer, 0, readcount);
                    return ms.ToArray();
                }
            }
        }

        public byte[] CompressBlock()
        {
            lock (_locker)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (GZipStream gzs = new GZipStream(ms, CompressionMode.Compress))
                    {
                        gzs.Write(_block, 0, _block.Length);
                    }
                    return ms.ToArray();
                }
            }
        }

        public byte[] DecompressBlock()
        {
            lock (_locker)
            {
                using (var output = new MemoryStream())
                {
                    using (var input = new MemoryStream(_block))
                    {
                        using (var decompressStream = new GZipStream(input, CompressionMode.Decompress))
                        {
                            decompressStream.Copy(output);
                        }
                        return output.ToArray();
                    }
                }
            }
        }

        public void WriteBlock(FileStream toStream, long offset)
        {
            lock (_locker)
            {
                toStream.Seek(offset, SeekOrigin.Begin);
                toStream.Write(_block, 0, _block.Length);
            }
        }
    }
}
