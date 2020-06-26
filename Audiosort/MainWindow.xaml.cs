using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Audiosort.Codecs;
using Audiosort.Codecs.CD;
using Audiosort.Data;
using Audiosort.Plugins;
using ASStyle = Audiosort.Plugins.Style;
using WPFStyle = System.Windows.Style;

namespace Audiosort
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Config.Config Config;
        static AudiosortDataset Dataset = null;
        DispatcherTimer UpdateTimer;

        WaveOutput SoundOutput;
        static AudiosortDataset.PlaylistRow CurrentPlaylist = null;
        public static AudiosortDataset.TitelRow CurrentTitle = null;
        List<AudiosortDataset.PlaylistRow> PlaylistAddedRows = new List<AudiosortDataset.PlaylistRow>();
        List<AudiosortDataset.TitelRow> TitleAddedRows = new List<AudiosortDataset.TitelRow>();

        float lastPosition = 0.0f;
        bool randomPlay = false;

        Random Random = new Random();

        char[] CDDeviceLetters = Codecs.CD.CDDevice.EnumDevices();
        Codecs.CD.CDDevice[] CDDevices;

        /* Funktionen */

        public MainWindow()
        {
            InitializeComponent();

            UpdateTimer = new DispatcherTimer();
            UpdateTimer.Tick += updatePlayer;
            UpdateTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);

            Config = new Audiosort.Config.Config("audiosort.xml");

            PluginManager.SetWindow(this);
            string[] directories = Config["PluginDirectories"].Split(';');
            PluginManager.Directories.AddRange(directories);
            PluginManager.Search();

            foreach (ASStyle style in PluginManager.Styles)
            {
                MenuItem mi = new MenuItem();
                mi.Icon = new RadioButton();
                (mi.Icon as RadioButton).GroupName = "MenuItemOptionsStyleGroup";
                (mi.Icon as RadioButton).IsHitTestVisible = false;
                mi.Header = style.Name;
                if (style.Name == Config["DefaultStyle"])
                    (mi.Icon as RadioButton).IsChecked = true;
                mi.Click += new RoutedEventHandler(ChangeStyle);
                MenuItemOptionsStyle.Items.Add(mi);
            }

            PluginManager.InvokeStyle(Config["DefaultStyle"]);

            string[] positions = Config["PlaylistColumnPositions"].Split(';');
            string[] widths = Config["PlaylistColumnWidths"].Split(';');
            for (int i = 0; i < Math.Min(Playlist.Columns.Count, positions.Length); i++)
            {
                Playlist.Columns[i].DisplayIndex = int.Parse(positions[i]);
                if (widths[i] != "*")
                {
                    Playlist.Columns[i].Width = new DataGridLength(double.Parse(widths[i].TrimEnd(new char[] { '*' })), DataGridLengthUnitType.Star);
                }
            }

            LoadSort();

            if (Config["PlayRandom"] == "1")
                randomPlay = true;
            else
                randomPlay = false;
            MenuItemViewRandom.IsChecked = randomPlay;

            if (Config["PlayLooping"] == "1")
                MenuItemViewLooping.IsChecked = true;
            else
                MenuItemViewLooping.IsChecked = false;

            Dataset = new AudiosortDataset();
            try
            {
                Dataset.ReadXml(Config["DatabasePath"]);
            }
            catch (Exception)
            {
                try
                {
                    if (Dataset.Interpreten.Count == 0)
                    {
                        AudiosortDataset.InterpretenRow rowi = Dataset.Interpreten.NewInterpretenRow();
                        rowi.interpret_id = -1;
                        rowi.interpret_name = "";
                        Dataset.Interpreten.AddInterpretenRow(rowi);
                    }

                    if (Dataset.Album.Count == 0)
                    {
                        AudiosortDataset.AlbumRow rowa = Dataset.Album.NewAlbumRow();
                        rowa.album_id = -1;
                        rowa.album_name = "";
                        rowa.album_image = "";
                        rowa.album_jahr = -1;
                        rowa.album_interpret = -1;
                        Dataset.Album.AddAlbumRow(rowa);
                    }

                    if (Dataset.Genre.Count == 0)
                    {
                        AudiosortDataset.GenreRow rowg = Dataset.Genre.NewGenreRow();
                        rowg.genre_id = -1;
                        rowg.genre_name = "";
                        Dataset.Genre.AddGenreRow(rowg);
                    }
                }
                catch (Exception)
                { }
            }

            //Playlist.AddHandler(UIElement.MouseLeftButtonDownEvent,
            //    new MouseButtonEventHandler(Playlist_MouseLeftButtonDown), true);
            //PlaylistList.AddHandler(UIElement.MouseLeftButtonDownEvent,
            //    new MouseButtonEventHandler(PlaylistList_MouseLeftButtonUp), true);

            CDDevices = new Codecs.CD.CDDevice[CDDeviceLetters.Length];
            for (int i = 0; i < CDDeviceLetters.Length; i++)
            {
                CDDevices[i] = new Codecs.CD.CDDevice(CDDeviceLetters[i], false);
            }

            try
            {
                CDDBCache.LoadData("cddb.xml");
            }
            catch (Exception)
            {
                Console.Write("");
            }
        }

        void ChangeStyle(object sender, RoutedEventArgs e)
        {
            MenuItem mi = sender as MenuItem;

            if (mi == null)
                return;
            if (mi.Icon == null)
                return;

            ASStyle style = PluginManager.GetStyle(mi.Header as string);
            if (style != null)
            {
                try
                {
                    PluginManager.InvokeStyle(style);
                    (mi.Icon as RadioButton).IsChecked = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Fehler beim Laden des Styles!\n\n" + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Fehler beim Laden des Styles!");
            }
        }

        void FillDatabase(int playlist)
        {
            SaveSort();
            if (playlist == ID.PlaylistAll)
            {
                var result = from t in Dataset.Titel
                             join i in Dataset.Interpreten on t.titel_interpret equals i.interpret_id
                             join a in Dataset.Album on t.titel_album equals a.album_id
                             join g in Dataset.Genre on t.titel_genre equals g.genre_id
                             where (t.titel_bewertung >= 0 && !t.titel_pfad.StartsWith("cd"))
                             select new { t, i, a, g };
                Playlist.DataContext = result;
            }
            else if (ID.isCD(playlist))
            {
                int n = -ID.PlaylistCD(-playlist);
                Codecs.CD.CDDevice cd = CDDevices[n];
                try
                {
                    cd.ReadTOC();
                }
                catch (Codecs.CD.CDException)
                {
                    MessageBox.Show("Bitte legen Sie eine gültige Audio-CD ein!");
                    return;
                }
                catch
                {
                    MessageBox.Show("Fehler beim Zugriff auf das CD-Laufwerk!");
                    return;
                }

                RemoveTitleRows();

                int track_count = cd.GetNumAudioTracks();
                for (int i = 1; i <= track_count; i++)
                {
                    titleAddRow(i, CDDeviceLetters[n], "Track " + i);
                }
                var result = from t in Dataset.Titel
                             join i in Dataset.Interpreten on t.titel_interpret equals i.interpret_id
                             join a in Dataset.Album on t.titel_album equals a.album_id
                             join g in Dataset.Genre on t.titel_genre equals g.genre_id
                             where t.titel_pfad.StartsWith("cd://" + CDDeviceLetters[n] + "/")
                             select new { t, i, a, g };
                Playlist.DataContext = result;
            }
            else
            {
                var result = from t in Dataset.Titel
                             join i in Dataset.Interpreten on t.titel_interpret equals i.interpret_id
                             join a in Dataset.Album on t.titel_album equals a.album_id
                             join g in Dataset.Genre on t.titel_genre equals g.genre_id
                             join p in Dataset.PlaylistEntry on t.titel_id equals p.titel_id
                             where (p.playlist_id == playlist && t.titel_bewertung >= 0 && !t.titel_pfad.StartsWith("cd"))
                             select new { t, i, a, g };
                Playlist.DataContext = result;
            }

            LoadSort();
        }

        void FillDatabase()
        {
            FillDatabase(ID.PlaylistAll);

            if (PlaylistAddedRows != null)
            {
                foreach (AudiosortDataset.PlaylistRow row in PlaylistAddedRows)
                {
                    Dataset.Playlist.RemovePlaylistRow(row);
                }
                PlaylistAddedRows.Clear();
            }

            playlistAddRow(ID.PlaylistAll, "Alle Titel");
            for (int i = 0; i < CDDeviceLetters.Length; i++)
            {
                playlistAddRow(ID.PlaylistCD(i), "CD: " + CDDeviceLetters[i]);
            }
            updatePlaylistList();
        }

        void updatePlaylistList()
        {
            var playlist = from p in Dataset.Playlist
                           select p;

            PlaylistList.DataContext = playlist.OrderBy(p => p, new PlaylistComparer());
        }

        private void InitSound()
        {
            IntPtr handle = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            SoundOutput = new SharpDXOutput(handle);
            SoundOutput.Initialized += outputInitialized;

            int playerVolume;
            try
            {
                playerVolume = int.Parse(Config["PlayerVolume"]);
                if (playerVolume > 100)
                    playerVolume = 100;
                if (playerVolume < 0)
                    playerVolume = 0;
            }
            catch (Exception)
            {
                playerVolume = 100;
            }
            SoundOutput.Volume = playerVolume;
            SoundOutput.Loop = MenuItemViewLooping.IsChecked;
        }

        private void LoadSound(string url)
        {
            Playlist.UpdateLayout();
            try
            {
                if (SoundOutput == null)
                {
                    InitSound();
                }
                if (SoundOutput.isPlaying())
                {
                    SoundOutput.Stop();
                    SoundOutput.AudioStream.Close();
                }

                AudioStream s;
                if (url.StartsWith("cd://"))
                {
                    int index = Array.IndexOf(CDDeviceLetters, url[5]);
                    s = new CDStream(CDDevices[index], int.Parse(url.Substring(12)));
                }
                else
                {
                    s = AudioStream.Load(url);
                }

                if (s != null)
                {
                    SoundOutput.PrepareBuffer(s);
                }
            }
            catch (CDException ex)
            {
                MessageBox.Show("Fehler beim Zugriff auf die CD\n\n" + ex.ToString());
            }
            catch (System.IO.IOException)
            {
                MessageBoxResult res = MessageBox.Show("Auf die Datei " + url + " konnte nicht zugegriffen werden!\n\nSoll die Datei aus der Musikbibliothek entfernt werden?", "", MessageBoxButton.YesNo);
                if (res == MessageBoxResult.Yes)
                {
                    if (!url.StartsWith("cd://"))
                    {
                        var result = from titel in Dataset.Titel
                                     where titel.titel_pfad == url
                                     select titel;

                        if (result.Count() > 0)
                        {
                            List<AudiosortDataset.TitelRow> rows = new List<AudiosortDataset.TitelRow>();
                            foreach (var row in result)
                            {
                                rows.Add(row);
                            }
                            foreach (var row in rows)
                            {
                                RemoveTitle(row);
                            }
                        }

                        FillDatabase();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bei der Wiedergabe der Musikdatei ist ein Fehler aufgetreten!\n\n" + ex.ToString());
            }

        }


        void goNext()
        {
            if (CurrentPlaylist == null || CurrentPlaylist.playlist_id == -1)
            {
                if (randomPlay)
                {
                    CurrentTitle = Dataset.Titel.Rows[Random.Next(Dataset.Titel.Rows.Count)] as AudiosortDataset.TitelRow;
                }
                else
                {
                    int index = Dataset.Titel.Rows.IndexOf(CurrentTitle);
                    if (index < Dataset.Titel.Rows.Count - 1)
                    {
                        CurrentTitle = Dataset.Titel.Rows[index + 1] as AudiosortDataset.TitelRow;
                    }
                    else
                    {
                        CurrentTitle = Dataset.Titel.Rows[0] as AudiosortDataset.TitelRow;
                    }
                }
                if (CurrentTitle != null)
                {
                    LoadSound(CurrentTitle.titel_pfad);
                    SoundOutput.Play();
                }
            }
            else if (ID.isCD(CurrentPlaylist.playlist_id))
            {
                int track = int.Parse(CurrentTitle.titel_pfad.Substring(12));
                int device = Array.IndexOf(CDDeviceLetters, CurrentTitle.titel_pfad[5]);
                int trackCount = CDDevices[device].GetNumAudioTracks();

                if (randomPlay)
                {
                    track = Random.Next(trackCount) + 1;
                }
                else
                {
                    if (track >= trackCount)
                        track = 0;
                    track++;
                }

                var result = from t in Dataset.Titel
                             where t.titel_pfad == "cd://" + CurrentTitle.titel_pfad[5] + "/Track" + track
                             select t;
                if (result.Count() > 0)
                {
                    CurrentTitle = result.ElementAt(0);
                    LoadSound(CurrentTitle.titel_pfad);
                    SoundOutput.Play();
                }
            }
            else
            {
                var result = from t in Dataset.Titel
                             join p in Dataset.PlaylistEntry on t.titel_id equals p.titel_id
                             where p.playlist_id == CurrentPlaylist.playlist_id
                             select t;

                int index = Array.IndexOf(result.ToArray(), CurrentTitle);
                if (randomPlay)
                {
                    index = Random.Next(result.Count());
                }
                else
                {
                    if (index >= result.Count() - 1)
                    {
                        index = 0;
                    }
                    else
                    {
                        index++;
                    }
                }

                CurrentTitle = result.ElementAt(index);
                LoadSound(CurrentTitle.titel_pfad);
                SoundOutput.Play();
            }
        }

        void goPrev()
        {
            if (CurrentPlaylist == null || CurrentPlaylist.playlist_id == -1)
            {
                if (randomPlay)
                {
                    CurrentTitle = Dataset.Titel.Rows[Random.Next(Dataset.Titel.Rows.Count)] as AudiosortDataset.TitelRow;
                }
                else
                {
                    int index = Dataset.Titel.Rows.IndexOf(CurrentTitle);
                    if (index > 0)
                    {
                        CurrentTitle = Dataset.Titel.Rows[index - 1] as AudiosortDataset.TitelRow;
                    }
                    else
                    {
                        CurrentTitle = Dataset.Titel.Rows[Dataset.Titel.Rows.Count - 1] as AudiosortDataset.TitelRow;
                    }
                }
                if (CurrentTitle != null)
                {
                    LoadSound(CurrentTitle.titel_pfad);
                    SoundOutput.Play();
                }
            }
            else if (ID.isCD(CurrentPlaylist.playlist_id))
            {
                int track = int.Parse(CurrentTitle.titel_pfad.Substring(12));
                int device = Array.IndexOf(CDDeviceLetters, CurrentTitle.titel_pfad[5]);
                int trackCount = CDDevices[device].GetNumAudioTracks();

                if (randomPlay)
                {
                    track = Random.Next(trackCount) + 1;
                }
                else
                {
                    if (track <= 1)
                        track = trackCount + 1;
                    track--;
                }
                var result = from t in Dataset.Titel
                             where t.titel_pfad == "cd://" + CurrentTitle.titel_pfad[5] + "/Track" + track
                             select t;
                if (result.Count() > 0)
                {
                    CurrentTitle = result.ElementAt(0);
                    LoadSound(CurrentTitle.titel_pfad);
                    SoundOutput.Play();
                }
            }
            else
            {
                var result = from t in Dataset.Titel
                             join p in Dataset.PlaylistEntry on t.titel_id equals p.titel_id
                             where p.playlist_id == CurrentPlaylist.playlist_id
                             select t;

                int index = Array.IndexOf(result.ToArray(), CurrentTitle);
                if (randomPlay)
                {
                    index = Random.Next(result.Count());
                }
                else
                {
                    if (index <= 0)
                    {
                        index = result.Count() - 1;
                    }
                    else
                    {
                        index--;
                    }
                }

                CurrentTitle = result.ElementAt(index);
                LoadSound(CurrentTitle.titel_pfad);
                SoundOutput.Play();
            }
        }

        /* Player-Steuerung */

        void updatePlayer(object sender, EventArgs e)
        {
            try
            {
                if (SoundOutput != null)
                {
                    if (CurrentTitle != null)
                    {
                        Title = "Audiosort - Now Playing: " + CurrentTitle.titel_name;
                    }
                    else
                    {
                        Title = "Audiosort";
                    }

                    if (SoundOutput.isPlaying())
                    {
                        lastPosition = SoundOutput.Position;
                        ButtonPlay.Style = TryFindResource("ButtonPause") as WPFStyle;
                    }
                    else
                    {
                        ButtonPlay.Style = TryFindResource("ButtonPlay") as WPFStyle;
                    }
                    PlayPositionSlider.Value = SoundOutput.Position * PlayPositionSlider.Maximum;

                    int secs = (int)(SoundOutput.Position * SoundOutput.WaveFormat.BufferLength / SoundOutput.WaveFormat.BytesPerSecond);
                    int mins = secs / 60;
                    secs %= 60;
                    PlayPositionSlider.ToolTip = mins + ":" + secs.ToString("00");

                    // Wiedergabe gerade beendet
                    if (!SoundOutput.isPlaying() && SoundOutput.Position == 0.0f && lastPosition > 0.0f)
                    {
                        lastPosition = 0.0f;
                        if (SoundOutput.Loop)
                            SoundOutput.Play();
                        else
                        {
                            goNext();
                        }
                    }

                    int volumeValue = (int)MenuItemViewVolume.Value;
                    SoundOutput.Volume = volumeValue;
                }
            }
            catch (Exception)
            { }
        }

        void outputInitialized(object sender, EventArgs e)
        {
            if (Config["PlayLooping"] == "1")
                SoundOutput.Loop = true;
            else
                SoundOutput.Loop = false;
            MenuItemViewLooping.IsChecked = SoundOutput.Loop;

            UpdateTimer.Start();
        }

        /* WPF-Events */

        protected override void OnSourceInitialized(EventArgs e)
        {
            FillDatabase();
            InitSound();

            base.OnSourceInitialized(e);
        }

        private void MenuItemFileExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Spaltenbreite und -position speichern
            string column_positions = "";
            string column_widths = "";
            foreach (DataGridColumn col in Playlist.Columns)
            {
                column_positions += ";" + col.DisplayIndex;
                column_widths += ";" + col.Width.Value;
            }

            Config["PlaylistColumnPositions"] = column_positions.Trim(new char[] { ';' });
            Config["PlaylistColumnWidths"] = column_widths.Trim(new char[] { ';' });
            if (SoundOutput != null)
            {
                Config["PlayerVolume"] = SoundOutput.Volume.ToString();
            }

            SaveSort();

            // sonstige Einstellungen
            Config["PlayRandom"] = randomPlay ? "1" : "0";
            Config["PlayLooping"] = SoundOutput.Loop ? "1" : "0";

            // aktuellen Style speichern
            foreach (MenuItem mi in MenuItemOptionsStyle.Items)
            {
                RadioButton rb = mi.Icon as RadioButton;
                if (rb != null)
                    if ((bool)rb.IsChecked)
                        Config["DefaultStyle"] = mi.Header as string;
            }

            Config.Save();

            RemoveRows();

            // Backup database
            string backupFile = Path.GetDirectoryName(Config["DatabasePath"]) + Path.GetFileNameWithoutExtension(Config["DatabasePath"]) + ".bak" + Path.GetExtension(Config["DatabasePath"]);
            File.Copy(Config["DatabasePath"], backupFile, true);

            // Save database
            XslXmlTextWriter w = new XslXmlTextWriter(Config["DatabasePath"], Encoding.UTF8, "audiosort-dataset.xsl");
            w.Formatting = System.Xml.Formatting.Indented;
            Dataset.WriteXml(w);

            CDDBCache.Unload();
        }

        private void MenuItemHelp_Click(object sender, RoutedEventArgs e)
        {
            new AboutDialog().ShowDialog();
        }

        private void MenuItemToolRefresh_Click(object sender, RoutedEventArgs e)
        {
            string directory = Config["ImportDirectory"];
            if (directory == "")
            {
                directory = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            }
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.SelectedPath = directory;
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                directory = dialog.SelectedPath;
                Config["ImportDirectory"] = directory;
            }
            else
            {
                return;
            }

            DatabaseFiller.ReadMusic(directory, Dataset);
            FillDatabase(ID.PlaylistAll);
        }

        void ButtonBack_Click(object sender, EventArgs e)
        {
            goPrev();
        }

        private void ButtonPlay_Click(object sender, RoutedEventArgs e)
        {
            if (SoundOutput != null)
            {
                if (CurrentTitle != null)
                {
                    if (!SoundOutput.isPlaying())
                    {
                        SoundOutput.Resume();
                        ButtonPlay.Style = (WPFStyle)FindResource("ButtonPause");
                    }
                    else
                    {
                        SoundOutput.Pause();
                        ButtonPlay.Style = (WPFStyle)FindResource("ButtonPlay");
                    }
                }
                else
                {
                    var row = Playlist.SelectedItem;
                    var titel = GetProperty<AudiosortDataset.TitelRow>(row, "t");

                    if (titel != null)
                    {
                        CurrentTitle = titel;
                        LoadSound(titel.titel_pfad);
                        SoundOutput.Play();
                    }
                }
            }
        }

        private void ButtonPause_Click(object sender, EventArgs e)
        {
            if (SoundOutput != null)
            {
                lastPosition = 0.0f;
                SoundOutput.Stop();
            }
        }

        void ButtonNext_Click(object sender, EventArgs e)
        {
            goNext();
        }

        private void PlayPositionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (SoundOutput != null)
            {
                //SoundOutput.Position = (float)(PlayPositionSlider.Value / PlayPositionSlider.Maximum);
            }
        }

        private void Playlist_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var row = Playlist.SelectedItem;
            if (row == null)
                return;

            var titel = GetProperty<AudiosortDataset.TitelRow>(row, "t");
            if (titel == null)
                return;

            string url = titel.titel_pfad;

            if (url.Length > 0)
            {
                CurrentTitle = titel;
                LoadSound(url);
                SoundOutput.Play();
            }
        }

        private void PlayPositionSlider_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void PlaylistList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isDragging)
            {
                isDragging = false;
            }
            else
            {
                PlaylistList_SelectionChanged(sender, null);
            }
        }

        private void PlaylistList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var row = PlaylistList.SelectedItem;
            if (row == null)
                return;

            int id = GetProperty<int>(row, "playlist_id");

            CurrentPlaylist = (AudiosortDataset.PlaylistRow)row;

            FillDatabase(id);
        }

        private void PlaylistListContextMenu_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ContextMenu menu = sender as ContextMenu;
            if (menu == null)
                return;

            Point p = Mouse.GetPosition(PlaylistList);
            var x = UIHelper.TryFindFromPoint<DataGridRow>(PlaylistList, p);
            if (x == null)
                return;

            AudiosortDataset.PlaylistRow row = x.Item as AudiosortDataset.PlaylistRow;
            if (menu == null || row == null)
                return;


            if (ID.isCD(row.playlist_id))
            {
                (menu.Items[0] as MenuItem).Visibility = Visibility.Visible;
            }
            else
            {
                (menu.Items[0] as MenuItem).Visibility = Visibility.Hidden;
            }
        }

        private void MenuItemViewRandom_Checked(object sender, RoutedEventArgs e)
        {
            randomPlay = true;
        }

        private void MenuItemViewRandom_Unchecked(object sender, RoutedEventArgs e)
        {
            randomPlay = false;
        }

        private void MenuItemViewLooping_Checked(object sender, RoutedEventArgs e)
        {
            if (SoundOutput != null)
                SoundOutput.Loop = true;
        }

        private void MenuItemViewLooping_Unchecked(object sender, RoutedEventArgs e)
        {
            if (SoundOutput != null)
                SoundOutput.Loop = false;
        }

        private void MenuItemOptionsDatabaseClearCache_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Cache wirklich leeren?", "Audiosort", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                CDDBCache.Clear();
            }
        }

        private void MenuItemViewVolume_Change(object sender, EventArgs e)
        {
            int i = (int)MenuItemViewVolume.Value;
            MessageBox.Show("Wert: " + i);
        }

        private void PlaylistList_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            AudiosortDataset.PlaylistRow row = PlaylistList.SelectedItem as AudiosortDataset.PlaylistRow;
            if (row == null)
            {
                PlaylistList.ContextMenu.Visibility = Visibility.Hidden;
                return;
            }

            PlaylistList.ContextMenu.Items.Clear();
            addMenuItem(PlaylistList.ContextMenu, "Wiedergabe", contextPlaylistPlay, "ContextButtonPlay");
            if (row.playlist_id > ID.PlaylistAll)
            {
                addMenuItem(PlaylistList.ContextMenu, "Umbenennen", contextPlaylistRename, "ContextButtonRename");
                addMenuItem(PlaylistList.ContextMenu, "Entfernen", contextPlaylistRemove, "ContextButtonDelete");
            }
            else if (ID.isCD(row.playlist_id))
            {
                addMenuItem(PlaylistList.ContextMenu, "Auswerfen", contextPlaylistEject, "ContextButtonEject");
                addMenuItem(PlaylistList.ContextMenu, "Informationen abfragen", contextPlaylistGetCDInformation, "ContextButtonGetCDInfo");
            }
            PlaylistList.ContextMenu.Items.Add(new Separator());
            addMenuItem(PlaylistList.ContextMenu, "Neue Abspielliste", contextPlaylistNew, "ContextButtonNew");
        }

        void Playlist_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var row = Playlist.SelectedItem;
            if (row == null)
                return;

            var titel = GetProperty<AudiosortDataset.TitelRow>(row, "t");
            if (titel == null)
                return;

            Playlist.ContextMenu.Items.Clear();
            if (CurrentTitle == titel && SoundOutput.isPlaying())
                addMenuItem(Playlist.ContextMenu, "Anhalten", contextPause, "ContextButtonPause");
            else
                addMenuItem(Playlist.ContextMenu, "Wiedergabe", contextPlay, "ContextButtonPlay");
            addMenuItem(Playlist.ContextMenu, "Nächster Titel", ButtonNext_Click, "ContextButtonForward");
            addMenuItem(Playlist.ContextMenu, "Vorheriger Titel", ButtonBack_Click, "ContextButtonBack");
            
            if (CurrentPlaylist == null || !ID.isCD(CurrentPlaylist.playlist_id))
            {
                Playlist.ContextMenu.Items.Add(new Separator());

                MenuItem rate = addMenuItem(Playlist.ContextMenu, "Bewertung", null, "ContextButtonRating");
                for (int i = 0; i <= 5; i++)
                {
                    addMenuItem(rate, "", contextPlaylistRate, "ContextButtonRating" + i);
                }

                addMenuItem(Playlist.ContextMenu, "Dateipfad öffnen", contextPlaylistOpenPath, "ContextButtonOpenPath");

                Playlist.ContextMenu.Items.Add(new Separator());

                var result = from p in Dataset.Playlist
                             where p.playlist_id > ID.PlaylistAll
                             select p;

                MenuItem add = addMenuItem(Playlist.ContextMenu, "Zu Abspielliste hinzufügen", null, "ContextButtonPlaylistAdd");
                addMenuItem(add, "Neue Abspielliste", contextPlaylistAdd, "ContextButtonNew");
                foreach (AudiosortDataset.PlaylistRow playlist in result)
                {
                    var result2 = from p in Dataset.PlaylistEntry
                                  where p.titel_id == titel.titel_id && p.playlist_id == playlist.playlist_id
                                  select p;
                    if (result2.Count() == 0)
                        addMenuItem(add, playlist.playlist_name, contextPlaylistAdd, "ContextButtonPlaylistAddSub");
                }

                addMenuItem(Playlist.ContextMenu, "Aus Bibliothek entfernen", contextPlaylistRemoveFull, "ContextButtonDeleteFull");
                addMenuItem(Playlist.ContextMenu, "Bearbeiten", contextPlaylistEdit, "ContextButtonEdit");
            }
            if (CurrentPlaylist != null && CurrentPlaylist.playlist_id > ID.PlaylistAll)
            {
                addMenuItem(Playlist.ContextMenu, "Aus Abspielliste entfernen: " + CurrentPlaylist.playlist_name, contextPlaylistRemoveItem, "ContextButtonDelete");
            }
        }

        void contextPlaylistRate(object sender, EventArgs e)
        {
            MenuItem mi = sender as MenuItem;
            if (mi == null)
                return;

            foreach (var row in Playlist.SelectedItems)
            {
                if (row == null)
                    continue;

                var titel = GetProperty<AudiosortDataset.TitelRow>(row, "t");
                if (titel == null)
                    continue;

                try
                {
                    int r = mi.Name[mi.Name.Length - 1] - 48;
                    titel.titel_bewertung = r;
                }
                catch (Exception)
                { }
            }
            Playlist.Items.Refresh();
        }

        void contextPlaylistOpenPath(object sender, EventArgs e)
        {
            var row = Playlist.SelectedItem;
            if (row == null)
                return;

            var titel = GetProperty<AudiosortDataset.TitelRow>(row, "t");
            if (titel == null)
                return;

            if (titel.titel_pfad == "")
                return;

            System.Diagnostics.Process.Start(
                "explorer.exe",
                "/select, " + titel.titel_pfad
                );
        }

        void contextPlaylistEdit(object sender, EventArgs e)
        {
            var row = Playlist.SelectedItem;
            if (row == null)
                return;

            var titel = GetProperty<AudiosortDataset.TitelRow>(row, "t");
            if (titel == null)
                return;

            EditWindow window = new EditWindow(titel, Dataset);
            window.ShowDialog();

            Playlist.Items.Refresh();
        }

        void contextPlaylistRemoveFull(object sender, EventArgs e)
        {
            foreach (var row in Playlist.SelectedItems)
            {
                if (row == null)
                    continue;

                var titel = GetProperty<AudiosortDataset.TitelRow>(row, "t");
                if (titel == null)
                    continue;

                RemoveTitle(titel);
            }

            FillDatabase();
        }

        void contextPlaylistEject(object sender, EventArgs e)
        {
            AudiosortDataset.PlaylistRow row = PlaylistList.SelectedItem as AudiosortDataset.PlaylistRow;
            if (row == null)
                return;

            if (!ID.isCD(row.playlist_id))
                return;

            char c = row.playlist_name[4];
            int index = Array.IndexOf(CDDeviceLetters, c);
            if (index >= 0)
            {
                CDDevices[index].EjectCD();
            }
        }

        void contextPlaylistGetCDInformation(object sender, EventArgs e)
        {
            AudiosortDataset.PlaylistRow row = PlaylistList.SelectedItem as AudiosortDataset.PlaylistRow;
            if (row == null)
                return;

            if (!ID.isCD(row.playlist_id))
                return;

            char c = row.playlist_name[4];
            int index = Array.IndexOf(CDDeviceLetters, c);
            if (index >= 0)
            {
                List<CDInformation> info = CDDBCache.Query(CDDevices[index]);
                if (info == null || info.Count == 0)
                {
                    MessageBox.Show("Keine Informationen gefunden!");
                }
                else
                {
                    CDInformation current = null;
                    if (info.Count == 1)
                    {
                        current = info[0];
                    }
                    else
                    {
                        CDInfoChoice window = new CDInfoChoice(info);
                        window.ShowDialog();
                        if (window.Result != null)
                        {
                            current = window.Result;
                        }
                    }
                    if (current != null)
                    {
                        //var titles = from t in Dataset.Titel
                        //             join r in Dataset.PlaylistEntry on t.titel_id equals r.titel_id
                        //             where r.playlist_id == row.playlist_id
                        //             select t;
                        var titles = from t in Dataset.Titel
                                     where t.titel_pfad.StartsWith("cd://" + CDDeviceLetters[index] + "/Track")
                                     select t;

                        // "cd://" + c + "/Track" + index;

                        foreach (var title in titles)
                        {
                            FileInformation finfo = current.Titles[title.titel_track - 1];
                            title.titel_name = finfo.SongTitle;
                            title.titel_jahr = finfo.SongYear;

                            var interpret = from i in Dataset.Interpreten
                                            where i.interpret_name.ToLower() == finfo.SongArtist.ToLower()
                                            select i;

                            if (interpret.Count() > 0)
                            {
                                title.titel_interpret = interpret.ElementAt(0).interpret_id;
                            }
                            else
                            {
                                var newRow = Dataset.Interpreten.NewInterpretenRow();
                                newRow.interpret_name = finfo.SongArtist;
                                Dataset.Interpreten.AddInterpretenRow(newRow);
                                title.titel_interpret = newRow.interpret_id;
                            }

                            var album = from a in Dataset.Album
                                        where a.album_name.ToLower() == finfo.SongAlbum.ToLower()
                                        select a;

                            if (album.Count() > 0)
                            {
                                title.titel_album = album.ElementAt(0).album_id;
                            }
                            else
                            {
                                var newRow = Dataset.Album.NewAlbumRow();
                                newRow.album_interpret = title.titel_interpret;
                                newRow.album_jahr = current.Year;
                                newRow.album_name = current.Title;
                                Dataset.Album.AddAlbumRow(newRow);
                                title.titel_album = newRow.album_id;
                            }

                            var genre = from g in Dataset.Genre
                                        where g.genre_name.ToLower() == finfo.SongGenre.ToLower()
                                        select g;

                            if (genre.Count() > 0)
                            {
                                title.titel_genre = genre.ElementAt(0).genre_id;
                            }
                            else
                            {
                                var newRow = Dataset.Genre.NewGenreRow();
                                newRow.genre_name = finfo.SongGenre;
                                Dataset.Genre.AddGenreRow(newRow);
                                title.titel_genre = newRow.genre_id;
                            }
                        }

                        var result = from t in Dataset.Titel
                                     join i in Dataset.Interpreten on t.titel_interpret equals i.interpret_id
                                     join a in Dataset.Album on t.titel_album equals a.album_id
                                     join g in Dataset.Genre on t.titel_genre equals g.genre_id
                                     where t.titel_pfad.StartsWith("cd://" + CDDeviceLetters[index] + "/")
                                     select new { t, i, a, g };
                        Playlist.DataContext = result;
                    }
                }
            }
        }

        void contextPlaylistNew(object sender, EventArgs e)
        {
            StringEntryDialog dialog = new StringEntryDialog();
            if (dialog.ShowDialog() == true)
            {
                AudiosortDataset.PlaylistRow row = Dataset.Playlist.NewPlaylistRow();
                row.playlist_name = dialog.ResponseText;
                Dataset.Playlist.AddPlaylistRow(row);
                updatePlaylistList();
            }
        }

        void contextPlaylistRemove(object sender, EventArgs e)
        {
            AudiosortDataset.PlaylistRow row = PlaylistList.SelectedItem as AudiosortDataset.PlaylistRow;
            if (row == null)
                return;

            var result = from p in Dataset.PlaylistEntry
                         where p.playlist_id == row.playlist_id
                         select p;

            foreach (AudiosortDataset.PlaylistEntryRow r in result.ToArray())
            {
                Dataset.PlaylistEntry.RemovePlaylistEntryRow(r);
            }

            Dataset.Playlist.RemovePlaylistRow(row);
            updatePlaylistList();
        }

        void contextPlaylistPlay(object sender, EventArgs e)
        {
            AudiosortDataset.PlaylistRow row = PlaylistList.SelectedItem as AudiosortDataset.PlaylistRow;
            if (row == null)
                return;

            CurrentPlaylist = row;
            FillDatabase(row.playlist_id);

            var result = from t in Dataset.Titel
                         join p in Dataset.PlaylistEntry on t.titel_id equals p.titel_id
                         where p.playlist_id == row.playlist_id
                         select t;

            if (result.Count() > 0)
            {
                AudiosortDataset.TitelRow titel;
                if (randomPlay)
                    titel = result.ElementAt(Random.Next(result.Count()));
                else
                    titel = result.ElementAt(0);
                CurrentTitle = titel;
                LoadSound(titel.titel_pfad);
                SoundOutput.Play();
            }
        }

        void contextPlaylistRename(object sender, EventArgs e)
        {
            AudiosortDataset.PlaylistRow row = PlaylistList.SelectedItem as AudiosortDataset.PlaylistRow;
            if (row == null)
                return;

            StringEntryDialog dialog = new StringEntryDialog();
            dialog.ResponseText = row.playlist_name;
            if (dialog.ShowDialog() == true)
            {
                row.playlist_name = dialog.ResponseText;
            }
            updatePlaylistList();
        }

        void contextPlaylistRemoveItem(object sender, EventArgs e)
        {
            foreach (var row in Playlist.SelectedItems)
            {
                if (row == null)
                    continue;

                var titel = GetProperty<AudiosortDataset.TitelRow>(row, "t");
                if (titel == null)
                    continue;

                string name;
                try
                {
                    name = (sender as MenuItem).Header.ToString().Substring(28);
                }
                catch (ArgumentOutOfRangeException)
                {
                    continue;
                }


                var result = from p in Dataset.Playlist
                             where p.playlist_name == name
                             select p;

                if (result.Count() > 0)
                {
                    AudiosortDataset.PlaylistRow playlist = result.ElementAt(0);
                    var result2 = from p in Dataset.PlaylistEntry
                                  where p.playlist_id == playlist.playlist_id && p.titel_id == titel.titel_id
                                  select p;
                    if (result2.Count() == 0)
                        return;

                    foreach (AudiosortDataset.PlaylistEntryRow r in result2.ToArray())
                    {
                        Dataset.PlaylistEntry.RemovePlaylistEntryRow(r);
                    }
                }
            }
            FillDatabase(CurrentPlaylist.playlist_id);
        }

        void contextPlaylistAdd(object sender, EventArgs e)
        {
            string name = (sender as MenuItem).Header.ToString();

            if (name == "Neue Abspielliste")
            {
                StringEntryDialog dialog = new StringEntryDialog();
                if (dialog.ShowDialog() == true)
                {
                    AudiosortDataset.PlaylistRow prow = Dataset.Playlist.NewPlaylistRow();
                    prow.playlist_name = dialog.ResponseText;
                    Dataset.Playlist.AddPlaylistRow(prow);
                    updatePlaylistList();

                    name = prow.playlist_name;
                }
                else
                {
                    return;
                }
            }

            foreach (var row in Playlist.SelectedItems)
            {
                if (row == null)
                    continue;

                var titel = GetProperty<AudiosortDataset.TitelRow>(row, "t");
                if (titel == null)
                    continue;

                var result = from p in Dataset.Playlist
                             where p.playlist_name == name
                             select p;

                if (result.Count() > 0)
                {
                    AudiosortDataset.PlaylistRow playlist = result.ElementAt(0);
                    var result2 = from p in Dataset.PlaylistEntry
                                  where p.playlist_id == playlist.playlist_id && p.titel_id == titel.titel_id
                                  select p;
                    if (result2.Count() > 0)
                        continue;

                    AudiosortDataset.PlaylistEntryRow newRow = Dataset.PlaylistEntry.NewPlaylistEntryRow();
                    newRow.titel_id = titel.titel_id;
                    newRow.playlist_id = playlist.playlist_id;
                    Dataset.PlaylistEntry.AddPlaylistEntryRow(newRow);
                }
            }
        }

        void contextPause(object sender, EventArgs e)
        {
            if (SoundOutput != null)
                SoundOutput.Pause();
        }

        void contextPlay(object sender, EventArgs e)
        {
            var row = Playlist.SelectedItem;
            if (row == null)
                return;

            var titel = GetProperty<AudiosortDataset.TitelRow>(row, "t");
            if (titel == null)
                return;

            if (titel != CurrentTitle)
            {
                CurrentTitle = titel;
                LoadSound(titel.titel_pfad);
                SoundOutput.Play();
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F1:
                    {
                        new AboutDialog().ShowDialog();
                        break;
                    }
            }
        }

        /* Hilfsfunktionen */

        MenuItem addMenuItem(ItemsControl menu, string title, EventHandler handler, string name = "")
        {
            MenuItem item = new MenuItem();
            if (name != "")
            {
                item.Name = name;
                Image icon = new Image();
                icon.Source = TryFindResource("img_" + name) as BitmapImage;
                item.Icon = icon;
            }
            item.Header = title;
            if (handler != null)
                item.Click += new RoutedEventHandler(handler);
            menu.Items.Add(item);
            return item;
        }

        T GetProperty<T>(object o, string prop)
        {
            if (o == null)
                return default(T);

            PropertyInfo pi = o.GetType().GetProperty(prop);
            if (pi == null)
                return default(T);

            return (T)pi.GetValue(o, null);
        }

        void playlistAddRow(int id, string name)
        {
            AudiosortDataset.PlaylistRow row = Dataset.Playlist.NewPlaylistRow();
            row.playlist_id = id;
            row.playlist_name = name;
            PlaylistAddedRows.Add(row);
            Dataset.Playlist.AddPlaylistRow(row);
        }

        void titleAddRow(int index, char c, string name)
        {
            AudiosortDataset.TitelRow row = Dataset.Titel.NewTitelRow();
            row.titel_name = name;
            row.titel_pfad = "cd://" + c + "/Track" + index;
            row.titel_track = index;
            TitleAddedRows.Add(row);
            Dataset.Titel.AddTitelRow(row);
        }

        void RemoveRows()
        {
            foreach (AudiosortDataset.PlaylistRow row in PlaylistAddedRows)
            {
                Dataset.Playlist.RemovePlaylistRow(row);
            }
            PlaylistAddedRows.Clear();
            RemoveTitleRows();
        }

        void RemoveTitleRows()
        {
            foreach (AudiosortDataset.TitelRow row in TitleAddedRows)
            {
                Dataset.Titel.RemoveTitelRow(row);
            }
            TitleAddedRows.Clear();
        }

        void RemoveTitle(AudiosortDataset.TitelRow row)
        {
            var result2 = from entry in Dataset.PlaylistEntry
                          where entry.titel_id == row.titel_id
                          select entry;

            if (result2.Count() > 0)
            {
                List<AudiosortDataset.PlaylistEntryRow> rows2 = new List<AudiosortDataset.PlaylistEntryRow>();
                foreach (var row2 in result2)
                {
                    rows2.Add(row2);
                }
                foreach (var row2 in rows2)
                {
                    Dataset.PlaylistEntry.RemovePlaylistEntryRow(row2);
                }
            }

            Dataset.Titel.RemoveTitelRow(row);
        }


        /* Drag and Drop */

        bool isDragging = false;
        AudiosortDataset.TitelRow DraggingObject = null;

        private void Playlist_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var row = UIHelper.TryFindFromPoint<DataGridRow>((UIElement)sender, e.GetPosition(Playlist));
            if (row != null)
            {
                DraggingObject = GetProperty<AudiosortDataset.TitelRow>(row.Item, "t");
                if (DraggingObject != null)
                {
                    isDragging = true;
                }
            }
        }

        private void DockPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!isDragging)
                return;

            var row = UIHelper.TryFindFromPoint<DataGridRow>((UIElement)sender, e.GetPosition(PlaylistList));
            if (row != null && row.Item.GetType() == typeof(AudiosortDataset.PlaylistRow))
            {
            }
        }

        public static AudiosortDataset GetDataset()
        {
            return Dataset;
        }

        void LoadSort()
        {
            string[] sort = Config["PlaylistSort"].Split(';');
            if (sort.Length == 2)
            {
                Playlist.Items.SortDescriptions.Clear();
                Playlist.Items.SortDescriptions.Add(
                    new System.ComponentModel.SortDescription(sort[0].Trim(),
                        (sort[1] == System.ComponentModel.ListSortDirection.Ascending.ToString()) ?
                System.ComponentModel.ListSortDirection.Ascending : System.ComponentModel.ListSortDirection.Descending));
            }
        }

        void SaveSort()
        {
            // Sortierung speichern
            var sort = Playlist.Items.SortDescriptions;
            if (sort.Count > 0)
            {
                Config["PlaylistSort"] = sort[0].PropertyName + ";" + sort[0].Direction;
            }
        }
    }

    class PlaylistComparer : IComparer<AudiosortDataset.PlaylistRow>
    {
        public int Compare(AudiosortDataset.PlaylistRow a, AudiosortDataset.PlaylistRow b)
        {
            if (a.playlist_id == -1 && b.playlist_id != -1)
                return -1;
            else if (b.playlist_id == -1 && a.playlist_id != -1)
                return 1;
            else if (a.playlist_id < 0)
                return a.playlist_id.CompareTo(b.playlist_id);
            else if (b.playlist_id < 0 && a.playlist_id >= 0)
                return 1;
            else if (a.playlist_id >= 0 && b.playlist_id >= 0)
                return String.Compare(a.playlist_name, b.playlist_name);
            return -1;
        }
    }

    public class ID
    {
        public const int PlaylistAll = -1;
        public const int PlaylistSep = 0;
        public static int PlaylistCD(int i)
        {
            return -20 + i;
        }
        public static bool isCD(int i)
        {
            return -20 <= i && i <= -10;
        }
    }
}
