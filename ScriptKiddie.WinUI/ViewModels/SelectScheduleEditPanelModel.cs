using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using ScriptKiddie.WinUI.Models;
using ScriptKiddie.WinUI.Services;
using System;

namespace ScriptKiddie.WinUI.ViewModels;

public partial class SelectScheduleEditPanelModel : ObservableObject
{
    private readonly SelectScheduleProvider selectScheduleProvider = App.Current.Services.GetRequiredService<SelectScheduleProvider>();

    private readonly bool isEdit;
    private readonly string? initialName = null;
    private readonly DateTime? initialStartTime = null;
    private readonly DateTime? initialEndTime = null;
    private readonly SelectType? initialSelectType = null;

    private readonly SelectSchedule? selectSchedule = null;

    public SelectScheduleEditPanelModel()
    {
        isEdit = false;

        Name = "未命名时间表";
        SelectType = SelectType.SelectAndWithdraw;

        var now = DateTime.Now;
        EndTimeDate = StartTimeDate = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);

        StartTimeTime = new TimeSpan(12, 0, 0);
        EndTimeTime = StartTimeTime + new TimeSpan(6, 0, 0);
    }

    public SelectScheduleEditPanelModel(SelectSchedule selectSchedule)
    {
        isEdit = true;
        initialName = Name = selectSchedule.Name;
        initialSelectType = SelectType = selectSchedule.SelectType;

        initialStartTime = StartTime = selectSchedule.ScheduleTime.StartTime;
        initialEndTime = EndTime = selectSchedule.ScheduleTime.EndTime;

        StartTimeDate = initialStartTime.Value;
        StartTimeTime = initialStartTime.Value.TimeOfDay;
        EndTimeDate = initialEndTime.Value;
        EndTimeTime = initialEndTime.Value.TimeOfDay;

        this.selectSchedule = selectSchedule;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NameErrorMessage))]
    [NotifyCanExecuteChangedFor(nameof(ModifyCommand))]
    public partial string Name { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ModifyCommand))]
    public partial SelectType SelectType { get; set; }

    [ObservableProperty]
    public partial DateTimeOffset StartTimeDate { get; set; }

    [ObservableProperty]
    public partial DateTimeOffset EndTimeDate { get; set; }

    [ObservableProperty]
    public partial TimeSpan StartTimeTime { get; set; }

    [ObservableProperty]
    public partial TimeSpan EndTimeTime { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NameErrorMessage))]
    [NotifyCanExecuteChangedFor(nameof(ModifyCommand))]
    public partial DateTime StartTime { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NameErrorMessage))]
    [NotifyCanExecuteChangedFor(nameof(ModifyCommand))]
    public partial DateTime EndTime { get; set; }

    [ObservableProperty]
    public partial string Message { get; set; } = string.Empty;

    private static string? ValidateName(string? value) => string.IsNullOrWhiteSpace(value) ? "名称不能为空" : null;

    private bool nameBeenValid = false;

    /// <summary>
    /// 执行校验并返回状态。仅在曾经合法过且当前有错时才暴露错误信息。
    /// </summary>
    private static ValidationErrorStatus Validate(string? value, Func<string?, string?> validateFunc, ref bool beenValidFlag)
    {
        string? error = validateFunc(value);
        if (error == null)
        {
            beenValidFlag = true;
            return new ValidationErrorStatus();
        }
        return beenValidFlag ? new ValidationErrorStatus(error) : new ValidationErrorStatus();
    }

    public ValidationErrorStatus NameErrorMessage
    {
        get
        {
            return Validate(Name, ValidateName, ref nameBeenValid);
        }
    }

    public ValidationErrorStatus TimeErrorMessage
    {
        get
        {
            if (EndTime <= StartTime)
            {
                Message = "结束时间需要在起始时间之后";
                return new ValidationErrorStatus("结束时间需要在起始时间之后");
            }

            return new ValidationErrorStatus();
        }
    }

    partial void OnStartTimeDateChanged(DateTimeOffset value)
    {
        StartTime = value.Date + StartTimeTime;
    }

    partial void OnEndTimeDateChanged(DateTimeOffset value)
    {
        EndTime = value.Date + EndTimeTime;
    }

    // 当 StartTimeTime 变化时，重新合并 StartTime
    partial void OnStartTimeTimeChanged(TimeSpan value)
    {
        StartTime = StartTimeDate.DateTime.Date + value;
    }

    // 当 EndTimeTime 变化时，重新合并 EndTime
    partial void OnEndTimeTimeChanged(TimeSpan value)
    {
        EndTime = EndTimeDate.DateTime.Date + value;
    }

    public event EventHandler? CloseRequested;

    private void RequestClose()
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    private bool CanModify()
    {
        if (!NameErrorMessage.Success || !TimeErrorMessage.Success || !nameBeenValid)
        {
            return false;
        }

        if (isEdit)
        {
            if (Name == initialName && StartTime == initialStartTime && EndTime == initialEndTime && SelectType == initialSelectType)
                return false;

            return true;
        }

        return true;
    }

    [RelayCommand(CanExecute = nameof(CanModify))]
    private void Modify()
    {
        if (isEdit)
        {
            selectSchedule?.Name = Name;
            selectSchedule?.SelectType = SelectType;
            selectSchedule?.ScheduleTime.StartTime = StartTime;
            selectSchedule?.ScheduleTime.EndTime = EndTime;
            selectScheduleProvider.Update();
        }
        else
        {
            var schedule = new SelectSchedule(new ScheduleTime(StartTime, EndTime), Name, SelectType);
            selectScheduleProvider.Add(schedule);
        }
        RequestClose();
    }

    [RelayCommand]
    private void Cancel()
    {
        RequestClose();
    }
}
