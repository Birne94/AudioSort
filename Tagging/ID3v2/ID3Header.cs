using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Audiosort.Data
{
    public class ID3Header : ID3Structure
    {
        public const int FLAG_UNSYNCHRONISATION = 128;
        public const int FLAG_EXTENTED_HEADER = 64;
        public const int FLAG_EXPERIMENTAL = 32;
        public const int FLAG_FOOTER = 16;

        public byte[] Identifier;
        public byte[] Version;
        public byte Flags;
        public int Size;

        public int VersionMajor
        {
            get { return Version[0]; }
        }
        public int VersionMinor
        {
            get { return Version[1]; }
        }
        public bool FlagUnsynchronisation
        {
            get { return (Flags & FLAG_UNSYNCHRONISATION) != 0; }
        }
        public bool FlagExtendedHeader
        {
            get { return (Flags & FLAG_EXTENTED_HEADER) != 0; }
        }
        public bool FlagExperimental
        {
            get { return (Flags & FLAG_EXPERIMENTAL) != 0; }
        }
        public bool FlagFooter
        {
            get { return (Flags & FLAG_FOOTER) != 0; }
        }
        public bool Flag(int x)
        {
            return (Flags & (1 << x)) != 0;
        }

        public static ID3Header Read(BinaryReader stream)
        {
            ID3Header Header = new ID3Header();
            Header.Identifier = stream.ReadBytes(3);
            Header.Version = stream.ReadBytes(2);
            Header.Flags = stream.ReadByte();
            byte[] size = stream.ReadBytes(4);
            Header.Size = syncsafe2int(size);

            if (!Header.Validate())
                return null;

            return Header;
        }

        public new bool Validate()
        {
            bool valid = true;

            if (!(Identifier[0] == 'I' && Identifier[1] == 'D' && Identifier[2] == '3'))
                valid = false;
            if (Version[0] == 255 || Version[1] == 255)
                valid = false;
            if (Flag(0) || Flag(1) || Flag(2) || Flag(3))
                valid = false;
            if (VersionMajor != 3)
                valid = false;

            return valid;
        }
    }
}
