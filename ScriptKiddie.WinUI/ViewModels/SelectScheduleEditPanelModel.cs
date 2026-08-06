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
        var now = DateTime.Now;
        Date = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
        StartTimeTime = new TimeSpan(now.TimeOfDay.Hours, 0, 0);
        EndTimeTime = StartTimeTime + new TimeSpan(1, 0, 0);

        SelectType = SelectType.SelectAndWithdraw;
    }

    public SelectScheduleEditPanelModel(SelectSchedule selectSchedule)
    {
        initialName = Name = selectSchedule.Name;
        initialStartTime = StartTime = selectSchedule.ScheduleTime.StartTime;
        initialEndTime = EndTime = selectSchedule.ScheduleTime.EndTime;
        initialSelectType = SelectType = selectSchedule.SelectType;
        StartTimeTime = initialStartTime.Value.TimeOfDay;
        EndTimeTime = initialEndTime.Value.TimeOfDay;
        Date = initialStartTime.Value;
        this.selectSchedule = selectSchedule;
        isEdit = true;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NameErrorMessage))]
    [NotifyCanExecuteChangedFor(nameof(ModifyCommand))]
    public partial string Name { get; set; } = "未命名时间表";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ModifyCommand))]
    public partial SelectType SelectType { get; set; }

    [ObservableProperty]
    public partial DateTimeOffset Date { get; set; }

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
            if (EndTime < StartTime)
            {
                Message = "结束时间需要大于起始时间";
                return new ValidationErrorStatus("结束时间需要大于起始时间");
            }

            return new ValidationErrorStatus();
        }
    }

    // 当 StartTimeDate 变化时，重新合并 StartTime
    partial void OnDateChanged(DateTimeOffset value)
    {
        StartTime = value.DateTime.Date + StartTimeTime;
        EndTime = value.DateTime.Date + EndTimeTime;
        //OnPropertyChanged(nameof(Modify));
    }

    // 当 StartTimeTime 变化时，重新合并 StartTime
    partial void OnStartTimeTimeChanged(TimeSpan value)
    {
        StartTime = Date.DateTime.Date + value;
    }

    // 当 EndTimeTime 变化时，重新合并 EndTime
    partial void OnEndTimeTimeChanged(TimeSpan value)
    {
        EndTime = Date.DateTime.Date + value;
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
        }
        else
        {
            var schedule = new SelectSchedule([], new ScheduleTime(StartTime, EndTime), SelectType, Name);
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
