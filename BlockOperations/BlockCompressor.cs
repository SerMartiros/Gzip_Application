using Gzip_Application.Interfaces;
using System;
using System.IO;
using System.IO.Compression;

namespace Gzip_Application.Fundametals
{
    public class BlockCompressor:IDe_Compressible
    {
        private byte[] _block;
        private Object _locker = new Object();
        public BlockCompressor(byte[] block = null)
        {
            _block = block;
        }
        public byte[] Execute()
        {
            lock (_locker)
            {
                byte[] holder = new byte[Helper.blockSize];
                try
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (GZipStream gzs = new GZipStream(ms, CompressionMode.Compress))
                        {
                            gzs.Write(_block, 0, _block.Length);
                        }
                        holder = ms.ToArray();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("Compress block operation error");
                }
                return holder;
            }
        }
    }
}
