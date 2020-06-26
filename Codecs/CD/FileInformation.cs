using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Audiosort.Codecs.CD
{
    /// <summary>
    /// Informationen über eine (Musik-) Datei
    /// </summary>
    public class FileInformation
    {
        /// <summary>
        /// Länge des Stücks in Sekunden
        /// </summary>
        public float Duration;

        /// <summary>
        /// Bit-Rate
        /// </summary>
        public int BitRate;

        /// <summary>
        /// Titel des Songs
        /// </summary>
        public string SongTitle;

        /// <summary>
        /// Künstler des Songs
        /// </summary>
        public string SongArtist;

        /// <summary>
        /// Songalbum
        /// </summary>
        public string SongAlbum;

        /// <summary>
        /// Erscheinungsjahr
        /// </summary>
        public int SongYear;

        /// <summary>
        /// Genre
        /// </summary>
        public string SongGenre;

        /// <summary>
        /// Track auf CD/Album
        /// </summary>
        public int SongTrack;

        /// <summary>
        /// Kommentar, Platz für sonstiges.
        /// </summary>
        public string Comment;
    }
}
