using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Audiosort.Codecs.RIFF;

namespace Audiosort.Codecs.Wave
{
    /// <summary>
    /// Klasse für das Laden eines Wave-basierenden Dateiformats
    /// </summary>
    class WaveFile : RIFFFile
    {
        public WaveChunkFMT ChunkFMT;
        public RIFFChunk ChunkData;
        public WaveChunkFact ChunkFact;
        public WaveChunkCue ChunkCue;
        public WaveChunkPlaylist ChunkPlaylist;
        public WaveChunkList ChunkList;
        public Dictionary<string, string> ChunkListData;

        /// <summary>
        /// Klasse für das Laden eines Wave-basierenden Dateiformats
        /// </summary>
        /// <param name="filename">Dateiname</param>
        public WaveFile(string filename)
            : base(filename)
        {

        }

        /// <summary>
        /// Klasse für das Laden eines Wave-basierenden Dateiformats
        /// </summary>
        /// <param name="source">Stream</param>
        public WaveFile(Stream source)
            : base(source)
        { }

        /// <summary>
        /// Verarbeiten eines Chunks
        /// </summary>
        /// <param name="chunk">Zu verarbeitender Chunk</param>
        /// <param name="stream">Datenstream</param>
        /// <param name="toSeek">...</param>
        protected override void process_chunk(RIFFChunk chunk, BinaryReader stream, int toSeek)
        {
            switch (new String(chunk.ID).ToLower())
            {
                case "fmt ":
                    {
                        stream.BaseStream.Seek(-chunk.DataSize, SeekOrigin.Current);

                        ChunkFMT = new WaveChunkFMT();
                        ChunkFMT.ID = chunk.ID;
                        ChunkFMT.DataSize = chunk.DataSize;
                        ChunkFMT.Data = chunk.Data;

                        ChunkFMT.FormatTag = stream.ReadInt16();
                        ChunkFMT.Channels = stream.ReadInt16();
                        ChunkFMT.SampleRate = stream.ReadInt32();
                        ChunkFMT.BytesPerSecond = stream.ReadInt32();
                        ChunkFMT.BlockAlign = stream.ReadInt16();
                        ChunkFMT.BitsPerSample = stream.ReadInt16();

                        if (chunk.DataSize > 16)
                        {
                            if (chunk.DataSize == 20)
                            {
                                ChunkFMT.CompressionDataSize = stream.ReadInt16();
                                ChunkFMT.CompressionData = stream.ReadBytes(ChunkFMT.CompressionDataSize);
                            }
                            else
                            {
                                stream.BaseStream.Seek(chunk.DataSize - 16, SeekOrigin.Current);
                            }
                        }
                        break;
                    }
                case "fact":
                    {
                        stream.BaseStream.Seek(-chunk.DataSize, SeekOrigin.Current);

                        ChunkFact = new WaveChunkFact();
                        ChunkFact.ID = chunk.ID;
                        ChunkFact.DataSize = chunk.DataSize;
                        ChunkFact.Data = chunk.Data;
                        ChunkFact.Samples = stream.ReadInt32();

                        if (chunk.DataSize > 4)
                        {
                            stream.BaseStream.Seek(chunk.DataSize - 4, SeekOrigin.Current);
                        }
                        break;
                    }
                case "cue ":
                    {
                        stream.BaseStream.Seek(-chunk.DataSize, SeekOrigin.Current);

                        ChunkCue = new WaveChunkCue();
                        ChunkCue.ID = chunk.ID;
                        ChunkCue.DataSize = chunk.DataSize;
                        ChunkCue.Data = chunk.Data;
                        ChunkCue.CuePointCount = stream.ReadInt32();
                        ChunkCue.CuePoints = new WaveChunkCuePoint[ChunkCue.CuePointCount];
                        for (int i = 0; i < ChunkCue.CuePointCount; i++)
                        {
                            WaveChunkCuePoint point = new WaveChunkCuePoint();
                            point.Name = stream.ReadInt32();
                            point.Position = stream.ReadInt32();
                            point.ChunkID = stream.ReadChars(4);
                            point.ChunkStart = stream.ReadInt32();
                            point.BlockStart = stream.ReadInt32();
                            point.SampleOffset = stream.ReadInt32();
                            ChunkCue.CuePoints[i] = point;
                        }

                        if (chunk.DataSize > (4 + ChunkCue.CuePointCount * 24))
                        {
                            stream.BaseStream.Seek(chunk.DataSize - (4 + ChunkCue.CuePointCount * 24), SeekOrigin.Current);
                        }
                        break;
                    }
                case "plst":
                    {
                        stream.BaseStream.Seek(-chunk.DataSize, SeekOrigin.Current);

                        ChunkPlaylist = new WaveChunkPlaylist();
                        ChunkPlaylist.ID = chunk.ID;
                        ChunkPlaylist.DataSize = chunk.DataSize;
                        ChunkPlaylist.Data = chunk.Data;
                        ChunkPlaylist.SegmentCount = stream.ReadInt32();
                        ChunkPlaylist.Segments = new WaveChunkPlaylistEntry[ChunkPlaylist.SegmentCount];
                        for (int i = 0; i < ChunkPlaylist.SegmentCount; i++)
                        {
                            WaveChunkPlaylistEntry entry = new WaveChunkPlaylistEntry();
                            entry.Name = stream.ReadInt32();
                            entry.Length = stream.ReadInt32();
                            entry.Loop = stream.ReadInt32();
                            ChunkPlaylist.Segments[i] = entry;
                        }

                        if (chunk.DataSize > (4 + ChunkPlaylist.SegmentCount * 24))
                        {
                            stream.BaseStream.Seek(chunk.DataSize - (4 + ChunkPlaylist.SegmentCount * 12), SeekOrigin.Current);
                        }
                        break;
                    }
                case "data":
                    {
                        ChunkData = chunk;

                        switch (ChunkFMT.FormatTag)
                        {
                            case 1: // PCM
                                {
                                    // Keine Konvertierung notwendig
                                    break;
                                }
                            case 7: // PCM (uLaw)
                                {
                                    ChunkData = CompressionMulaw.DecodeDataChunk(ChunkData);

                                    // Format-Chunk anpassen
                                    ChunkFMT.BitsPerSample *= 2;
                                    ChunkFMT.BytesPerSecond *= 2;
                                    ChunkFMT.BlockAlign = (short)(ChunkFMT.Channels * ((ChunkFMT.BitsPerSample + 7) / 8));
                                    break;
                                }
                                // FAIL
                            case 17: // ADPCM
                                {
                                    ChunkData = CompressionADPCM_MS.DecodeDataChunk(ChunkData, this);

                                    // Format-Chunk anpassen
                                    ChunkFMT.BitsPerSample *= 4;
                                    ChunkFMT.BlockAlign = (short)(ChunkFMT.Channels * ChunkFMT.BitsPerSample / 8);
                                    ChunkFMT.BytesPerSecond = ChunkFMT.SampleRate * ChunkFMT.Channels * ChunkFMT.BitsPerSample / 8;
                                    break;
                                }
                        }
                        break;
                    }
                case "list":
                    {
                        stream.BaseStream.Seek(-chunk.DataSize, SeekOrigin.Current);

                        ChunkList = new WaveChunkList();
                        ChunkList.ID = chunk.ID;
                        ChunkList.DataSize = chunk.DataSize;
                        ChunkList.Data = chunk.Data;
                        ChunkList.TypeID = stream.ReadBytes(4);
                        ChunkList.ListData = stream.ReadBytes(ChunkList.DataSize - 4);

                        ChunkListData = new Dictionary<string, string>();
                        System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
                        using (BinaryReader s = new BinaryReader(new MemoryStream(ChunkList.ListData)))
                        {
                            while (s.BaseStream.Position < s.BaseStream.Length)
                            {
                                WaveChunkListEntry entry = new WaveChunkListEntry();
                                entry.Name = s.ReadBytes(4);
                                entry.DataSize = s.ReadInt32();
                                entry.Data = enc.GetString(s.ReadBytes(entry.DataSize));
                                ChunkListData.Add(enc.GetString(entry.Name), entry.Data);
                                s.BaseStream.Seek(1, SeekOrigin.Current);
                            }
                        }

                        break;
                    }
            }
        }
    }
}
