using System;
using System.Threading;

namespace Gzip_Application
{
    public static class Helper
    {
        public static volatile Semaphore semaf = new Semaphore(Environment.ProcessorCount, Environment.ProcessorCount);
        public static readonly byte[] gzipHeader = { 0x1f, 0x8b, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00 };
        public const int blockSize = 1024 * 1024;
        public static ManualResetEvent writeLock = new ManualResetEvent(false);
        public static volatile int writeCount = 0;
        public static ManualResetEvent archivationLock = new ManualResetEvent(false);
        public static volatile int archivationCount = 0;
        public static ReaderWriterLockSlim rw_lock_slim = new ReaderWriterLockSlim();
    }
}
