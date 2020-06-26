using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Audiosort.Codecs.Wave
{
    class WaveChunkListEntry
    {
        public byte[] Name;
        public int DataSize;
        public string Data;
    }
}
