using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Audiosort.Codecs
{
    public class AudioStream
    {
        public Stream Source;
        public WaveFormat Format;

        public AudioStream(string filename)
            : this(new FileStream(filename, FileMode.Open))
        { }

        public AudioStream(Stream source)
        {
            Source = source;
            Format = new WaveFormat();
        }

        public AudioStream()
        {
            Source = null;
            Format = new WaveFormat();
        }

        public int Read(byte[] buff, int offset, int count)
        {
            return Source.Read(buff, offset, count);
        }

        public void CopyTo(Stream dest)
        {
            Source.CopyTo(dest);
        }

        public long Length
        {
            get
            {
                return Source.Length;
            }
        }

        public void Close()
        {
            Source.Close();
        }

        public static string[] Formats = {
                                             "mp3", "raw", "wav"
                                         };

        public static AudioStream Load(string url)
        {
            AudioStream s = null;

            if (url.EndsWith(".mp3"))
                s = new MP3Stream(url);
            else if (url.EndsWith(".raw") || url.EndsWith(".pcm"))
                s = new PCMStream(url);
            else if (url.EndsWith(".wav"))
                s = new WAVStream(url);

            return s;
        }
    }
}
