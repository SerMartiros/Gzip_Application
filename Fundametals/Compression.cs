using Gzip_Application.Interfaces;
using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;

namespace Gzip_Application
{
    class Compression
    {
        private string _inputFile;
        private string _outputFile;
        private int _iterations;
        private long _fileLength;

        private byte[][] _blocksToCompress_array;
        private byte[][] _blocksCompressed_array;

        private OffsetsCalculator _offsets_calc;
        private IterationsCalculator _iterations_calc;

        public Compression(string inputFile, string outputFile)
        {
            _inputFile = inputFile;
            _outputFile = outputFile;
            _iterations_calc = new IterationsCalculator(new FileInfo(_inputFile).Length, Helper.blockSize);
            _iterations = _iterations_calc.IterationCount();
            _blocksToCompress_array = new byte[_iterations][];
            _blocksCompressed_array = new byte[_iterations][];
            _offsets_calc = new OffsetsCalculator(_iterations);
            _fileLength = new FileInfo(inputFile).Length;
        }

        public void CompressFile()
        {
            SplitTasks();
            CompressionTasks();
            _offsets_calc.CalculateOffsets(_blocksCompressed_array);
            WriteTasks();
        }

        private void SplitTasks()
        {
            try
            {
                using (BinaryReader breader = new BinaryReader(File.Open(_inputFile, FileMode.Open, FileAccess.Read, FileShare.Read)))
                {
                    for (int i = 0; i < _iterations; i++)
                    {
                        IFetchable bo = new BlockOperation();
                        byte[] bt = bo.FetchBlock(i, breader);
                        _blocksToCompress_array[i] = bt;
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("File split into blocks error");
            }
        }

        private void CompressionTasks()
        {
            try
            {
                for (int i = 0; i < _blocksToCompress_array.Length; i++)
                {
                    Helper.semaf.WaitOne();
                    int j = i;
                    Thread compress = new Thread(() => Block_Compress_ToArray(j));
                    { }
                    compress.Start();
                }
                while (Helper.compressCount < _iterations)
                {
                    if (Helper.compressCount >= _iterations)
                    {
                        Helper.compressEvent.Set();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Compression blocks error");
            }
        }
        private void WriteTasks()
        {
            try
            {
                using (FileStream toStream = new FileStream(_outputFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    for (int i = 0; i < _iterations; i++)
                    {
                        Helper.semaf.WaitOne();
                        int j = i;
                        Thread write = new Thread(() => Block_Write_ToStream(toStream, _offsets_calc.Offsets[j], j));
                        { }
                        write.Start();
                    }
                    Helper.writeEvent.WaitOne();
                    Console.WriteLine("Compression Finished Successfully");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Write blocks error");
            }
        }

        private void Block_Compress_ToArray(int index)
        {
            ICompressable bo = new BlockOperation(_blocksToCompress_array[index]);
            byte[] byte_arr = bo.CompressBlock();
            _blocksCompressed_array[index] = byte_arr;
            Helper.compressCount++;
            Helper.semaf.Release();
            Helper.compressEvent.WaitOne();
        }
        private void Block_Write_ToStream(FileStream toStream, long off, int index)
        {
            Helper.rw_lock_slim.EnterWriteLock();
            IWritable bo = new BlockOperation(_blocksCompressed_array[index]);
            bo.WriteBlock(toStream, off);
            Helper.writeCount++;
            if (Helper.writeCount >= _iterations)
            {
                Helper.writeEvent.Set();
            }
            Helper.semaf.Release();
            Helper.rw_lock_slim.ExitWriteLock();
        }
    }
}
