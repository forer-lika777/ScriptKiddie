using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ScriptKiddie.WinUI.Models;
using ScriptKiddie.WinUI.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ScriptKiddie.WinUI.ViewModels;

public partial class CourseListPageModel : ObservableObject, IRecipient<SelectScheduleChangedMessage>
{
    private readonly AccountManageService accountManageService;
    private readonly SelectScheduleProvider selectScheduleProvider;

    public CourseListPageModel(ICourseSelectService courseSelectService, AccountManageService accountManageService, SelectScheduleProvider selectScheduleProvider)
    {
        this.accountManageService = accountManageService;
        this.selectScheduleProvider = selectScheduleProvider;
        SelectSchedules = selectScheduleProvider.SelectSchedules;
        _ = SyncCoursesContent();
        WeakReferenceMessenger.Default.Register(this);
    }

    [RelayCommand]
    private async Task SyncCoursesContent()
    {
        var selectableCourses = await accountManageService.GetSelectableCoursesAsync();
        if (selectableCourses is not null)
            SelectableCourses = selectableCourses.Rows;

        var selectedCourses = await accountManageService.GetSelectedCoursesAsync();
        if (selectedCourses is not null)
            SelectedCourses = selectedCourses;

        var limitCount = await accountManageService.GetSelectLimitCountAsync();
        if (limitCount is not null)
            SelectLimitCount = (int)limitCount;

        IsLoading = false;

        _ = accountManageService.BeginSyncCourses();
    }

    [ObservableProperty]
    public partial bool IsLoggedIn { get; set; } = false;

    [ObservableProperty]
    public partial bool IsLoading { get; set; } = true;

    [ObservableProperty]
    public partial bool LoadingFailed { get; set; } = false;

    [ObservableProperty]
    public partial List<CourseItem> SelectableCourses { get; set; } = [];

    [ObservableProperty]
    public partial List<CourseItem> SelectedCourses { get; set; } = [];

    [ObservableProperty]
    public partial List<CourseItem> PreSelectCourses { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<SelectSchedule> SelectSchedules { get; set; } = [];

    partial void OnSelectSchedulesChanged(ObservableCollection<SelectSchedule> value)
    {
        Receive(null);
    }

    [ObservableProperty]
    public partial bool HasCourseSelectSchedule { get; set; } = false;

    [ObservableProperty]
    public partial int SelectLimitCount { get; set; } = 0;

    [ObservableProperty]
    public partial bool AutoRefresh { get; set; } = true;

    async partial void OnAutoRefreshChanged(bool value)
    {
        if (AutoRefresh)
        {
            await accountManageService.BeginSyncCourses();
        }
        else
        {
            await accountManageService.StopSyncCourses();
        }
    }

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
