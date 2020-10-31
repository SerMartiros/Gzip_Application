﻿using Gzip_Application.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Gzip_Application.Fundametals
{
    public abstract class Archivation
    {
        protected string _inputFile;
        protected string _outputFile;
        protected int _iterations;
        protected long _fileLength;

        protected byte[][] _blocksToCompress_array;
        protected byte[][] _blocksCompressed_array;

        protected OffsetsCalculator _offsets_calc;
        protected IterationsCalculator _iterations_calc;


        protected Dictionary<int, byte[]> _blocksToDecompress_array = new Dictionary<int, byte[]>();
        protected byte[][] _blocksDecompressed_array;
        protected byte[] gzipHeader = Helper.gzipHeader;

        public virtual void CompressFile()
        {
            Console.WriteLine("Base CompressFile");
        }
        public virtual void DecompressFile()
        {
            Console.WriteLine("Base DecompressFile");
        }
        public abstract void SplitTasks();

        public virtual void CompressionTasks()
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

        public virtual void DecompressionTasks()
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
        public abstract void WriteTasks();
        public virtual void Block_Compress_ToArray(int index)
        {
            ICompressable bo = new BlockOperation(_blocksToCompress_array[index]);
            byte[] byte_arr = bo.CompressBlock();
            _blocksCompressed_array[index] = byte_arr;
            Helper.compressCount++;
            Helper.semaf.Release();
            Helper.compressEvent.WaitOne();
        }
        public virtual void Block_Decompress_ToArray(int index)
        {
            IDecompressable bo = new BlockOperation(_blocksToDecompress_array[index]);
            byte[] byte_arr = bo.DecompressBlock();
            _blocksDecompressed_array[index] = byte_arr;
            Helper.decompressCount++;
            Helper.semaf.Release();
            Helper.compressEvent.WaitOne();
        }
        public abstract void Block_Write_ToStream(FileStream toStream, long off, int index);

    }
}