using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Audiosort.Data
{
    public class DatabaseFiller
    {
        public static List<ID3Tag> ReadMusic(string directory)
        {
            List<ID3Tag> result = new List<ID3Tag>();
            foreach (string ext in Audiosort.Codecs.AudioStream.Formats)
            {
                result.AddRange(ReadMusic(directory, ext));
            }
            return result;
        }

        public static List<ID3Tag> ReadMusic(string directory, string extension)
        {
            List<ID3Tag> result = new List<ID3Tag>();
            string[] files = Directory.GetFiles(directory, "*." + extension, SearchOption.AllDirectories);

            foreach (string file in files)
            {
                try
                {
                    ID3Tag TagInfo = new ID3Tag(file);
                    result.Add(TagInfo);
                }
                catch (Exception)
                {
                    ID3Tag TagInfo = new ID3Tag(file, false);
                    string filename = Path.GetFileNameWithoutExtension(file);
                    string[] split = filename.Split(new char[] { '-' });
                    if (split.Length == 2)
                    {
                        string dings = split[0].Trim();
                        string title = split[1].Trim();
                        TagInfo.Frames.Add(ID3Frame.Create("TIT2", title));
                        TagInfo.Frames.Add(ID3Frame.Create("TPE1", dings));
                    }
                    else
                    {
                        TagInfo.Frames.Add(ID3Frame.Create("TIT2", filename));
                    }
                    result.Add(TagInfo);
                }
            }

            return result;
        }

        public static void ReadMusic(string directory, AudiosortDataset Dataset)
        {
            List<ID3Tag> tags = ReadMusic(directory);

            foreach (ID3Tag tag in tags)
            {
                AudiosortDataset.TitelRow titel;
                string songtitle = tag["TIT2"];
                var titelresult = from t in Dataset.Titel
                                  where (tag.Filename != "" && t.titel_pfad == tag.Filename) || t.titel_name.ToLower() == songtitle.ToLower()
                                  select t;

                if (titelresult.Count() > 0)
                {
                    titel = titelresult.ElementAt(0);
                }
                else
                {
                    titel = Dataset.Titel.NewTitelRow();
                }

                titel.titel_name = tag["TIT2"];
                try
                {
                    titel.titel_dauer = int.Parse(tag["TLEN"] == "" ? "0" : tag["TLEN"]);
                }
                catch (Exception)
                { }
                try
                {
                    titel.titel_track = int.Parse(tag["TRCK"] == "" ? "-1" : tag["TRCK"]);
                }
                catch (Exception)
                { }
                titel.titel_pfad = tag.Filename;
                try
                {
                    titel.titel_jahr = int.Parse(tag["TYER"] == "" ? "-1" : tag["TYER"]);
                }
                catch (Exception)
                { }
                titel.titel_bewertung = 0;

                string genre = tag["TCON"];
                var genre_result = from g in Dataset.Genre
                                   where g.genre_name.ToLower() == genre.ToLower()
                                   select g.genre_id;
                if (genre_result.Count() > 0)
                {
                    titel.titel_genre = genre_result.ElementAt(0);
                }
                else
                {
                    AudiosortDataset.GenreRow row = Dataset.Genre.NewGenreRow();
                    row.genre_name = genre;
                    Dataset.Genre.AddGenreRow(row);
                    titel.titel_genre = row.genre_id;
                }

                string interpret = tag["TPE1"];
                var interpret_result = from i in Dataset.Interpreten
                                       where i.interpret_name.ToLower() == interpret.ToLower()
                                       select i.interpret_id;
                if (interpret_result.Count() > 0)
                {
                    titel.titel_interpret = interpret_result.ElementAt(0);
                }
                else
                {
                    AudiosortDataset.InterpretenRow row = Dataset.Interpreten.NewInterpretenRow();
                    row.interpret_name = interpret;
                    Dataset.Interpreten.AddInterpretenRow(row);
                    titel.titel_interpret = row.interpret_id;
                }

                string album = tag["TALB"];
                var album_result = from a in Dataset.Album
                                   where a.album_name.ToLower() == album.ToLower()
                                   select a.album_id;
                if (album_result.Count() > 0)
                {
                    titel.titel_album = album_result.ElementAt(0);
                }
                else
                {
                    AudiosortDataset.AlbumRow row = Dataset.Album.NewAlbumRow();
                    row.album_name = album;
                    row.album_image = "";
                    row.album_interpret = titel.titel_interpret;
                    row.album_jahr = titel.titel_jahr;
                    Dataset.Album.AddAlbumRow(row);
                    titel.titel_album = row.album_id;
                }

                if (titelresult.Count() == 0)
                {
                    Dataset.Titel.AddTitelRow(titel);
                }
            }

            tags.Clear();
        }
    }
}
