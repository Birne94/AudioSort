using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// http://www.speech.cs.cmu.edu/comp.speech/Section2/Q2.7.html

namespace Audiosort.Codecs.Wave
{
    class CompressionMulaw
    {
        /// <summary>
        /// MuLaw-Kodierten DataChunk decodieren
        /// </summary>
        /// <param name="Data"></param>
        /// <returns>Dekodierten Chunk</returns>
        public static RIFF.RIFFChunk DecodeDataChunk(RIFF.RIFFChunk Data)
        {
            byte[] newData = new byte[Data.DataSize * 2];
            for (int i = 0; i < Data.DataSize; i++)
            {
                int[] exp_lut = new int[8] { 0, 132, 396, 924, 1980, 4092, 8316, 16764 };

                int x = Data.Data[i];
                x = (byte)~x;
                int sign = x & 0x80;
                int exp = (x >> 4) & 0x07;
                int data = x & 0x0f;

                data = exp_lut[exp] + (data << (exp + 3));
                if (sign != 0) data = -data;
                newData[2 * i] = (byte)(data & 0xff);
                newData[2 * i + 1] = (byte)(data >> 8);
            }
            Data.DataSize *= 2;
            Data.Data = new byte[Data.DataSize];
            Buffer.BlockCopy(newData, 0, Data.Data, 0, Data.DataSize);

            return Data;
        }
    }
}
