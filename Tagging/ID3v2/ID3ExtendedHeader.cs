using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Audiosort.Data
{
    public class ID3ExtendedHeader : ID3Structure
    {
        public int Size;
        public int FlagBytes;
        public byte[] Flags;

        public static ID3ExtendedHeader Read(BinaryReader stream)
        {
            ID3ExtendedHeader ExtendedHeader = new ID3ExtendedHeader();
            byte[] size = stream.ReadBytes(4);
            ExtendedHeader.Size = syncsafe2int(size);
            ExtendedHeader.FlagBytes = stream.ReadByte();
            ExtendedHeader.Flags = stream.ReadBytes(ExtendedHeader.FlagBytes);
            stream.BaseStream.Seek(ExtendedHeader.Size - 4 - 1 - ExtendedHeader.FlagBytes, SeekOrigin.Current);

            return ExtendedHeader;
        }

        public new bool Validate()
        {
            bool valid = true;

            return valid;
        }
    }
}
