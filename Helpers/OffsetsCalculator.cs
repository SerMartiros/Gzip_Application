namespace Gzip_Application
{
    public class OffsetsCalculator
    {
        private long[] _offsets;
        public OffsetsCalculator(int iterations)
        {
            _offsets = new long[iterations];
        }

        public long[] Offsets
        {
            get
            {
                return _offsets;
            }
        }

        public void CalculateOffsets(byte[][] byteArr)
        {
            for (int i = 0; i <= byteArr.Length - 1; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    if (i == 0)
                    {
                        _offsets[0] = 0;
                    }
                    else
                    {
                        _offsets[i] += byteArr[j].Length;
                    }
                }
            }
        }
    }
}
