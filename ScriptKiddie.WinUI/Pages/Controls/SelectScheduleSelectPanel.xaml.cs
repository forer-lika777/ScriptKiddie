using Microsoft.UI.Xaml.Controls;
using ScriptKiddie.WinUI.ViewModels;

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
