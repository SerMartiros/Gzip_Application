using Gzip_Application.Fundametals;
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
                Console.WriteLine("Expected Three Arguments - Command(Compress or Decompress)  'Input Full File Path String'  'Output Full File Path String' ");
                Console.ReadLine();
            }
            string executableAction = args[0];
            string inputFile = args[1];
            string outputFile = args[2];

            switch (executableAction)
            {
                case "Compress":
                    Archivation compressionTask = new Compression(inputFile, outputFile);
                    if (File.Exists(outputFile) || !File.Exists(inputFile))
                    {
                        Console.WriteLine($"File {outputFile} already exists or File {inputFile} doesn't exist ");
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Started Compression");
                        compressionTask.CompressFile();
                    }
                    break;
                case "Decompress":
                    Archivation decompressionTask = new Decompression(inputFile, outputFile);
                    if (File.Exists(outputFile) || !File.Exists(inputFile))
                    {
                        Console.WriteLine($"File {outputFile} already exists or File {inputFile} doesn't exist ");
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Started Decompression");
                        decompressionTask.DecompressFile();
                    }
                    break;
                default:
                    Console.WriteLine("Wrong Method Name");
                    break;
            }

            Console.ReadLine();
        }
    }
}
