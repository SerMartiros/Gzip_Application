using Gzip_Application.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gzip_Application.Fundametals
{
    public class BlockDecompressor:IDe_Compressible
    {
        private byte[] _block;
        private Object _locker = new Object();
        public BlockDecompressor(byte[] block = null)
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
                    using (var output = new MemoryStream())
                    {
                        using (var input = new MemoryStream(_block))
                        {
                            using (var decompressStream = new GZipStream(input, CompressionMode.Decompress))
                            {
                                decompressStream.Copy(output);
                            }
                            holder = output.ToArray();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("Decompress block operation error");
                }
                return holder;
            }
        }
    }
}
