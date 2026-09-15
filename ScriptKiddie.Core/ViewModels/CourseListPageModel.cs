using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ScriptKiddie.Core.Models;
using ScriptKiddie.Core.Services;
using System.Collections.ObjectModel;

namespace ScriptKiddie.Core.ViewModels;

public partial class CourseListPageModel : ObservableObject, IRecipient<SelectScheduleRemoveMessage>, IRecipient<SelectScheduleAddedMessage>, IRecipient<LockSelectableCoursesSyncStatusMessage>
{
    private readonly ICourseSelectService courseSelectService;
    private readonly IAccountManageService accountManageService;
    private readonly ISelectScheduleProvider selectScheduleProvider;
    private readonly IMessenger messenger;

    public CourseListPageModel(ICourseSelectService courseSelectService, IAccountManageService accountManageService, ISelectScheduleProvider selectScheduleProvider, IMessenger messenger)
    {
        this.courseSelectService = courseSelectService;
        this.accountManageService = accountManageService;
        this.selectScheduleProvider = selectScheduleProvider;
        this.messenger = messenger;
        SelectSchedules = selectScheduleProvider.SelectSchedules;
        SelectTasks = courseSelectService.SelectTasks;
        _ = SyncCoursesContent();

        messenger.Register<SelectScheduleRemoveMessage>(this);
        messenger.Register<SelectScheduleAddedMessage>(this);
        messenger.Register<LockSelectableCoursesSyncStatusMessage>(this);
    }

    [RelayCommand]
    private async Task SyncCoursesContent()
    {
        await SetSelectableCoursesAsync();
        await SetSelectedCoursesAsync();
        await SetSelectLimitCountAsync();

        IsLoading = false;

        courseSelectService.SelectedCoursesChanged += CourseSelectService_SelectedCoursesChanged;
        courseSelectService.SelectableCoursesChanged += CourseSelectService_SelectableCoursesChanged;

        _ = accountManageService.BeginSyncCoursesAsync();
    }

    private async Task SetSelectableCoursesAsync()
    {
        var selectableCourses = await accountManageService.GetSelectableCoursesAsync();
        if (selectableCourses is not null)
            SelectableCourses = selectableCourses;
    }

    private async Task SetSelectedCoursesAsync()
    {
        var selectedCourses = await accountManageService.GetSelectedCoursesAsync();
        if (selectedCourses is not null)
            SelectedCourses = selectedCourses;
    }

    private async Task SetSelectLimitCountAsync()
    {
        var limitCount = await accountManageService.GetSelectLimitCountAsync();
        if (limitCount is not null)
            SelectLimitCount = (int)limitCount;
    }

    private async void CourseSelectService_SelectableCoursesChanged(object? sender, EventArgs e)
    {
        await SetSelectableCoursesAsync();
    }

    private async void CourseSelectService_SelectedCoursesChanged(object? sender, EventArgs e)
    {
        await SetSelectedCoursesAsync();
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

    private bool originAutoRefreshValue = true;

    async partial void OnAutoRefreshChanged(bool value)
    {
        if (CanModifyAutoRefresh)
        {
            originAutoRefreshValue = AutoRefresh;

            if (AutoRefresh)
            {
                await accountManageService.BeginSyncCoursesAsync();
            }
            else
            {
                await accountManageService.StopSyncCoursesAsync();
            }
        }
    }

    [ObservableProperty]
    public partial bool CanModifyAutoRefresh { get; set; } = true;

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
        messenger.Send<RequestChooseSelectScheduleMessage>(new RequestChooseSelectScheduleMessage(selectScheduleTcs));

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
            messenger.Send<RequestConfirmWithdrawCourseMessage>(new RequestConfirmWithdrawCourseMessage(SelectedCourses, confirmCourseTcs));

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
        messenger.Send<RequestChooseSelectScheduleMessage>(new RequestChooseSelectScheduleMessage(selectScheduleTcs));

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

    [RelayCommand]
    private async Task CancelTask(CourseSelectTask task)
    {
        task.Cts.Cancel();
    }

    public void Receive(LockSelectableCoursesSyncStatusMessage message)
    {
        CanModifyAutoRefresh = !message.IsLocked;

        if (CanModifyAutoRefresh)
        {
            AutoRefresh = originAutoRefreshValue;
            OnAutoRefreshChanged(AutoRefresh);
        }
        else
        {
            AutoRefresh = true;
        }
    }
}
