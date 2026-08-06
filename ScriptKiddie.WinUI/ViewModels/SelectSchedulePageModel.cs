using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ScriptKiddie.WinUI.Models;
using ScriptKiddie.WinUI.Services;
using System.Collections.ObjectModel;

namespace ScriptKiddie.WinUI.ViewModels;

public partial class SelectSchedulePageModel : ObservableObject, IRecipient<SelectScheduleChangedMessage>
{
    private readonly IAppSettingsService appSettingsService;
    private readonly SelectScheduleProvider selectScheduleProvider;

    public SelectSchedulePageModel(IAppSettingsService appSettingsService, SelectScheduleProvider selectScheduleProvider)
    {
        this.appSettingsService = appSettingsService;
        this.selectScheduleProvider = selectScheduleProvider;
        SelectSchedules = selectScheduleProvider.SelectSchedules;
        WeakReferenceMessenger.Default.Register(this);
    }

    [ObservableProperty]
    public partial ObservableCollection<SelectSchedule> SelectSchedules { get; set; }

    partial void OnSelectSchedulesChanged(ObservableCollection<SelectSchedule> value)
    {
        Receive(null);
    }

    [ObservableProperty]
    public partial bool HasCourseSelectSchedule { get; set; } = false;

    public void Receive(SelectScheduleChangedMessage? message)
    {
        if (SelectSchedules.Count > 0)
        {
            HasCourseSelectSchedule = true;
        }
        else
        {
            HasCourseSelectSchedule = false;
        }
    }
}
