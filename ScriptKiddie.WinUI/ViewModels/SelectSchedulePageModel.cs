using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ScriptKiddie.WinUI.Models;
using ScriptKiddie.WinUI.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ScriptKiddie.WinUI.ViewModels;

public partial class SelectSchedulePageModel : ObservableObject, IRecipient<SelectScheduleRemoveMessage>, IRecipient<SelectScheduleAddedMessage>
{
    private readonly IAppSettingsService appSettingsService;
    private readonly SelectScheduleProvider selectScheduleProvider;

    public SelectSchedulePageModel(IAppSettingsService appSettingsService, SelectScheduleProvider selectScheduleProvider)
    {
        this.appSettingsService = appSettingsService;
        this.selectScheduleProvider = selectScheduleProvider;
        SelectSchedules = selectScheduleProvider.SelectSchedules;
        WeakReferenceMessenger.Default.Register<SelectScheduleRemoveMessage>(this);
        WeakReferenceMessenger.Default.Register<SelectScheduleAddedMessage>(this);
    }

    [ObservableProperty]
    public partial ObservableCollection<SelectSchedule> SelectSchedules { get; set; }

    partial void OnSelectSchedulesChanged(ObservableCollection<SelectSchedule> value)
    {
        CheckSelectScheduleCount();
    }

    [ObservableProperty]
    public partial bool HasCourseSelectSchedule { get; set; } = false;

    public async void Receive(SelectScheduleRemoveMessage message)
    {
        try
        {
            await message.TaskCompletionSource.Task;
        }
        catch (OperationCanceledException)
        {
            return;
        }

        await Task.Delay(10);
        CheckSelectScheduleCount();
    }

    public async void Receive(SelectScheduleAddedMessage message)
    {
        CheckSelectScheduleCount();
    }

    private void CheckSelectScheduleCount()
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
