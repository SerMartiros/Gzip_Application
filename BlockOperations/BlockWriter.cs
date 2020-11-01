using Gzip_Application.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gzip_Application.Fundametals
{
    public class BlockWriter:IWritable
    {
        private byte[] _block;
        private Object _locker = new Object();
        public BlockWriter(byte[] block = null)
        {
            _block = block;
        }
        public void Execute(FileStream toStream, long offset)
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
