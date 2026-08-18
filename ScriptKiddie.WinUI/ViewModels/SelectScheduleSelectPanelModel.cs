using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using ScriptKiddie.WinUI.Models;
using ScriptKiddie.WinUI.Services;
using System.Collections.ObjectModel;

namespace ScriptKiddie.WinUI.ViewModels;

public partial class SelectScheduleSelectPanelModel : ObservableObject
{
    private readonly SelectScheduleProvider selectScheduleProvider;

    public SelectScheduleSelectPanelModel()
    {
        selectScheduleProvider = App.Current.Services.GetRequiredService<SelectScheduleProvider>();
        SelectSchedules = selectScheduleProvider.SelectSchedules;
        SelectedSchedule = SelectSchedules[0];
    }

    [ObservableProperty]
    public partial ObservableCollection<SelectSchedule> SelectSchedules { get; set; }

    [ObservableProperty]
    public partial SelectSchedule SelectedSchedule { get; set; }
}
