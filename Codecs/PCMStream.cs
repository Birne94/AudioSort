using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Audiosort.Codecs
{
    public class PCMStream : AudioStream
    {
        public PCMStream(Stream source)
            : base(source)
        {
            Format.Channels = 2;
            Format.BitsPerSample = 16;
            Format.SampleRate = 44100;
            Format.BlockAlign = 4;
            Format.BytesPerSecond = 176400;
            Format.BufferLength = Source.Length;
        }

        public PCMStream(string filename)
            : this(new FileStream(filename, FileMode.Open, FileAccess.Read))
        { }
    }
}
