using Gzip_Application.Interfaces;
using System;
using System.IO;
using System.IO.Compression;
using System.Web;

namespace Gzip_Application
{
    class BlockOperation: ICompressable, IDecompressable, IWritable, IFetchable
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
                byte[] holder = new byte[Helper.blockSize];
                try
                {
                    byte[] buffer = new byte[Helper.blockSize];
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
                        holder = ms.ToArray();
                        
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("Fetch block operation error");
                }
                return holder;
            }
        }

        public byte[] CompressBlock()
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

        public byte[] DecompressBlock()
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

        public void WriteBlock(FileStream toStream, long offset)
        {
            lock (_locker)
            {
                try
                {
                    toStream.Seek(offset, SeekOrigin.Begin);
                    toStream.Write(_block, 0, _block.Length);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("Write block operation error");
                }
            }
        }
    }
}
