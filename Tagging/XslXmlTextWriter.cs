using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Audiosort
{
    public sealed class XslXmlTextWriter : XmlTextWriter
    {
        private string stylesheet;

        public XslXmlTextWriter(string filename, Encoding enc, string stylesheet)
            : base(filename, enc)
        {
            this.stylesheet = stylesheet;
        }

        public override void WriteStartElement(string prefix, string localName, string ns)
        {
            if (WriteState == WriteState.Prolog || WriteState == WriteState.Start)
            {
                base.WriteProcessingInstruction("xml-stylesheet",
                  String.Format("type=\"text/xsl\" href=\"{0}\"", stylesheet));
            }
            base.WriteStartElement(prefix, localName, ns);
        }
    }
}
