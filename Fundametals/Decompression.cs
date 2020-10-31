using Gzip_Application.Interfaces;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Gzip_Application
{
    public class Decompression
    {
        private string _inputFile;
        private string _outputFile;
        private long _fileLength;
        private Dictionary<int, byte[]> _blocksToDecompress_array = new Dictionary<int, byte[]>();
        private byte[][] _blocksDecompressed_array;
        private byte[] gzipHeader = Helper.gzipHeader;
        private OffsetsCalculator _offsets_calc;

        public Decompression(string inputFile, string outputFile)
        {
            _inputFile = inputFile;
            _outputFile = outputFile;
            _fileLength = new FileInfo(inputFile).Length;

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
                                _blocksToDecompress_array.Add(readCycle, block.ToArray());
                                readCycle++;
                                break;
                            }
                            if (currentByte == gzipHeader[foundCount])
                            {
                                foundCount++;
                                if (foundCount != gzipHeader.Length)
                                    continue;

                                block.RemoveRange(block.Count - gzipHeader.Length, gzipHeader.Length);
                                _blocksToDecompress_array.Add(readCycle, block.ToArray());
                                readCycle++;
                                break;
                            }
                            foundCount = 0;
                        }
                    }
                }
                _blocksDecompressed_array = new byte[readCycle][];
                _offsets_calc = new OffsetsCalculator(readCycle);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("File split into blocks error");
            }
        }

        private void DecompressionTasks()
        {
            try
            {
                for (int i = 0; i < _blocksToDecompress_array.Count; i++)
                {
                    Helper.semaf.WaitOne();
                    int j = i;
                    Thread decompress = new Thread(() => Block_Decompress_ToArray(j));
                    { }
                    decompress.Start();

                }
                while (Helper.decompressCount < _blocksDecompressed_array.Length)
                {
                    if (Helper.compressCount >= _blocksDecompressed_array.Length)
                    {
                        Helper.compressEvent.Set();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Decompression blocks error");
            }
        }

        private void Block_Decompress_ToArray(int index)
        {
            IDecompressable bo = new BlockOperation(_blocksToDecompress_array[index]);
            byte[] byte_arr = bo.DecompressBlock();
            _blocksDecompressed_array[index] = byte_arr;
            Helper.decompressCount++;
            Helper.semaf.Release();
            Helper.compressEvent.WaitOne();
        }


        private void WriteTasks()
        {
            try
            {
                using (FileStream toStream = new FileStream(_outputFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    for (int i = 0; i < _blocksToDecompress_array.Count; i++)
                    {
                        Helper.semaf.WaitOne();
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
            IWritable bo = new BlockOperation(_blocksDecompressed_array[index]);
            bo.WriteBlock(toStream, offset);
            Helper.writeCount++;
            if (Helper.writeCount >= _blocksToDecompress_array.Count)
            {
                Helper.writeEvent.Set();
            }
            Helper.semaf.Release();
            Helper.rw_lock_slim.ExitWriteLock();
        }
    }
}