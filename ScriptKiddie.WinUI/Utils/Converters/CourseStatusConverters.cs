using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.UI;

namespace ScriptKiddie.WinUI.Utils.Converters;

public partial class ErrorToBrushConverter : IValueConverter
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

public partial class ErrorToTranslucentBrushConverter : IValueConverter
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

public partial class IsFullStatusToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not bool isFull || isFull)
            return "已满";

        return "未满";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public partial class IsFullStatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not bool isFull || isFull)
            return new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));

        return new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
