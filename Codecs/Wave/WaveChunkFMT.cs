using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Audiosort.Codecs.Wave
{
    /// <summary>
    /// FMT-Chunk einer Wave-Datei
    /// </summary>
    class WaveChunkFMT : RIFF.RIFFChunk
    {
        public short FormatTag;
        public short Channels;
        public int SampleRate;
        public int BytesPerSecond;
        public short BlockAlign;
        public short BitsPerSample;

        public int CompressionDataSize;
        public byte[] CompressionData;
    }
}
