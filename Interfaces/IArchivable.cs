using System.IO;

namespace Gzip_Application.Fundametals
{
    public interface IArchivable
    {
        void Block_Compress_ToArray(int index);
        void Block_Decompress_ToArray(int index);
        void Block_Write_ToStream(FileStream toStream, long offset, byte[] block, int count);
        void CompressFile();
        void CompressionTasks();
        void DecompressFile();
        void DecompressionTasks();
        void SplitTasks();
        void WriteTasks(byte[][] byteArr);
        void Read_Block_ToArray(BinaryReader breader, int index);
    }
}