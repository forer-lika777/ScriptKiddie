using CommunityToolkit.Mvvm.ComponentModel;
using ScriptKiddie.WinUI.Models;
using System.Collections.Generic;

namespace ScriptKiddie.WinUI.ViewModels;

public partial class SelectScheduleDeleteConfirmPanelModel : ObservableObject
{
    [ObservableProperty]
    public partial List<CourseSelectTask> SelectTasksToRemove { get; set; } = [];

    [ObservableProperty]
    public partial bool NeedRemoveTask { get; set; } = false;

    public SelectScheduleDeleteConfirmPanelModel(List<CourseSelectTask> selectTasksToRemove)
    {
        if (selectTasksToRemove.Count == 0)
            return;

        NeedRemoveTask = true;
        SelectTasksToRemove = selectTasksToRemove;
    }
}
