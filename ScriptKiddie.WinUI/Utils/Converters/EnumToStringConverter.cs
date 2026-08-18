using Microsoft.UI.Xaml.Data;
using ScriptKiddie.WinUI.Models;
using System;

namespace ScriptKiddie.WinUI.Utils.Converters;

public partial class SelectStatusToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not SelectStatus status)
            throw new ArgumentException($"转换参数类型不正确。应为：{typeof(SelectStatus).Name}，实际：{targetType.Name}");

        if (status == SelectStatus.Pending)
            return "挂起";

        if (status == SelectStatus.Executing)
            return "执行中";

        if (status == SelectStatus.Completed)
            return "已完成";

        if (status == SelectStatus.Failed)
            return "失败";

        if (status == SelectStatus.Canceled)
            return "已取消";

        throw new ArgumentException($"未知的参数：{status}");
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public partial class SelectTypeToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not SelectType type)
            throw new ArgumentException($"转换参数类型不正确。应为：{typeof(SelectType).Name}，实际：{targetType.Name}");

        if (type == SelectType.SelectOnly)
            return "仅选课";

        if (type == SelectType.WithdrawOnly)
            return "仅退选";

        if (type == SelectType.SelectAndWithdraw)
            return "选课与退选";

        throw new ArgumentException($"未知的参数：{type}");
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public partial class OperationTypeToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not OperationType type)
            throw new ArgumentException($"转换参数类型不正确。应为：{typeof(OperationType).Name}，实际：{targetType.Name}");

        if (type == OperationType.Select)
            return "选课";

        if (type == OperationType.Withdraw)
            return "退选";

        if (type == OperationType.WithdrawToSelect)
            return "退选并选课";

        throw new ArgumentException($"未知的参数：{type}");
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}