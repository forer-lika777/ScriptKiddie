using Microsoft.UI.Xaml.Data;
using ScriptKiddie.WinUI.Models;
using System;

namespace ScriptKiddie.WinUI.Utils.Converters;

public partial class SelectTypeToIndexConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is SelectType selectType)
            return (int)selectType;

        return 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is int index)
            return (SelectType)index;

        return SelectType.SelectAndWithdraw;
    }
}
