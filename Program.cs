using System;
using System.IO;

namespace Gzip_Application
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Expected Three Arguments -Command(Compress/Decompress)- -Inpur File Path- -Output File Path- ");
            }
            string executableAction = args[0];
            string inputFile = args[1];
            string outputFile = args[2];

            switch (executableAction)
            {
                case "Compress":
                    Compression compressionTask = new Compression(inputFile, outputFile);
                    if (File.Exists(outputFile))
                    {
                        Console.WriteLine($"File {outputFile} already exists");
                    }
                    else
                    {
                        compressionTask.CompressFile();
                    }
                    break;
                case "Decompress":
                    Decompression decompressionTask = new Decompression(inputFile, outputFile);
                    if (File.Exists(outputFile))
                    {
                        Console.WriteLine($"File {outputFile} already exists");
                    }
                    else
                    {
                        decompressionTask.DecompressFile();
                    }
                    break;
                default:
                    Console.WriteLine("Wrong Method Name");
                    break;
            }
        }
    }
}
