using CommunityToolkit.Mvvm.ComponentModel;
using ScriptKiddie.Core.Models;
using ScriptKiddie.Core.Services;
using System.Collections.ObjectModel;

namespace ScriptKiddie.Core.ViewModels;

public partial class SelectScheduleSelectPanelModel : ObservableObject
{
    private readonly ISelectScheduleProvider selectScheduleProvider;

    public SelectScheduleSelectPanelModel(ISelectScheduleProvider selectScheduleProvider)
    {
        this.selectScheduleProvider = selectScheduleProvider;
        SelectSchedules = selectScheduleProvider.GetSelectSchedules();
        SelectedSchedule = SelectSchedules[0];
    }

    [ObservableProperty]
    public partial ObservableCollection<SelectSchedule> SelectSchedules { get; set; }

    [ObservableProperty]
    public partial SelectSchedule SelectedSchedule { get; set; }
}
