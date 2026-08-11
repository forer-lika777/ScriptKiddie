using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Threading;
using Windows.Security.EnterpriseData;

namespace ScriptKiddie.WinUI.Models;

public partial class CourseSelectTask : ObservableObject
{
    public CourseSelectTask(SelectSchedule selectSchedule, CourseItem course, SelectStatus selectStatus, OperationType operationType, CancellationToken? cancellationToken = null)
    {
        this.SelectSchedule = selectSchedule;
        this.Course = course;
        this.SelectStatus = selectStatus;
        this.OperationType = operationType;
        this.cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken ?? CancellationToken.None);
    }

    [ObservableProperty]
    public partial SelectSchedule SelectSchedule { get; set; }

    [ObservableProperty]
    public partial CourseItem Course { get; set; }

    [ObservableProperty]
    public partial CourseItem? CourseToWithdraw { get; set; } = null;

    [ObservableProperty]
    public partial SelectStatus SelectStatus { get; set; }

    [ObservableProperty]
    public partial OperationType OperationType { get; set; }

    public CancellationTokenSource cts { get; set; }
}

public partial class SelectSchedule : ObservableObject
{
    public SelectSchedule(ScheduleTime scheduleTime, string name = "", SelectType selectType = SelectType.SelectAndWithdraw)
    {
        ScheduleTime = scheduleTime;
        SelectType = selectType;
        if (string.IsNullOrWhiteSpace(name))
        {
            Name = "未命名时间表";
        }
        else
        {
            Name = name;
        }
    }

    [ObservableProperty]
    public partial string Name { get; set; }

    [ObservableProperty]
    public partial ScheduleTime ScheduleTime { get; set; }

    [ObservableProperty]
    public partial SelectType SelectType { get; set; }
}

public partial class ScheduleTime : ObservableObject
{
    public ScheduleTime() { }

    public ScheduleTime(DateTime startTime, DateTime endTime)
    {
        StartTime = startTime;
        EndTime = endTime;
    }

    [ObservableProperty]
    public partial DateTime StartTime { get; set; }
    [ObservableProperty]
    public partial DateTime EndTime { get; set; }

    public override string ToString()
    {
        return StartTime.ToString() + " ~ " + EndTime.ToString();
    }
}

public enum SelectType
{
    SelectAndWithdraw,
    SelectOnly,
    WithdrawOnly,
}

public enum OperationType
{
    Select,
    Withdraw,
    WithdrawToSelect
}

public enum SelectStatus
{
    Pending,
    Executing,
    Completed,
    Canceled,
    Failed,
}