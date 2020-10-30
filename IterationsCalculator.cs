using System;

namespace Gzip_Application
{
    class IterationsCalculator
    {
        private long _fileLength;
        private int _blockSize;
        public IterationsCalculator(long fLength, int bSize)
        {
            _fileLength = fLength;
            _blockSize = bSize;
        }
        public int IterationCount()
        {
            float result = ((float)(_fileLength)) / _blockSize;
            int converted = Convert.ToInt32(Math.Ceiling(result));
            return converted;
        }
    }
}
