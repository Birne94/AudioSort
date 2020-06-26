using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Audiosort.Codecs.Wave
{
    class CompressionADPCM_IMA
    {
        static int[] index_table = new int[16] {
            -1, -1, -1, -1, 2, 4, 6, 8,
            -1, -1, -1, -1, 2, 4, 6, 8
        };
        static int[] step_table = new int[89] {
            7, 8, 9, 10, 11, 12, 13, 14, 16, 17,
            19, 21, 23, 25, 28, 31, 34, 37, 41, 45,
            50, 55, 60, 66, 73, 80, 88, 97, 107, 118,
            130, 143, 157, 173, 190, 209, 230, 253, 279, 307,
            337, 371, 408, 449, 494, 544, 598, 658, 724, 796,
            876, 963, 1060, 1166, 1282, 1411, 1552, 1707, 1878, 2066,
            2272, 2499, 2749, 3024, 3327, 3660, 4026, 4428, 4871, 5358,
            5894, 6484, 7132, 7845, 8630, 9493, 10442, 11487, 12635, 13899,
            15289, 16818, 18500, 20350, 22385, 24623, 27086, 29794, 32767
        };

        /// <summary>
        /// ADPCM-Kodierten DataChunk decodieren
        /// </summary>
        /// <param name="Data"></param>
        /// <returns>Dekodierten Chunk</returns>
        public static RIFF.RIFFChunk DecodeDataChunk(RIFF.RIFFChunk Data, Wave.WaveFile File)
        {
            short predictor = 0;
            int step_index = 0;
            int step = step_table[step_index];

            int nibble_size = File.ChunkFMT.BitsPerSample * File.ChunkFMT.Channels;
            int nibble_count = Data.DataSize * 8 / nibble_size;
            if (File.ChunkFact != null && File.ChunkFact.Samples > 0)
            {
                nibble_count = File.ChunkFact.Samples;
            }

            if (nibble_size != 4)
            {
                throw new NotImplementedException();
            }

            byte[] newData = new byte[nibble_count * 2];

            int pos = 0; // bit
            for (int i = 0; i < nibble_count; i++)
            {
                int nibble;
                if (pos % 8 != 0)   // first 4 bit
                {
                    nibble = Data.Data[pos / 8] >> 4;
                    //nibble = Data.Data[pos / 8] & 0x0f;
                }
                else // last 4 bit
                {
                    nibble = Data.Data[pos / 8] & 0x0f;
                    //nibble = Data.Data[pos / 8] >> 4;
                }
                
                step_index += index_table[(uint)nibble];

                //int diff = (int)((double)nibble + 0.5) * step / 4;
                int diff = (step * nibble / 4) + (step / 8);
                predictor += (short)diff;

                if (step_index >= step_table.Length)
                    step_index = step_table.Length - 1;
                if (step_index < 0)
                    step_index = 0;
                if (predictor > short.MaxValue)
                    predictor = short.MaxValue;
                if (predictor < short.MinValue)
                    predictor = short.MinValue;

                step = step_table[step_index];
                
                newData[2 * i] = (byte)(predictor & 0xff);
                newData[2 * i + 1] = (byte)(predictor >> 8);

                pos += nibble_size;
            }

            Data.DataSize = nibble_count * 2;
            Data.Data = new byte[Data.DataSize];
            Buffer.BlockCopy(newData, 0, Data.Data, 0, Data.DataSize);

            return Data;
        }
    }
}
