using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Audiosort.Codecs.CD
{
    public class CDException : Exception
    {
        public int ErrorCode = 0;

        public CDException()
        {
            ErrorCode = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
        }
    }

    public class InvalidCD : CDException
    { }

    public class InvalidTOC : CDException
    { }

    public class NoCD : CDException
    { }

    public class TrackNotFound : CDException
    { }
}
