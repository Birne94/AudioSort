using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Audiosort.Codecs
{
    public class MP3Stream : AudioStream
    {
        Mp3Sharp.Mp3Stream mp3 = null;

        public MP3Stream(string filename)
        {
            mp3 = new Mp3Sharp.Mp3Stream(filename);
            init();
        }

        public MP3Stream(Stream source)
        {
            mp3 = new Mp3Sharp.Mp3Stream(source);
            init();
        }

        protected void init()
        {
            Format.Channels = mp3.ChannelCount;
            Format.BitsPerSample = 16;
            Format.SampleRate = 44100;
            Format.BlockAlign = 4;
            Format.BytesPerSecond = 176400;

            Source = new MemoryStream();
            mp3.CopyTo(Source);
            mp3.Close();
            mp3 = null;
            Source.Seek(0, SeekOrigin.Begin);

            Format.BufferLength = Source.Length;
        }
    }
}
