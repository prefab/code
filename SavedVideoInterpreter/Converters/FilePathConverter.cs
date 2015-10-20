using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace SavedVideoInterpreter
{
    public class FilePathConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType == typeof(string))
            {
                return (string)value;
            }
            else
            {
                return Thumb(value as string);
            }
        }

        private BitmapSource Thumb(string uri)
        {
            if (uri == null)
                return null;
            
            return new BitmapImage(new Uri(uri, UriKind.RelativeOrAbsolute));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
