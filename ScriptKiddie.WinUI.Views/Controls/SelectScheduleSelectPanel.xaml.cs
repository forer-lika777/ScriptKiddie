using Microsoft.UI.Xaml.Controls;
using ScriptKiddie.Core.Services;
using ScriptKiddie.Core.ViewModels;
using ScriptKiddie.WinUI.Views;

namespace ScriptKiddie.WinUI.Pages.Controls;

public sealed partial class SelectScheduleSelectPanel : UserControl
{
    public SelectScheduleSelectPanelModel ViewModel { get; set; }

    public SelectScheduleSelectPanel()
    {
        InitializeComponent();
        ViewModel = new SelectScheduleSelectPanelModel(AppServices.GetRequiredService<ISelectScheduleProvider>());
    }
}
