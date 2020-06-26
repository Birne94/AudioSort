using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Audiosort.Codecs.RIFF
{
    /// <summary>
    /// Chunk einer RIFF-Datei
    /// </summary>
    class RIFFChunk
    {
        public char[] ID;
        public int DataSize;
        public byte[] Data;
    }
}
