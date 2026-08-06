using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;

namespace ScriptKiddie.WinUI.Models;

public partial class SelectSchedule : ObservableObject
{
    public SelectSchedule(List<CourseItem> courses, ScheduleTime scheduleTime, SelectType selectType = SelectType.SelectAndWithdraw, string name = "")
    {
        Courses = courses;
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
    public partial List<CourseItem> Courses { get; set; }
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
}

public enum SelectType
{
    SelectAndWithdraw,
    SelectOnly,
    WithdrawOnly
}