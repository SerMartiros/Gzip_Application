using Gzip_Application.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gzip_Application.Fundametals
{
    public class BlockReader:IReadable
    {
        private byte[] _block;
        private Object _locker = new Object();
        public BlockReader(byte[] block = null)
        {
            _block = block;
        }
        public byte[] Execute(int index, BinaryReader breader)
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
    }
}
