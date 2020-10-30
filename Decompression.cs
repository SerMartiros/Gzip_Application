using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Gzip_Application
{
    class Decompression
    {
        private string _inputFile;
        private string _outputFile;
        private int _iterations;
        private long _fileLength;
        private byte[][] _blocksToDecompress_array;
        private byte[][] _blocksDecompressed_array;


        private byte[] gzipHeader = Helper.gzipHeader;
        private IterationsCalculator _iterations_calc;
        private OffsetsCalculation _offsets_calc;

        public Decompression(string inputFile, string outputFile)
        {
            _inputFile = inputFile;
            _outputFile = outputFile;
            _fileLength = new FileInfo(inputFile).Length;
            _iterations_calc = new IterationsCalculator(new FileInfo(_inputFile).Length, Helper.blockSize);
            _iterations = _iterations_calc.IterationCount();
            _blocksToDecompress_array = new byte[_iterations+1][];
            _blocksDecompressed_array = new byte[_iterations+1][];
            _offsets_calc = new OffsetsCalculation(_iterations+1);
        }
        public void DecompressFile()
        {
            SplitTasks();
            DecompressionTasks();
            _offsets_calc.CalculateOffsets(_blocksDecompressed_array);
            WriteTasks();
        }


        private void SplitTasks()
        {
            try
            {
                long remainingBytes = _fileLength;
                int readCycle = 0;

                using (BinaryReader bReader = new BinaryReader(File.Open(_inputFile, FileMode.Open, FileAccess.Read, FileShare.Read)))
                {
                    while (remainingBytes > 0)
                    {
                        List<byte> block = new List<byte>(Helper.blockSize);

                        if (readCycle == 0)
                        {
                            gzipHeader = bReader.ReadBytes(gzipHeader.Length);
                            remainingBytes -= gzipHeader.Length;
                        }
                        block.AddRange(gzipHeader);

                        int foundCount = 0;
                        while (remainingBytes > 0)
                        {
                            var currentByte = bReader.ReadByte();
                            block.Add(currentByte);
                            remainingBytes--;
                            if (remainingBytes <= 0)
                            {
                                _blocksToDecompress_array[readCycle] = block.ToArray();
                                break;
                            }
                            if (currentByte == gzipHeader[foundCount])
                            {
                                foundCount++;
                                if (foundCount != gzipHeader.Length)
                                    continue;

                                block.RemoveRange(block.Count - gzipHeader.Length, gzipHeader.Length);

                                _blocksToDecompress_array[readCycle] = block.ToArray();
                                readCycle++;
                                break;
                            }
                            foundCount = 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Split file to blocks error");
            }
        }

        private void DecompressionTasks()
        {
            try
            {
                for (int i = 0; i <= _iterations; i++)
                {
                    int j = i;
                    Thread decompress = new Thread(() => Block_Decompress_ToArray(j));
                    { }
                    decompress.Start();
                }
                while (Helper.decompressCount < _iterations + 1)
                {
                    if (Helper.decompressCount >= _iterations + 1)
                    {
                        Helper.decompressEvent.Set();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Deccompress blocks error");
            }
        }

        private void Block_Decompress_ToArray(int index)
        {
            BlockOperation bo = new BlockOperation(_blocksToDecompress_array[index]);
            byte[] byte_arr = bo.DecompressBlock();
            _blocksDecompressed_array[index] = byte_arr;
            Helper.decompressCount++;
            Helper.decompressEvent.WaitOne();
        }


        private void WriteTasks()
        {
            try
            {
                using (FileStream toStream = new FileStream(_outputFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    for (int i = 0; i <= _iterations; i++)
                    {
                        int j = i;
                        Thread write = new Thread(() => Block_Write_ToStream(toStream, _offsets_calc.Offsets[j], j));
                        { }
                        write.Start();
                    }
                    Helper.writeEvent.WaitOne();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Write blocks error");
            }
        }

        private void Block_Write_ToStream(FileStream toStream, long offset, int index)
        {
            Helper.rw_lock_slim.EnterWriteLock();
            toStream.Seek(offset, SeekOrigin.Begin);
            toStream.Write(_blocksDecompressed_array[index], 0, _blocksDecompressed_array[index].Length);
            Helper.writeCount++;
            if (Helper.writeCount >= _iterations+1) { 
            
                Helper.writeEvent.Set();
            }
            Helper.rw_lock_slim.ExitWriteLock();
        }
    }
}