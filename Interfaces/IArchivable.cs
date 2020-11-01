using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gzip_Application.Interfaces
{
    public interface IArchivable
    {
        void CompressFile();
        void DecompressFile();
    }
}
