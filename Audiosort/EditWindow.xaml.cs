using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Audiosort
{
    /// <summary>
    /// Interaktionslogik für EditWindow.xaml
    /// </summary>
    public partial class EditWindow : Window
    {
        AudiosortDataset.TitelRow CurrentTitle;
        AudiosortDataset Dataset;

        public EditWindow()
            : this(null, null)
        { }

        public EditWindow(AudiosortDataset.TitelRow row, AudiosortDataset dataset)
        {
            CurrentTitle = row;
            Dataset = dataset;

            InitializeComponent();

            if (row != null)
            {
                EditTitel.Text = row.titel_name;
                EditJahr.Text = row.titel_jahr.ToString();
                if (EditJahr.Text == "-1")
                    EditJahr.Text = "";
                EditTrack.Text = row.titel_track.ToString();
                if (EditTrack.Text == "-1")
                    EditTrack.Text = "";
            }

            fillComboboxes();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (CurrentTitle != null)
            {
                int jahr = -1;
                int track = -1;
                try
                {
                    if (EditJahr.Text.Trim().Length > 0)
                        jahr = int.Parse(EditJahr.Text);
                    if (EditTrack.Text.Trim().Length > 0)
                        track = int.Parse(EditTrack.Text);
                }
                catch (Exception)
                {
                    MessageBox.Show("Fehlerhafte Eingabe(n)!");
                    return;
                }

                CurrentTitle.titel_interpret = (EditInterpret.SelectedItem as AudiosortDataset.InterpretenRow).interpret_id;
                CurrentTitle.titel_album = (EditAlbum.SelectedItem as AudiosortDataset.AlbumRow).album_id;
                CurrentTitle.titel_genre = (EditGenre.SelectedItem as AudiosortDataset.GenreRow).genre_id;
                CurrentTitle.titel_jahr = jahr;
                CurrentTitle.titel_track = track;
            }

            DialogResult = true;
            Close();
        }

        void fillComboboxes()
        {
            var interpreten = from i in Dataset.Interpreten
                              orderby i.interpret_name
                              select i;


            foreach (var i in interpreten)
            {
                EditInterpret.Items.Add(i);
                if (CurrentTitle != null && CurrentTitle.titel_interpret == i.interpret_id)
                {
                    EditInterpret.SelectedItem = i;
                }
            }

            var alben = from a in Dataset.Album
                        orderby a.album_name
                        select a;


            foreach (var a in alben)
            {
                EditAlbum.Items.Add(a);
                if (CurrentTitle != null && CurrentTitle.titel_album == a.album_id)
                {
                    EditAlbum.SelectedItem = a;
                }
            }

            var genres = from g in Dataset.Genre
                        orderby g.genre_name
                        select g;


            foreach (var g in genres)
            {
                EditGenre.Items.Add(g);
                if (CurrentTitle != null && CurrentTitle.titel_genre == g.genre_id)
                {
                    EditGenre.SelectedItem = g;
                }
            }
        }

        private void ButtonAddInterpret_Click(object sender, RoutedEventArgs e)
        {
            StringEntryDialog dia = new StringEntryDialog();
            dia.Title = "Neuer Interpret";
            if (dia.ShowDialog() == true)
            {
                AudiosortDataset.InterpretenRow row;

                var result = from i in Dataset.Interpreten
                             where i.interpret_name.ToLower() == dia.ResponseText.ToLower()
                             select i;
                if (result.Count() > 0)
                {
                    row = result.ElementAt(0);
                }
                else
                {
                    row = Dataset.Interpreten.NewInterpretenRow();
                    row.interpret_name = dia.ResponseText;
                    Dataset.Interpreten.AddInterpretenRow(row);
                    EditInterpret.Items.Add(row);
                }
                EditInterpret.SelectedItem = row;
            }
        }

        private void ButtonAddAlbum_Click(object sender, RoutedEventArgs e)
        {
            StringEntryDialog dia = new StringEntryDialog();
            dia.Title = "Neues Album";
            if (dia.ShowDialog() == true)
            {
                AudiosortDataset.AlbumRow row;

                var result = from a in Dataset.Album
                             where a.album_name.ToLower() == dia.ResponseText.ToLower()
                             select a;
                if (result.Count() > 0)
                {
                    row = result.ElementAt(0);
                }
                else
                {
                    row = Dataset.Album.NewAlbumRow();
                    row.album_name = dia.ResponseText;
                    Dataset.Album.AddAlbumRow(row);
                    EditAlbum.Items.Add(row);
                }
                EditAlbum.SelectedItem = row;
            }
        }

        private void ButtonAddGenre_Click(object sender, RoutedEventArgs e)
        {
            StringEntryDialog dia = new StringEntryDialog();
            dia.Title = "Neues Genre";
            if (dia.ShowDialog() == true)
            {
                AudiosortDataset.GenreRow row;

                var result = from g in Dataset.Genre
                             where g.genre_name.ToLower() == dia.ResponseText.ToLower()
                             select g;
                if (result.Count() > 0)
                {
                    row = result.ElementAt(0);
                }
                else
                {
                    row = Dataset.Genre.NewGenreRow();
                    row.genre_name = dia.ResponseText;
                    Dataset.Genre.AddGenreRow(row);
                    EditGenre.Items.Add(row);
                }
                EditGenre.SelectedItem = row;
            }
        }
    }
}
