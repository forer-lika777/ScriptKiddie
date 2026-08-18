using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using ScriptKiddie.WinUI.Models;
using ScriptKiddie.WinUI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
