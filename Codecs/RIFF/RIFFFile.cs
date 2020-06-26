using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Audiosort.Codecs.RIFF
{
    /// <summary>
    /// Klasse für das Laden eines RIFF-basierenden Dateiformats
    /// </summary>
    class RIFFFile
    {
        /// <summary>
        /// RIFF-Header
        /// </summary>
        public RIFFHeader Header;

        /// <summary>
        /// Chunk-Liste
        /// </summary>
        public List<RIFFChunk> Chunks;

        public RIFFFile(string filename)
        {
            FileStream file = new FileStream(filename, FileMode.Open);
            init(file);
            file.Close();
        }

        /// <summary>
        /// Klasse für das Laden eines RIFF-basierenden Dateiformats
        /// </summary>
        /// <param name="filename">Dateiname</param>
        public RIFFFile(Stream source)
        {
            init(source);
        }

        protected void init(Stream file)
        {
            Header = new RIFFHeader();
            Chunks = new List<RIFFChunk>();

            using (BinaryReader stream = new BinaryReader(file))
            {
                Header.Identifier = stream.ReadChars(4);
                Header.FileSize = stream.ReadInt32();
                Header.FileType = stream.ReadChars(4);

                while (stream.BaseStream.Position < stream.BaseStream.Length)
                {
                    try
                    {
                        RIFFChunk chunk = new RIFFChunk();
                        chunk.ID = stream.ReadChars(4);
                        chunk.DataSize = stream.ReadInt32();
                        chunk.Data = stream.ReadBytes(chunk.DataSize);
                        process_chunk(chunk, stream, 4 + chunk.DataSize); // Chunk weiterverarbeiten
                        Chunks.Add(chunk);
                    }
                    catch (EndOfStreamException)
                    {

                    }
                }
            }
        }

        /// <summary>
        /// Verarbeiten eines Chunks
        /// </summary>
        /// <param name="chunk">Zu verarbeitender Chunk</param>
        /// <param name="stream">Datenstream</param>
        /// <param name="toSeek">...</param>
        protected virtual void process_chunk(RIFFChunk chunk, BinaryReader stream, int toSeek)
        {

        }
    }
}
