using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ScriptKiddie.Core.Models;
using ScriptKiddie.Core.Services;
using System.Collections.ObjectModel;

namespace ScriptKiddie.Core.ViewModels;

public partial class SelectSchedulePageModel : ObservableObject, IRecipient<SelectScheduleRemoveMessage>, IRecipient<SelectScheduleAddedMessage>
{
    private readonly IAppSettingsService appSettingsService;
    private readonly ISelectScheduleProvider selectScheduleProvider;

    public SelectSchedulePageModel(IAppSettingsService appSettingsService, ISelectScheduleProvider selectScheduleProvider)
    {
        this.appSettingsService = appSettingsService;
        this.selectScheduleProvider = selectScheduleProvider;
        SelectSchedules = selectScheduleProvider.SelectSchedules;

        WeakReferenceMessenger.Default.Register<SelectScheduleRemoveMessage>(this);
        WeakReferenceMessenger.Default.Register<SelectScheduleAddedMessage>(this);
    }

    [ObservableProperty]
    public partial ObservableCollection<SelectSchedule> SelectSchedules { get; set; } = [];

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
