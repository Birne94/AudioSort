using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace Audiosort.Codecs.CD
{
    /// <summary>
    /// Informationen über eine CD
    /// </summary>
    public class CDInformation
    {
        /// <summary>
        /// CDDB-Script
        /// </summary>
        const string Server = "http://freedb.org/~cddb/cddb.cgi";

        /// <summary>
        /// Authentifizierungszeile
        /// </summary>
        const string ServerAuth = "&hello=birne+www.daniel-birnstiel.de+audiosort+1&proto=6";

        /// <summary>
        /// CDDB-ID
        /// </summary>
        public string DiscID;

        /// <summary>
        /// Album
        /// </summary>
        public string Title;

        /// <summary>
        /// Künstler
        /// </summary>
        public string Artist;

        /// <summary>
        /// Jahr
        /// </summary>
        public int Year;

        /// <summary>
        /// Genre
        /// </summary>
        public string Genre;

        /// <summary>
        /// Zusatzinformationen
        /// </summary>
        public string ExtendedData;

        /// <summary>
        /// Tracks
        /// </summary>
        public List<FileInformation> Titles = new List<FileInformation>();

        /// <summary>
        /// Meta-Informationen abfragen
        /// </summary>
        /// <param name="device">CDDevice</param>
        /// <returns>Liste mit Treffern</returns>
        public static List<CDInformation> Query(CDDevice device)
        {
            List<CDInformation> result = new List<CDInformation>();

            string url = Server + "?cmd=cddb+query+" +device.GetCDDB1ID();
            url += "+" + device.GetNumTracks();

            int tracks = device.GetNumTracks();
            int offset = 0;
            Win32API.CDROM_TOC TOC = device.GetTOC();

            for (int i = 0; i < tracks; i++)
            {
                offset = ((TOC.TrackData[i].Address_1 * 60) + TOC.TrackData[i].Address_2) * 75 + TOC.TrackData[i].Address_3;
                url += "+" + offset;
            }

            Win32API.TRACK_DATA first = TOC.TrackData[0];
            Win32API.TRACK_DATA last = TOC.TrackData[tracks];
            int length = ((last.Address_1 * 60) + last.Address_2) - ((first.Address_1 * 60) + first.Address_2);

            url += "+" + length;
            url += ServerAuth;

            string response = QueryPage(url).Trim();
            string[] lines = response.Split('\n');

            if (lines.Length > 0)
            {
                string[] status_line = lines[0].Split(' ');
                string code = status_line[0];

                switch (code)
                {
                    case "200": // Found exact match
                        {
                            CDInformation info = ParseLine(lines[0], true);
                            info = QueryDetails(info);
                            result.Add(info);

                            break;
                        }
                    case "211": // Found inexact matches
                    case "210": // Found exact matches
                        {
                            for (int i = 1; i < lines.Length - 1; i++)
                            {
                                string line = lines[i].Trim();
                                if (line == "." || line == "")
                                {
                                    continue;
                                }

                                CDInformation info = ParseLine(lines[i], false);
                                info = QueryDetails(info);
                                result.Add(info);
                            }
                            break;
                        }
                    case "202": // No match found
                    case "403": // Database entry is corrupt
                    case "409": // No handshake
                        {
                            return null;
                        }
                }
            }

            return result;
        }

        /// <summary>
        /// Website per HTTP abfragen
        /// </summary>
        /// <param name="url">URL</param>
        /// <returns>Inhalt</returns>
        protected static string QueryPage(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse) request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            StringBuilder result = new StringBuilder();
            byte[] buff = new byte[1024];

            while (true)
            {
                int count = responseStream.Read(buff, 0, buff.Length);

                if (count == 0)
                {
                    break;
                }

                string s = Encoding.ASCII.GetString(buff, 0, count);
                result.Append(s);
            }

            return result.ToString();
        }

        /// <summary>
        /// Antwort vom Server parsen
        /// </summary>
        /// <param name="line">Zeile</param>
        /// <param name="status">Ist Statuscode enthalten?</param>
        /// <returns>Informationen</returns>
        protected static CDInformation ParseLine(string line, bool status)
        {
            int offset = status ? 1 : 0;

            string[] status_line = line.Split(' ');

            CDInformation info = new CDInformation();
            info.Genre = status_line[offset];
            info.DiscID = status_line[offset + 1];

            string title = line.Substring(line.IndexOf(info.DiscID) + info.DiscID.Length + 1);
            string[] title_author = title.Split('/');
            if (title_author.Length == 2)
            {
                info.Artist = title_author[0].Trim();
                info.Title = title_author[1].Trim();
            }

            return info;
        }

        /// <summary>
        /// Details abfragen
        /// </summary>
        /// <param name="info">Informationen</param>
        /// <returns>Erweiterte Informationen</returns>
        protected static CDInformation QueryDetails(CDInformation info)
        {
            string url = Server + "?cmd=cddb+read+" + info.Genre + "+" + info.DiscID;
            url += ServerAuth;

            string response = QueryPage(url).Trim();
            string[] lines = response.Split('\n');

            if (lines.Length > 0)
            {
                string[] status_line = lines[0].Split(' ');
                string code = status_line[0];

                switch (code)
                {
                    case "210": // OK
                        {
                            for (int i = 1; i < lines.Length - 1; i++)
                            {
                                string line = lines[i].Trim();
                                if (line == "." || line == "" || line.StartsWith("#"))
                                {
                                    continue;
                                }

                                string[] pairs = line.Split('=');
                                if (pairs.Length == 2)
                                {
                                    string key = pairs[0].Trim();
                                    string value = pairs[1].Trim();

                                    switch (key.ToUpper())
                                    {
                                        case "DISCID":
                                        case "DTITLE":
                                            {
                                                break; // bereits in Datensatz
                                            }
                                        case "DYEAR":
                                            {
                                                try
                                                {
                                                    info.Year = int.Parse(value);
                                                }
                                                catch (Exception)
                                                {
                                                    
                                                }
                                                break;
                                            }
                                        case "DGENRE":
                                            {
                                                info.Genre = value;
                                                break;
                                            }
                                        case "EXTD":
                                            {
                                                info.ExtendedData = value;
                                                break;
                                            }
                                        case "PLAYORDER":
                                            {
                                                break; // kp what to do.
                                            }
                                        default:
                                            {
                                                if (key.StartsWith("TTITLE"))
                                                {
                                                    try
                                                    {
                                                        int track = int.Parse(key.Substring(6));
                                                        FileInformation file = new FileInformation();
                                                        file.SongTitle = value;
                                                        file.SongArtist = info.Artist;
                                                        file.SongAlbum = info.Title;
                                                        file.SongYear = info.Year;
                                                        file.SongGenre = info.Genre;
                                                        file.SongTrack = track + 1;
                                                        info.Titles.Add(file);
                                                    }
                                                    catch (Exception)
                                                    {

                                                    }
                                                }
                                                else if (key.StartsWith("EXTT"))
                                                {
                                                    try
                                                    {
                                                        int track = int.Parse(key.Substring(4));
                                                        var result = info.Titles.Where(x => x.SongTrack == track);
                                                        foreach (FileInformation obj in result)
                                                        {
                                                            obj.Comment = value;
                                                        }
                                                    }
                                                    catch (Exception)
                                                    {

                                                    }
                                                }
                                                break;
                                            }
                                    }
                                }
                            }

                            break;
                        }
                    case "401": // Specified CDDB entry not found
                    case "402": // Server error
                    case "403": // Database entry is corrupt
                    case "409": // No handshake
                        {
                            break;
                        }
                }
            }

            info.Titles.Sort((x,y) => x.SongTrack.CompareTo(y.SongTrack));

            return info;
        }

        public override string ToString()
        {
            return Title + " von " + Artist + (Year > 0 ? " (" + Year + ")" : "");
        }
    }
}
