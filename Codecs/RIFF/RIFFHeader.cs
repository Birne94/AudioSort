using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Audiosort.Codecs.RIFF
{
    /// <summary>
    /// Headerstruktur einer RIFF-Datei
    /// </summary>
    class RIFFHeader
    {
        public char[] Identifier;   // "RIFF"
        public int FileSize;        // Dateigröße - 8
        public char[] FileType;     // "WAVE" etc.
    }
}
