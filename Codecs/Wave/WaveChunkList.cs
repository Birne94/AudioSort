using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Audiosort.Codecs.Wave
{
    class WaveChunkList : RIFF.RIFFChunk
    {
        public byte[] TypeID;
        public byte[] ListData;
    }
}
