using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ScriptKiddie.WinUI.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ScriptKiddie.WinUI.Services;

public partial class SelectScheduleProvider : ObservableObject
{
    private readonly IAppSettingsService appSettingsService;

    [ObservableProperty]
    public partial ObservableCollection<SelectSchedule> SelectSchedules { get; set; }

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

    public void RemoveRange(IEnumerable<SelectSchedule> selectSchedules)
    {
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

    public void Add(SelectSchedule schedule)
    {
        SelectSchedules.Add(schedule);
        Update();
    }

    /// <summary>
    /// 供外部在修改集合内部对象的属性时调用
    /// </summary>
    public void Update()
    {
        // 集合内容变化不会导致引用地址发生变化。需要手动保存。
        appSettingsService.SelectSchedules.Save();

        WeakReferenceMessenger.Default.Send<SelectScheduleChangedMessage>(new SelectScheduleChangedMessage());
    }
}
