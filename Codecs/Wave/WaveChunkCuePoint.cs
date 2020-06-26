using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Audiosort.Codecs.Wave
{
    /// <summary>
    /// Eintrag im cue-Chunk
    /// </summary>
    class WaveChunkCuePoint
    {
        public int Name;
        public int Position;
        public char[] ChunkID;
        public int ChunkStart;
        public int BlockStart;
        public int SampleOffset;
    }
}
