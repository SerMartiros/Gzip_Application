using Gzip_Application.Fundametals;
using Gzip_Application.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Gzip_Application
{
    public class Decompression:Archivation
    {
        public Decompression(string inputFile, string outputFile)
        {
            _inputFile = inputFile;
            _outputFile = outputFile;
            _fileLength = new FileInfo(inputFile).Length;

        }
        public override void DecompressFile()
        {
            SplitTasks();
            base.DecompressionTasks();
            _offsets_calc.CalculateOffsets(_blocksDecompressed_array);
            WriteTasks();
        }


        public override void SplitTasks()
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
        public override void WriteTasks()
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
                    Console.WriteLine("Decompression Finished Successfully");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Write blocks error");
            }
        }
        public override void Block_Write_ToStream(FileStream toStream, long offset, int index)
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