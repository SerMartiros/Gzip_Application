using Gzip_Application.Fundametals;
using System;
using System.Collections.Generic;
using System.IO;

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
            base.WriteTasks(_blocksDecompressed_array);
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
    }
}