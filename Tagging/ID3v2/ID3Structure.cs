using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Audiosort.Data
{
    public class ID3Structure
    {
        public bool Validate()
        {
            return true;
        }

        protected static int syncsafe2int(byte[] syncsafe)
        {
            return syncsafe[0] << 21 | syncsafe[1] << 14 | syncsafe[2] << 7 | syncsafe[3];
        }
    }
}
