using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Audiosort.Codecs.Wave
{
    /// <summary>
    /// FACT-Chunk einer Wave-Datei
    /// </summary>
    class WaveChunkFact : RIFF.RIFFChunk
    {
        public int Samples;
    }
}
