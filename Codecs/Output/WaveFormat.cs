using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Audiosort.Codecs
{
    public struct WaveFormat
    {
        public short Channels;
        public short BitsPerSample;
        public int SampleRate;
        public short BlockAlign;
        public int BytesPerSecond;
        public long BufferLength;
    }
}
