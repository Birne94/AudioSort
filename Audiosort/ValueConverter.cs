using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Text.RegularExpressions;

namespace Audiosort
{
    public class TitleWeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            TextBlock block = value as TextBlock;
            if (block != null && block.Text != "" && MainWindow.CurrentTitle != null)
            {
                var result = from t in MainWindow.GetDataset().Titel
                             where t.titel_name == block.Text
                             select t;

                if (result.Count() > 0)
                {
                    if (result.ElementAt(0).titel_id == MainWindow.CurrentTitle.titel_id)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NumberValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int number = int.Parse(value.ToString());
            if (number <= 0)
                return "";
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class GenreValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Match match = Regex.Match(value.ToString(), @"\((\d+)\)");
            if (match.Success)
            {
                int i = int.Parse(match.Groups[1].Value);
                if (i < Audiosort.Data.ID3Tag.Genres.Length && i > 0)
                {
                    return Audiosort.Data.ID3Tag.Genres[i];
                }
                return "";
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class RatingValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int number = int.Parse(value.ToString());
            if (number < 0)
                number = 0;
            if (number > 5)
                number = 5;

            if (number == 0)
                return null;

            try
            {
                return Application.Current.MainWindow.FindResource("img_star" + number);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
