using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Audiosort.Codecs.CD
{
    public class DataEventArgs : EventArgs
    {
        public byte[] Data;
        public uint Size;
        public DataEventArgs(byte[] data, uint size)
        {
            Data = data;
            Size = size;
        }
    }

    public class CDBufferFiller
    {
        byte[] DataBuffer;
        int Position;

        public CDBufferFiller(byte[] buff)
        {
            DataBuffer = buff;
        }

        public void OnDataRead(object sender, DataEventArgs e)
        {
            Buffer.BlockCopy(e.Data, 0, DataBuffer, Position, (int)e.Size);
            Position += (int)e.Size;
        }
    }

    public delegate void CdDataReadEventHandler(object sender, DataEventArgs ea);

    public enum DeviceChangeEventType { DeviceInserted, DeviceRemoved };
    public class DeviceChangeEventArgs : EventArgs
    {
        public DeviceChangeEventType Type;
        public char Drive;

        public DeviceChangeEventArgs(char drive, DeviceChangeEventType type)
        {
            Type = type;
            Drive = drive;
        }
    }

    public delegate void DeviceChangeEventHandler(object sender, DeviceChangeEventArgs ea);
}
