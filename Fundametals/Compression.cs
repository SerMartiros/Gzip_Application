using Gzip_Application.Fundametals;
using System;
using System.IO;

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
            base.WriteTasks(_blocksCompressed_array);
        }

        public override void SplitTasks()
        {
            try
            {
                using (BinaryReader breader = new BinaryReader(File.Open(_inputFile, FileMode.Open, FileAccess.Read, FileShare.Read)))
                {
                    for (int i = 0; i < _iterations; i++)
                    {
                        Read_Block_ToArray(breader, i);
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("File split into blocks error");
            }
        }
    }
}
