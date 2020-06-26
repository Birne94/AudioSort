using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Audiosort.Codecs.Wave;

namespace Audiosort.Codecs
{
    public class WAVStream : AudioStream
    {
        WaveFile WaveFile = null;

        public WAVStream(Stream source)
            : base()
        {
            WaveFile = new WaveFile(source);
            Source = new MemoryStream();
            Source.Write(WaveFile.ChunkData.Data, 0, WaveFile.ChunkData.DataSize);
            Source.Seek(0, SeekOrigin.Begin);

            Format.Channels = WaveFile.ChunkFMT.Channels;
            Format.BitsPerSample = WaveFile.ChunkFMT.BitsPerSample;
            Format.SampleRate = WaveFile.ChunkFMT.SampleRate;
            Format.BlockAlign = WaveFile.ChunkFMT.BlockAlign;
            Format.BytesPerSecond = WaveFile.ChunkFMT.BytesPerSecond;
            Format.BufferLength = Source.Length;
        }

        public WAVStream(string filename)
            : this(new FileStream(filename, FileMode.Open, FileAccess.Read))
        { }
    }
}
