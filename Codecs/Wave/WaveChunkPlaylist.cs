using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Audiosort.Codecs.Wave
{
    /// <summary>
    /// PLST-Chunk einer Wave-Datei
    /// </summary>
    class WaveChunkPlaylist : RIFF.RIFFChunk
    {
        public int SegmentCount;
        public WaveChunkPlaylistEntry[] Segments;
    }
}
