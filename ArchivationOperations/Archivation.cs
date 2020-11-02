using Gzip_Application.Helpers;
using Gzip_Application.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Gzip_Application.Fundametals
{
    public abstract class Archivation:IArchivable
    {
        protected string _inputFile;
        protected string _outputFile;
        protected int _iterations;
        protected long _fileLength;
        protected byte[][] _blocks_toCompress_array;
        protected OffsetsCalculator _offsets_calc;
        protected IterationsCalculator _iterations_calc;
        protected List<byte[]> _blocks_toDecompress_array = new List<byte[]>();
        protected byte[][] _blocks_processed_array;
        protected byte[] gzipHeader = Helper.gzipHeader;

        public virtual void CompressFile()
        {
            Console.WriteLine("Base CompressFile");
        }
        public virtual void DecompressFile()
        {
            Console.WriteLine("Base DecompressFile");
        }
        public abstract void ReadTasks();

        public virtual void WriteTasks(byte[][] byteArr)
        {
            try
            {
                using (FileStream toStream = new FileStream(_outputFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    for (int i = 0; i < byteArr.Length; i++)
                    {
                        Helper.semaf.WaitOne();
                        int j = i;
                        Thread write = new Thread(() => Block_Write_ToStream(toStream, _offsets_calc.Offsets[j], byteArr[j], byteArr.Length));
                        { }
                        write.Start();
                    }
                    Helper.writeLock.WaitOne();
                    Console.WriteLine("Operation Finished Successfully");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Write blocks error");
            }
        }
        public virtual void Block_Read_ToArray(BinaryReader breader, int index)
        {
            try
            {
                IReadable bo = new BlockReader();
                byte[] bt = bo.Execute(index, breader);
                _blocks_toCompress_array[index] = bt;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Read block error");
            }
        }
        public virtual void Block_Write_ToStream(FileStream toStream, long offset, byte[] block, int count)
        {
            try
            {
                Helper.rw_lock_slim.EnterWriteLock();
                IWritable bo = new BlockWriter(block);
                bo.Execute(toStream, offset);
                Helper.semaf.Release();
                Helper.rw_lock_slim.ExitWriteLock();
                Helper.writeCount++;
                if (Helper.writeCount >= count)
                {
                    Helper.writeLock.Set();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Block write to stream error");
            }
        }

        public virtual void ArchivationTasks(byte[][] original, OperationType operation)
        {
            try
            {
                switch (operation)
                {
                    case OperationType.Compress:
                        for (int i = 0; i < original.Length; i++)
                        {
                            Helper.semaf.WaitOne();
                            int j = i;
                            Thread compress = new Thread(() => Block_Process_ToArray(j, original, new BlockCompressor(original[j])));
                            { }
                            compress.Start();
                        }
                        break;
                    case OperationType.Decompress:
                        for (int i = 0; i < original.Length; i++)
                        {
                            Helper.semaf.WaitOne();
                            int j = i;
                            Thread decompress = new Thread(() => Block_Process_ToArray(j, original, new BlockDecompressor(original[j])));
                            { }
                            decompress.Start();
                        }
                        break;
                }
                while (Helper.archivationCount < original.Length)
                {
                    if (Helper.archivationCount >= original.Length)
                    {
                        Helper.archivationLock.Set();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Archivation blocks error");
            }
        }
        public void Block_Process_ToArray(int index, byte[][] original, IDe_Compressible task)
        {
            try
            {
                byte[] byte_arr = task.Execute();
                _blocks_processed_array[index] = byte_arr;
                Helper.semaf.Release();
                Helper.archivationCount++;
                Helper.archivationLock.WaitOne();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Block Compression/Decompression task error");
            }
        }
    }
}