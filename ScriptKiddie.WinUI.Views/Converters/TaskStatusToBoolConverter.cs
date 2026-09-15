using Microsoft.UI.Xaml.Data;
using ScriptKiddie.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptKiddie.WinUI.Views.Converters;

public partial class TaskStatusToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not SelectStatus status)
            return false;

        return status == SelectStatus.Executing || status == SelectStatus.Pending;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
