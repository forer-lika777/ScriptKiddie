using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptKiddie.WinUI.Utils.Converters;

public class ErrorToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not bool success || !success)
            return new SolidColorBrush();

        return new SolidColorBrush(Color.FromArgb(175, 255, 255, 0));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class ErrorToTranslucentBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not bool success || !success)
            return new SolidColorBrush();

        return new SolidColorBrush(Color.FromArgb(20, 255, 0, 0));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
