using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Audiosort.Codecs.Wave
{
    /// <summary>
    /// Eintrag im plst-Chunk
    /// </summary>
    class WaveChunkPlaylistEntry
    {
        public int Name;
        public int Length;
        public int Loop;
    }
}
