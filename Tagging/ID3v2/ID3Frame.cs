using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Audiosort.Data
{
    public class ID3Frame : ID3Structure
    {
        public string FrameID;
        public int Size;
        public byte[] Flags;
        public byte[] DataRaw;
        public string Data;

        public static ID3Frame Read(BinaryReader stream)
        {
            ID3Frame Frame = new ID3Frame();
            byte[] frameID = stream.ReadBytes(4);
            Frame.FrameID = Encoding.ASCII.GetString(frameID);
            byte[] size = stream.ReadBytes(4);
            Frame.Size = syncsafe2int(size);
            Frame.Flags = stream.ReadBytes(2);
            Frame.DataRaw = stream.ReadBytes(Frame.Size);
            Frame.Data = Encoding.ASCII.GetString(Frame.DataRaw);

            return Frame;
        }

        public static ID3Frame Create(string id, string value)
        {
            ID3Frame frame = new ID3Frame();
            frame.FrameID = id;
            frame.Data = value;
            frame.Size = value.Length;
            return frame;
        }
    }
}
