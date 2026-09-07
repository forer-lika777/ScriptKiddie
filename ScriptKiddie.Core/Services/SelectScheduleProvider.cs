using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ScriptKiddie.Core.Models;
using System.Collections.ObjectModel;

namespace ScriptKiddie.Core.Services;

public partial class SelectScheduleProvider : ObservableObject, ISelectScheduleProvider
{
    private readonly IAppSettingsService appSettingsService;

    [ObservableProperty]
    public partial ObservableCollection<SelectSchedule> SelectSchedules { get; set; }

    public ObservableCollection<SelectSchedule> GetSelectSchedules()
    {
        return this.SelectSchedules;
    }

    public SelectScheduleProvider(IAppSettingsService appSettingsService)
    {
        this.appSettingsService = appSettingsService;
        SelectSchedules = appSettingsService.SelectSchedules.Value;
    }

    public void Remove(SelectSchedule selectSchedule)
    {
        SelectSchedules.Remove(selectSchedule);
        Update();
    }

    public async Task RemoveRange(IEnumerable<SelectSchedule> selectSchedules)
    {
        var tcs = new TaskCompletionSource();

        // 接受方：CourseSelectService、CourseListPageModel
        WeakReferenceMessenger.Default.Send<SelectScheduleRemoveMessage>(new SelectScheduleRemoveMessage(selectSchedules, tcs));

        try
        {
            await tcs.Task;
        }
        catch (OperationCanceledException)
        {
            return;
        }

        foreach (var selectSchedule in selectSchedules)
        {
            SelectSchedules.Remove(selectSchedule);
        }

        Update();
    }

    public void Remove(int hash)
    {
        foreach (var selectSchedule in SelectSchedules)
        {
            if (selectSchedule.GetHashCode() == hash)
            {
                SelectSchedules.Remove(selectSchedule);
                Update();
                break;
            }
        }
    }

    public async void Add(SelectSchedule schedule)
    {
        SelectSchedules.Add(schedule);

        WeakReferenceMessenger.Default.Send<SelectScheduleAddedMessage>(new SelectScheduleAddedMessage());

        Update();
    }

    public void Update()
    {
        // 集合内容变化不会导致引用地址发生变化。需要手动保存。
        appSettingsService.SelectSchedules.Save();
    }
}
