using Gzip_Application.Fundametals;
using Gzip_Application.Interfaces;
using System;
using System.IO;
using System.Threading;

namespace Gzip_Application
{
    public class Compression:Archivation
    {
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

        public override void CompressFile()
        {
            SplitTasks();
            base.CompressionTasks();
            _offsets_calc.CalculateOffsets(_blocksCompressed_array);
            WriteTasks();
        }

        public override void SplitTasks()
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
        public override void WriteTasks()
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

        public override void Block_Write_ToStream(FileStream toStream, long off, int index)
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
