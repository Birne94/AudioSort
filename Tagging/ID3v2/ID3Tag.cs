using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;

namespace Audiosort.Data
{
    public class ID3Tag : IEnumerable<ID3Frame>
    {
        public ID3Header Header;
        public ID3ExtendedHeader HeaderExtended;
        public List<ID3Frame> Frames;
        public ID3Footer Footer;
        public int Offset;
        public string Filename;

        #region Constants
        public static string[] Genres = new string[] {
            "Blues", "Classic Rock", "Country", "Dance", "Disco", "Funk", "Grunge", "Hip-Hop", "Jazz", "Metal",
            "New Age", "Oldies", "Other", "Pop", "R&B", "Rap", "Reggae", "Rock", "Techno", "Industrial",
            "Alternative", "Ska", "Death Metal", "Pranks", "Soundtrack", "Euro-Techno", "Ambient", "Trip-Hop",
            "Vocal", "Jazz+Funk", "Fusion", "Trance", "Classical", "Instrumental", "Acid", "House", "Game",
            "Sound Clip", "Gospel", "Noise", "AlternRock", "Bass", "Soul", "Punk", "Space", "Meditative",
            "Instrumental Pop", "Instrumental Rock", "Ethnic", "Gothic", "Darkwave", "Techno-Industrial",
            "Electronic", "Pop-Folk", "Eurodance", "Dream", "Southern Rock", "Comedy", "Cult", "Gangsta",
            "Top 40", "Christian Rap", "Pop/Funk", "Jungle", "Native American", "Cabaret", "New Wave",
            "Psychadelic", "Rave", "Showtunes", "Trailer", "Lo-Fi", "Tribal", "Acid Punk", "Acid Jazz",
            "Polka", "Retro", "Musical", "Rock & Roll", "Hard Rock"
        };
        #endregion

        public ID3Tag(string filename, bool parseit = true)
        {
            Filename = filename;

            if (parseit)
            {
                using (FileStream stream = new FileStream(filename, FileMode.Open))
                {
                    parse(stream);
                }
            }
            else
            {
                Frames = new List<ID3Frame>();
                Header = null;
                HeaderExtended = null;
                Footer = null;
                Offset = -1;
            }
        }

        public ID3Tag(Stream s)
        {
            parse(s);
        }

        protected void parse(Stream s)
        {
            BinaryReader stream = new BinaryReader(s);

            Offset = find_offset(stream);
            if (Offset < 0)
                throw new InvalidTagException();
            s.Seek(Offset, SeekOrigin.Begin);

            Header = ID3Header.Read(stream);
            if (!Header.Validate())
                throw new InvalidTagException();

            if (Header.FlagExtendedHeader)
                HeaderExtended = ID3ExtendedHeader.Read(stream);
            else
                HeaderExtended = null;

            Frames = new List<ID3Frame>();
            while (s.Position < Offset + Header.Size + 10)
            {
                ID3Frame frame = ID3Frame.Read(stream);
                if (frame.Size + s.Position >= Offset + Header.Size + 10)
                    break;
                if (frame.FrameID.Trim().Length > 0 && frame.Data.Trim().Length > 0)
                    Frames.Add(frame);
            }

            if (Header.FlagFooter)
            {
                s.Seek(Offset + 10 + Header.Size, SeekOrigin.Begin);
                Footer = ID3Footer.Read(stream);
            }
        }

        protected int find_offset(BinaryReader stream)
        {
            ID3Header h = ID3Header.Read(stream);
            if (h == null)
                return -1;
            if (h.Validate())
                return 0;

            stream.BaseStream.Seek(-10, SeekOrigin.End);
            ID3Footer f = ID3Footer.Read(stream);
            if (f.Validate())
                return (int)stream.BaseStream.Length - f.Size - 20;

            return -1;
        }

        public string this[string frame]
        {
            get
            {
                var result = Frames.Where(f => f.FrameID == frame);
                if (result.Count() > 0)
                {
                    string data = result.ElementAt(0).Data;
                    if (data.StartsWith("\x01??"))
                    {
                        data = data.Replace("\x01??", "");
                    }
                    data = data.Replace("\0", "");
                    return data.Trim();
                }
                return "";
            }
        }

        public IEnumerator<ID3Frame> GetEnumerator() { return Frames.GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
    }
}