using Microsoft.UI.Xaml.Controls;
using ScriptKiddie.Core.Models;
using ScriptKiddie.Core.ViewModels;
using System.Collections.Generic;

namespace ScriptKiddie.WinUI.Pages.Controls;

public sealed partial class SelectScheduleDeleteConfirmPanel : UserControl
{
    public SelectScheduleDeleteConfirmPanelModel ViewModel { get; set; }

    public SelectScheduleDeleteConfirmPanel(List<CourseSelectTask> selectSchedulesToRemove)
    {
        InitializeComponent();
        ViewModel = new SelectScheduleDeleteConfirmPanelModel(selectSchedulesToRemove);
    }
}
