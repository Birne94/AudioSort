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
using Audiosort.Codecs.CD;

namespace Audiosort
{
    /// <summary>
    /// Interaktionslogik für CDInfoChoice.xaml
    /// </summary>
    public partial class CDInfoChoice : Window
    {
        List<CDInformation> Data;
        public CDInformation Result = null;

        public CDInfoChoice(List<CDInformation> data)
        {
            Data = data;
            InitializeComponent();
            initData();
        }

        public CDInfoChoice()
            : this(null)
        { }

        void initData()
        {
            foreach (CDInformation info in Data)
            {
                ResultListBox.Items.Add(info);
            }
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            if (ResultListBox.SelectedItem != null)
            {
                Result = ResultListBox.SelectedItem as CDInformation;
                Close();
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Result = null;
            Close();
        }
    }
}
