using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Threading;

namespace ScriptKiddie.WinUI.Models;

public partial class CourseSelectTask : ObservableObject
{
    public CourseSelectTask(SelectSchedule selectSchedule, CourseItem course, SelectStatus selectStatus, OperationType operationType, CancellationToken? cancellationToken = null)
    {
        this.SelectSchedule = selectSchedule;
        this.Course = course;
        this.SelectStatus = selectStatus;
        this.OperationType = operationType;
        this.Cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken ?? CancellationToken.None);
    }

    public CourseSelectTask(SelectSchedule selectSchedule, CourseItem course, CourseItem courseToWithdraw, SelectStatus selectStatus, CancellationToken? cancellationToken = null)
    {
        this.SelectSchedule = selectSchedule;
        this.Course = course;
        this.CourseToWithdraw = courseToWithdraw;
        this.SelectStatus = selectStatus;
        this.OperationType = OperationType.WithdrawToSelect;
        this.Cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken ?? CancellationToken.None);
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

    partial void OnOperationTypeChanged(OperationType value)
    {
        if (value == OperationType.WithdrawToSelect)
        {
            if (CourseToWithdraw is null)
                throw new InvalidOperationException("操作类型是退选后选课的任务必须提供要退选的课程。");
        }
    }

    public CancellationTokenSource Cts { get; set; }
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