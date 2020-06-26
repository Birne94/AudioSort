using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Audiosort.Codecs.CD;

namespace Audiosort
{
    public class CDDBCache
    {
        static CDDBCacheDataSet DataSet;
        public static List<CDInformation> Information;
        static string Path;

        public static void LoadData(string path)
        {
            Path = path;
            DataSet = new CDDBCacheDataSet();
            try
            {
                DataSet.ReadXml(path);
            }
            catch (Exception)
            { }
            Information = new List<CDInformation>();

            var cds = from row in DataSet.Discs
                      select row;
            foreach (var cd in cds)
            {
                try
                {
                    CDInformation info = new CDInformation();
                    info.DiscID = cd.disc_cddbid;
                    info.Title = cd.disc_title;
                    info.Artist = cd.disc_artist;
                    info.Year = cd.disc_year;
                    info.Genre = cd.disc_genre;
                    info.ExtendedData = cd.disc_extended_data;
                    info.Titles = new List<FileInformation>();

                    var titles = from title in DataSet.Titel
                                 where title.disc_id == cd.disc_id
                                 select title;
                    foreach (var title in titles)
                    {
                        FileInformation finfo = new FileInformation();
                        finfo.SongTitle = title.title_name;
                        finfo.SongArtist = title.title_artist;
                        finfo.SongAlbum = title.title_album;
                        finfo.SongYear = title.title_year;
                        finfo.SongGenre = title.title_genre;
                        finfo.SongTrack = title.title_track;
                        finfo.Comment = title.title_comment;
                        info.Titles.Add(finfo);
                    }

                    Information.Add(info);
                }
                catch (Exception)
                { }
            }
        }

        public static void Unload()
        {
            if (DataSet != null)
            {
                DataSet.WriteXml(Path);
                DataSet = null;
                Information.Clear();
                Information = null;
            }
        }

        public static List<CDInformation> Query(CDDevice device)
        {
            string id = device.GetCDDB1ID();

            var result = from CDInformation info in Information
                         where info.DiscID.ToLower() == id.ToLower()
                         select info;

            if (result.Count() > 0)
            {
                return result.ToList();
            }

            List<CDInformation> cdinfo = CDInformation.Query(device);

            foreach (CDInformation info in cdinfo)
            {
                var result3 = from row in Information
                              where row.DiscID.ToLower() == info.DiscID.ToLower()
                              select row;

                if (result3.Count() == 0)
                    Information.Add(info);

                var result2 = from row in DataSet.Discs
                              where row.disc_id.ToString().ToLower() == info.DiscID.ToLower()
                              select row;

                if (result2.Count() > 0)
                    continue;

                CDDBCacheDataSet.DiscsRow rowD = DataSet.Discs.NewDiscsRow();
                rowD.disc_cddbid = info.DiscID;
                rowD.disc_title = info.Title;
                rowD.disc_artist = info.Artist;
                rowD.disc_year = info.Year;
                rowD.disc_genre = info.Genre;
                rowD.disc_extended_data = info.ExtendedData;
                DataSet.Discs.AddDiscsRow(rowD);

                foreach (FileInformation finfo in info.Titles)
                {
                    CDDBCacheDataSet.TitelRow rowT = DataSet.Titel.NewTitelRow();
                    rowT.disc_id = rowD.disc_id;
                    rowT.title_name = finfo.SongTitle;
                    rowT.title_artist = finfo.SongArtist;
                    rowT.title_album = finfo.SongAlbum;
                    rowT.title_year = finfo.SongYear;
                    rowT.title_genre = finfo.SongGenre;
                    rowT.title_track = finfo.SongTrack;
                    rowT.title_comment = finfo.Comment;
                    DataSet.Titel.AddTitelRow(rowT);
                }
            }

            return cdinfo;
        }

        public static void Clear()
        {
            Information.Clear();
            DataSet = new CDDBCacheDataSet();
        }
    }
}
