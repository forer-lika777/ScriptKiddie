using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ScriptKiddie.Core.Models;
using ScriptKiddie.Core.Services;
using System.Collections.ObjectModel;

namespace ScriptKiddie.Core.ViewModels;

public partial class CourseListPageModel : ObservableObject, IRecipient<SelectScheduleRemoveMessage>, IRecipient<SelectScheduleAddedMessage>
{
    private readonly IAccountManageService accountManageService;
    private readonly ISelectScheduleProvider selectScheduleProvider;

    public CourseListPageModel(ICourseSelectService courseSelectService, IAccountManageService accountManageService, ISelectScheduleProvider selectScheduleProvider)
    {
        this.accountManageService = accountManageService;
        this.selectScheduleProvider = selectScheduleProvider;
        SelectSchedules = selectScheduleProvider.SelectSchedules;
        SelectTasks = courseSelectService.SelectTasks;
        _ = SyncCoursesContent();

        WeakReferenceMessenger.Default.Register<SelectScheduleRemoveMessage>(this);
        WeakReferenceMessenger.Default.Register<SelectScheduleAddedMessage>(this);
    }

    [RelayCommand]
    private async Task SyncCoursesContent()
    {
        var selectableCourses = await accountManageService.GetSelectableCoursesAsync();
        if (selectableCourses is not null)
            SelectableCourses = selectableCourses;

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
    public partial ObservableCollection<CourseItem> SelectableCourses { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<CourseItem> SelectedCourses { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<SelectSchedule> SelectSchedules { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<CourseSelectTask> SelectTasks { get; set; } = [];

    partial void OnSelectSchedulesChanged(ObservableCollection<SelectSchedule> value)
    {
        CheckSelectScheduleCount();
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddCourseCommand))]
    public partial bool HasCourseSelectSchedule { get; set; } = false;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddCourseCommand))]
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

    public void Receive(SelectScheduleAddedMessage message)
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

    private bool CanAddCourse()
    {
        if (SelectLimitCount == 0)
            return false;

        return HasCourseSelectSchedule;
    }

    [RelayCommand(CanExecute = nameof(CanAddCourse))]
    private async Task AddCourse(CourseItem course)
    {
        var selectScheduleTcs = new TaskCompletionSource<SelectSchedule>();
        WeakReferenceMessenger.Default.Send<RequestChooseSelectScheduleMessage>(new RequestChooseSelectScheduleMessage(selectScheduleTcs));

        SelectSchedule? schedule;

        try
        {
            schedule = await selectScheduleTcs.Task;
        }
        catch (OperationCanceledException)
        {
            return;
        }

        if (schedule is null)
            return;

        if (SelectedCourses.Count >= SelectLimitCount)
        {
            var confirmCourseTcs = new TaskCompletionSource<CourseItem>();
            WeakReferenceMessenger.Default.Send<RequestConfirmWithdrawCourseMessage>(new RequestConfirmWithdrawCourseMessage(SelectedCourses, confirmCourseTcs));

            CourseItem? courseToWithdraw;

            try
            {
                courseToWithdraw = await confirmCourseTcs.Task;
            }
            catch (OperationCanceledException)
            {
                return;
            }

            if (courseToWithdraw is null)
                return;

            await accountManageService.AddCourseAsync(course, courseToWithdraw, schedule);
        }
        else
        {
            await accountManageService.AddCourseAsync(course, schedule, OperationType.Select);
        }
    }

    [RelayCommand]
    private async Task WithdrawCourse(CourseItem course)
    {
        var selectScheduleTcs = new TaskCompletionSource<SelectSchedule>();
        WeakReferenceMessenger.Default.Send<RequestChooseSelectScheduleMessage>(new RequestChooseSelectScheduleMessage(selectScheduleTcs));

        SelectSchedule? schedule;

        try
        {
            schedule = await selectScheduleTcs.Task;
        }
        catch (OperationCanceledException)
        {
            return;
        }

        if (schedule is null)
            return;

        await accountManageService.AddCourseAsync(course, schedule, OperationType.Withdraw);
    }
}
