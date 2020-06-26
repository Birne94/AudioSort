using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Audiosort.Data
{
    public class ID3Footer : ID3Header
    {
        public new static ID3Footer Read(BinaryReader stream)
        {
            return (ID3Footer)ID3Header.Read(stream);
        }

        public new bool Validate()
        {
            bool valid = true;

            if (!(Identifier[0] == '3' && Identifier[1] == 'D' && Identifier[2] == 'I'))
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
