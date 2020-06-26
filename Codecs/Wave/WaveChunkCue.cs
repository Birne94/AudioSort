using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Audiosort.Codecs.Wave
{
    /// <summary>
    /// CUE-Chunk einer Wave-Datei
    /// </summary>
    class WaveChunkCue : RIFF.RIFFChunk
    {
        public int CuePointCount;
        public WaveChunkCuePoint[] CuePoints;
    }
}
