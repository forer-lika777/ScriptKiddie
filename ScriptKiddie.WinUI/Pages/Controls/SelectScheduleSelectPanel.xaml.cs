using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using ScriptKiddie.WinUI.Models;
using ScriptKiddie.WinUI.Services;
using ScriptKiddie.WinUI.ViewModels;
using System.Collections.ObjectModel;

namespace ScriptKiddie.WinUI.Pages.Controls;

public sealed partial class SelectScheduleSelectPanel : UserControl
{
    public SelectScheduleSelectPanelModel ViewModel { get; set; }

    public SelectScheduleSelectPanel()
    {
        InitializeComponent();
        ViewModel = new SelectScheduleSelectPanelModel();
    }
}
