using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Audiosort.Codecs.CD;

namespace Audiosort.Codecs
{
    public class CDStream : AudioStream
    {
        CDDevice Device = null;

        public CDStream(CDDevice device, int track)
            : base()
        {
            Device = device;
            init(track);
        }

        public CDStream(char path, int track)
            : base()
        {
            Device = new CDDevice(path);
            init(track);
            Device.Close();
        }

        void init(int track)
        {
            uint size = Device.TrackSize(track);
            byte[] buff = new byte[size];
            uint read = 0;
            Device.ReadTrack(track, buff, ref read, 0, 0);

            Source = new MemoryStream();
            Source.Write(buff, 0, (int)read);
            Source.Seek(0, SeekOrigin.Begin);


            Format.Channels = 2;
            Format.BitsPerSample = 16;
            Format.SampleRate = 44100;
            Format.BlockAlign = 4;
            Format.BytesPerSecond = 176400;
            Format.BufferLength = Source.Length;
        }
    }
}
