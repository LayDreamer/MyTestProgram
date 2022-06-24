using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ModelReviewFunction.MVVM
{
    public class PointState2TreeViewItemBackgroundConverter : IValueConverter
    {

        SolidColorBrush green = new SolidColorBrush(Color.FromRgb(50, 205, 50));
        SolidColorBrush red = new SolidColorBrush(Color.FromRgb(255, 106, 106));
        SolidColorBrush transparent = new SolidColorBrush(Colors.Transparent);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return red;
            }
            int? myEnum = (int)value;
            if (myEnum != null)
            {
                if (myEnum == 10)
                {
                    return green;
                }
                else if (myEnum == 20)
                {
                    return red;
                }
                else if (myEnum == 30)
                {
                    return transparent;
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class Bool2VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return System.Windows.Visibility.Collapsed;
            }
            bool flag = (bool)value;
            if (flag)
            {
                return System.Windows.Visibility.Visible;
            }
            else
            {
                return System.Windows.Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
