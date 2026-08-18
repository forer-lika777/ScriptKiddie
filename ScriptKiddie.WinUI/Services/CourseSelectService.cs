using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using ScriptKiddie.WinUI.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;

namespace ScriptKiddie.WinUI.Services;

/// <summary>
/// 选课服务。此服务类实现较复杂，并且未经实际测试验证。
/// </summary>
public partial class CourseSelectService : ICourseSelectService, IRecipient<SelectScheduleRemoveMessage>
{
    private readonly IHttpClientProvider httpClientProvider;
    private readonly SelectScheduleProvider selectScheduleProvider;
    private readonly ILogger<CourseSelectService> logger;

    private readonly SemaphoreSlim refreshSemaphore = new SemaphoreSlim(0, int.MaxValue);
    private readonly SemaphoreSlim taskLock = new SemaphoreSlim(1, 1);
    private CancellationTokenSource? refreshCts;
    //private bool isRefreshing = false;

    private readonly ObservableCollection<SelectSchedule> selectSchedules;

    //private static readonly SemaphoreSlim _syncSemaphore = new SemaphoreSlim(1, 1);

    private readonly ObservableCollection<CourseSelectTask> selectTasks = [];

    private const string BASE_URL = "https://jxfw.gdut.edu.cn";
    private const string CAPTCHA_PAGE_URL = BASE_URL + "/waf_text_verify.html";

    private CourseResponse? selectableCourses = null;
    private List<CourseItem>? selectedCourses = null;

    private CancellationTokenSource? syncSelectableCoursesCts = null;
    //private CancellationTokenSource? syncSelectedCoursesCts = null;

    //private bool isSyncingCourses = false;
    private bool isInternalSyncingCourses = false;

    //private bool isExecutingTask = false;

    //private volatile int activeTaskCount = 0;

    private event EventHandler? SelectableCoursesChanged;
    private event EventHandler? SelectedCoursesChanged;

    private int selectCountLimit;

    public CourseSelectService(IHttpClientProvider httpClientProvider, ILogger<CourseSelectService> logger, SelectScheduleProvider selectScheduleProvider)
    {
        this.httpClientProvider = httpClientProvider;
        this.logger = logger;
        this.selectScheduleProvider = selectScheduleProvider;
        selectSchedules = selectScheduleProvider.SelectSchedules;
        WeakReferenceMessenger.Default.Register(this);
    }

    public async Task<CourseResponse?> GetSelectableCoursesAsync(CancellationToken cancellationToken)
    {
        await RefreshSelectableCoursesAsync(cancellationToken);
        return selectableCourses;
    }

    public async Task<List<CourseItem>?> GetSelectedCoursesAsync(CancellationToken cancellationToken)
    {
        await RefreshSelectedCoursesAsync(cancellationToken);
        return selectedCourses;
    }

    public async Task BeginSyncCourses(CancellationToken cancellationToken)
    {
        if (isInternalSyncingCourses)
            return;

        if (syncSelectableCoursesCts is not null && !syncSelectableCoursesCts.IsCancellationRequested)
            return;

        ResetCts(ref syncSelectableCoursesCts);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, syncSelectableCoursesCts!.Token);

        try
        {
            while (!cts.IsCancellationRequested)
            {
                await Task.Delay(1500, cts.Token);
                await RefreshSelectableCoursesAsync(cts.Token);
                await RefreshSelectedCoursesAsync(cts.Token);
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("已停止课程列表自动更新");
        }
    }

    public async Task StopSyncCourses()
    {
        if (isInternalSyncingCourses)
            return;

        if (syncSelectableCoursesCts is null)
            return;

        CancelCts(ref syncSelectableCoursesCts);
    }

    /// <summary>
    /// 刷新可选课程列表。
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>是否刷新成功</returns>
    private async Task<bool> RefreshSelectableCoursesAsync(CancellationToken cancellationToken)
    {
        try
        {
            selectableCourses = await httpClientProvider.FetchSelectableCoursesAsync(cancellationToken);

            SelectableCoursesChanged?.Invoke(this, EventArgs.Empty);

            return true;
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("RefreshSelectableCoursesAsync was canceled.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{Message}", ex.Message);
            return false;
        }

        return false;
    }

    /// <summary>
    /// 刷新已选课程列表。
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>是否刷新成功</returns>
    private async Task<bool> RefreshSelectedCoursesAsync(CancellationToken cancellationToken)
    {
        try
        {
            selectedCourses = await httpClientProvider.FetchSelectedCoursesAsync(cancellationToken);

            SelectedCoursesChanged?.Invoke(this, EventArgs.Empty);

            return true;
        }
        catch (OperationCanceledException)
        {
            logger.LogError("RefreshSelectedCoursesAsync was canceled.");
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{Message}", ex.Message);
            return false;
        }
    }

    public ObservableCollection<CourseSelectTask> GetSelectTasks()
    {
        return selectTasks;
    }

    /// <summary>
    /// 添加一个与课程和时间表关联的选课任务到任务列表。
    /// </summary>
    /// <param name="course">要添加的课程</param>
    /// <param name="selectSchedule">要关联的时间表</param>
    /// <param name="operationType">操作类型（选课、退选）</param>
    public bool AddCourse(CourseItem course, SelectSchedule selectSchedule, OperationType operationType)
    {
        if (selectTasks.Any(task => task.Course.Equals(course) && (task.SelectStatus == SelectStatus.Pending || task.SelectStatus == SelectStatus.Executing)))
        {
            ReportAddCourseError("已存在该课程的任务。");
            return false;
        }

        if (!selectSchedules.Contains(selectSchedule))
        {
            ReportAddCourseError("没有找到时间表。");
            return false;
        }

        if (selectSchedule.SelectType == SelectType.WithdrawOnly)
        {
            if (operationType == OperationType.Select || operationType == OperationType.WithdrawToSelect)
            {
                ReportAddCourseError("操作类型与时间表的选择方式不匹配。");
                return false;
            }
        }

        if (selectSchedule.SelectType == SelectType.SelectOnly)
        {
            if (operationType == OperationType.Withdraw || operationType == OperationType.WithdrawToSelect)
            {
                ReportAddCourseError("操作类型与时间表的选择方式不匹配。");
                return false;
            }
        }

        var now = DateTime.Now;
        if (selectSchedule.ScheduleTime.EndTime < now)
        {
            ReportAddCourseError("无法关联到已经结束的时间表。");
            return false;
        }

        var task = new CourseSelectTask(selectSchedule, course, SelectStatus.Pending, operationType);
        selectTasks.Add(task);

        _ = ExcuteTask(task);

        return true;
    }

    /// <summary>
    /// 添加一个与课程和时间表关联的选课任务到任务列表。
    /// </summary>
    /// <param name="course">要添加的课程</param>
    /// <param name="courseToWithdraw">要替换的课程</param>
    /// <param name="selectSchedule">要关联的时间表</param>
    public bool AddCourse(CourseItem course, CourseItem courseToWithdraw, SelectSchedule selectSchedule)
    {
        if (selectTasks.Any(task => task.Course.Equals(course) && (task.SelectStatus == SelectStatus.Pending || task.SelectStatus == SelectStatus.Executing)))
        {
            ReportAddCourseError("已存在该课程的任务。");
            return false;
        }

        if (!selectSchedules.Contains(selectSchedule))
        {
            ReportAddCourseError("没有找到时间表。");
            return false;
        }

        if (selectSchedule.SelectType != SelectType.SelectAndWithdraw)
        {
            ReportAddCourseError("关联的时间表不支持此操作类型。");
            return false;
        }

        var now = DateTime.Now;
        if (selectSchedule.ScheduleTime.EndTime < now)
        {
            ReportAddCourseError("无法关联到已经结束的时间表。");
            return false;
        }

        var task = new CourseSelectTask(selectSchedule, course, courseToWithdraw, SelectStatus.Pending);
        selectTasks.Add(task);

        _ = ExcuteTask(task);

        return true;
    }

    private async void ReportAddCourseError(string message)
    {
        logger.LogError("添加课程失败：{message}", message);
        //await Task.Delay(10);
        WeakReferenceMessenger.Default.Send(new TaskAddFailedMessage(message));
    }

    /// <summary>
    /// 将与课程关联的任务从任务列表中移除。
    /// </summary>
    /// <param name="course">要移除的课程</param>
    public bool RemoveCourse(CourseItem course)
    {
        // 先取消，再一次性删除
        var tasksToRemove = selectTasks.Where(t => t.Course.Equals(course)).ToList();

        if (tasksToRemove.Count == 0)
        {
            logger.LogError("删除失败：没有在任务列表中找到匹配的课程项。");
            return false;
        }

        foreach (var task in tasksToRemove)
        {
            task.Cts.Cancel();
            selectTasks.Remove(task);
        }

        return true;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0305:简化集合初始化", Justification = "<挂起>")]
    public async void Receive(SelectScheduleRemoveMessage message)
    {
        var changedSelectSchedules = message.ChangedSelectSchedules;
        var tasksToRemove = new List<CourseSelectTask>();

        tasksToRemove.AddRange(selectTasks.Where(task => changedSelectSchedules.Contains(task.SelectSchedule)));

        var tcs = message.TaskCompletionSource;

        // 接受方：CourseListPage
        WeakReferenceMessenger.Default.Send<SelectScheduleRemoveConfirmMessage>(new SelectScheduleRemoveConfirmMessage(tasksToRemove, tcs));

        if (tasksToRemove.Count == 0)
            return;

        try
        {
            await tcs.Task;
        }
        catch (OperationCanceledException)
        {
            return;
        }

        foreach (var task in tasksToRemove)
        {
            task.Cts.Cancel();
            selectTasks.Remove(task);
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0305:简化集合初始化", Justification = "<挂起>")]
    private async Task UpdateTasks(TaskCompletionSource tcs)
    {
        var tasksToRemove = new List<CourseSelectTask>();

        tasksToRemove.AddRange(selectTasks.Where(task => !selectSchedules.Contains(task.SelectSchedule)).ToList());
        tasksToRemove.AddRange(selectTasks.Where(task => selectedCourses?.Count < selectCountLimit && task.OperationType == OperationType.WithdrawToSelect));

        WeakReferenceMessenger.Default.Send<SelectScheduleRemoveConfirmMessage>(new SelectScheduleRemoveConfirmMessage(tasksToRemove, tcs));

        try
        {
            await tcs.Task;
        }
        catch (OperationCanceledException)
        {
            return;
        }

        foreach (var task in tasksToRemove)
        {
            task.Cts.Cancel();
            selectTasks.Remove(task);
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0305:简化集合初始化", Justification = "<挂起>")]
    private void UpdateTasks()
    {
        var tasksToRemove = new List<CourseSelectTask>();

        tasksToRemove.AddRange(selectTasks.Where(task => !selectSchedules.Contains(task.SelectSchedule)).ToList());
        tasksToRemove.AddRange(selectTasks.Where(task => selectedCourses?.Count < selectCountLimit && task.OperationType == OperationType.WithdrawToSelect));

        foreach (var task in tasksToRemove)
        {
            task.Cts.Cancel();
            selectTasks.Remove(task);
        }
    }

    private async Task ExcuteTask(CourseSelectTask task)
    {
        var now = DateTime.Now;
        try
        {
            logger.LogInformation("已设定选课任务：{hash}。目标课程：{Course}，设定时间：{Time}，操作类型：{Type}", task.GetHashCode(), task.Course.ToString(), task.SelectSchedule.ScheduleTime.ToString(), task.OperationType.ToString());

            if (now < task.SelectSchedule.ScheduleTime.StartTime.AddSeconds(-3))
            {
                var time = task.SelectSchedule.ScheduleTime.StartTime.AddSeconds(-3) - now;
                logger.LogInformation("等待 {Seconds} 执行选课任务。", time.TotalSeconds);
                task.SelectStatus = SelectStatus.Pending;
                await Task.Delay(time, task.Cts.Token);

                if (task.OperationType == OperationType.Select)
                {
                    task.SelectStatus = SelectStatus.Executing;
                    if (await ConcurrentRequest(task.Course, task.Cts.Token))
                        task.SelectStatus = SelectStatus.Completed;
                }
                else
                {
                    await Task.Delay(3000);
                    task.SelectStatus = SelectStatus.Executing;

                    if (task.OperationType == OperationType.Withdraw)
                    {
                        if (await SequentialRequest(task.Course, task.OperationType, task.Cts.Token))
                        {
                            task.SelectStatus = SelectStatus.Completed;
                            await RefreshSelectedCoursesAsync(CancellationToken.None);
                        }
                    }
                    else if (task.OperationType == OperationType.WithdrawToSelect)
                    {
                        if (task.CourseToWithdraw == null)
                            throw new NullReferenceException(nameof(task.CourseToWithdraw));

                        if (await WaitForRefreshThenCheck(task.CourseToWithdraw, task.Course, task.Cts.Token))
                        {
                            task.SelectStatus = SelectStatus.Completed;
                            await RefreshSelectedCoursesAsync(CancellationToken.None);
                        }
                    }
                }
            }
            else if (now >= task.SelectSchedule.ScheduleTime.StartTime.AddSeconds(-3) && now < task.SelectSchedule.ScheduleTime.EndTime)
            {
                logger.LogInformation("开始选课时间已过。");

                task.SelectStatus = SelectStatus.Executing;

                if (task.OperationType == OperationType.Select || task.OperationType == OperationType.Withdraw)
                {
                    if (await SequentialRequest(task.Course, task.OperationType, task.Cts.Token))
                    {
                        task.SelectStatus = SelectStatus.Completed;
                        await RefreshSelectedCoursesAsync(CancellationToken.None);
                    }
                }
                else if (task.OperationType == OperationType.WithdrawToSelect)
                {
                    if (task.CourseToWithdraw == null)
                        throw new NullReferenceException(nameof(task.CourseToWithdraw));

                    if (await WaitForRefreshThenCheck(task.CourseToWithdraw, task.Course, task.Cts.Token))
                    {
                        task.SelectStatus = SelectStatus.Completed;
                        await RefreshSelectedCoursesAsync(CancellationToken.None);
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("操作已终止。");
            task.SelectStatus = SelectStatus.Canceled;
            return;
        }
        catch (Exception ex)
        {
            logger.LogError("引发了未知异常：{Message}", ex.Message);
            task.SelectStatus = SelectStatus.Failed;
            return;
        }
    }

    /// <summary>
    /// 任务调用：等待下一次刷新完成，然后检查自己的课程
    /// </summary>
    private async Task<bool> WaitForRefreshThenCheck(CourseItem course, CourseItem courseToWithdraw, CancellationToken cancellationToken)
    {
        // 如果循环还没启动，启动它
        RequestRefreshCourses(cancellationToken);

        while (!cancellationToken.IsCancellationRequested)
        {
            // 等待下一次刷新完成
            await refreshSemaphore.WaitAsync(cancellationToken);

            // 数据已刷新，检查自己的课程
            var courses = selectableCourses; // 读取引用（原子操作）
            var targetCourse = courses?.Rows.FirstOrDefault(c => c.Equals(course));
            //var targetCourse = selectableCourses?.Rows.FirstOrDefault(c => c.Equals(course));

            if (targetCourse == null)
            {
                logger.LogError("没有在可选课程列表中找到目标课程。");
                return false;
            }

            if (!targetCourse.IsFull)
            {
                // 课程有空位，执行选课
                return await TryWithdrawAndSelect(courseToWithdraw, course, cancellationToken);
            }

            // 课程还满着，继续等待下一次刷新
            logger.LogDebug("课程 {Course} 仍满，等待下一次刷新", course);
        }

        return false;
    }

    private async Task<bool> TryWithdrawAndSelect(CourseItem courseToWithdraw, CourseItem selectCourse, CancellationToken cancellationToken)
    {
        if (!await SequentialRequest(courseToWithdraw, OperationType.Withdraw, cancellationToken, 5))
        {
            logger.LogError("退选 {Course} 失败。请检查错误信息。", courseToWithdraw);
            return false;
        }

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(4));

        bool selectSuccess = false;
        try
        {
            selectSuccess = await ConcurrentRequest(selectCourse, cts.Token, 50, 50, 5);
        }
        catch (OperationCanceledException)
        {
            // 超时（4 秒到了）或外部取消
            logger.LogError("抢课超时（4 秒），或任务被取消。准备回滚...");
            selectSuccess = false;
        }

        if (!selectSuccess)
        {
            logger.LogWarning("退选成功，但抢课失败（超时/网络/其他）。尝试选回原有课程...");

            if (!await SequentialRequest(courseToWithdraw, OperationType.Select, CancellationToken.None, 5, 100))
            {
                logger.LogError("警告：原有课程选择失败！（这个情况应该不会发生吧。如果发生了...那不能怪我，是你自己要用的。嘻嘻~~）");
                return false;
            }

            logger.LogInformation("成功选回原有课程。");
            return false;
        }

        logger.LogInformation("成功退选并选择目标课程。");
        return true;
    }

    /// <summary>
    /// 启动刷新循环（由第一个任务触发）
    /// </summary>
    /// <param name="cancellationToken"></param>
    private void RequestRefreshCourses(CancellationToken cancellationToken)
    {
        taskLock.Wait(cancellationToken);
        try
        {
            if (isInternalSyncingCourses)
                return;
            isInternalSyncingCourses = true;
            refreshCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _ = RefreshLoopAsync(refreshCts.Token);
        }
        finally
        {
            taskLock.Release();
        }
    }

    /// <summary>
    /// 刷新循环：不断请求最新数据，每次请求完成就通知等待的任务
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task RefreshLoopAsync(CancellationToken cancellationToken)
    {
        int failTime = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            // 刷新一次数据
            if (!await RefreshSelectableCoursesAsync(cancellationToken))
            {
                failTime++;
                logger.LogError("更新数据失败，请检查错误信息。当前失败次数为 {Count}", failTime);
                if (failTime > 10)
                {
                    logger.LogError("已达失败次数限制。");
                    break;
                }

                await Task.Delay(3000, cancellationToken);

                continue;
            }

            failTime = 0;

            // 通知所有等待的任务：数据更新了
            refreshSemaphore.Release(int.MaxValue);

            await Task.Delay(1500, cancellationToken);
        }

        try
        {
            // 循环结束，重置状态
            taskLock.Wait(cancellationToken);
            isInternalSyncingCourses = false;
        }
        catch (OperationCanceledException)
        {
            // 取消触发，直接返回，不重置状态
            return;
        }
        finally
        {
            taskLock.Release();
        }
    }

    //private async Task<bool> WithdrawToSelectCycle(CourseItem course, CourseItem courseToWithdraw, CancellationToken cancellationToken)
    //{
    //    int cycleCount = 0;

    //    while (!cancellationToken.IsCancellationRequested)
    //    {
    //        cycleCount++;

    //        await RefreshSelectableCoursesAsync(cancellationToken);

    //        var newCourse = selectableCourses!.Rows.FirstOrDefault(c => c.Equals(course));

    //        if (newCourse is null)
    //            throw new InvalidOperationException("没有在列表中找到要选择的课程！");

    //        if (newCourse.IsFull)
    //        {
    //            logger.LogInformation("目标课程已满。等待下一个周期检查。");
    //        }
    //        else
    //        {
    //            if (!await SequentialRequest(courseToWithdraw, OperationType.Withdraw, cancellationToken))
    //            {
    //                logger.LogError("没有退选成功。");
    //                return false;
    //            }

    //            logger.LogInformation("成功退选课程：{Course}", courseToWithdraw.ToString());

    //            if (await SequentialRequest(course, OperationType.Select, cancellationToken))
    //            {
    //                logger.LogInformation("成功选择课程：{Course}", course.ToString());
    //            }
    //        }

    //        logger.LogInformation("目标课程已满，选课失败。等待 2 秒重试。当前循环次数为 {Count}", cycleCount);

    //        await Task.Delay(2000, cancellationToken);
    //    }

    //    throw new OperationCanceledException();
    //}

    /// <summary>
    /// 发送并发的高频请求。仅选课。
    /// </summary>
    /// <param name="course"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="interval"></param>
    /// <param name="maxConcurrency"></param>
    /// <param name="maxAttempts"></param>
    /// <returns></returns>
    private async Task<bool> ConcurrentRequest(CourseItem course, CancellationToken cancellationToken, int maxAttempts = 700, int interval = 15, int maxConcurrency = 5)
    {
        logger.LogInformation("开始高频并发选课：{Course}，并发数 {Concurrency}，启动间隔 {Interval}ms，总上限 {MaxAttempts}", course.ToString(), maxConcurrency, interval, maxAttempts);

        var semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var tasks = new List<Task>();

        int successFlag = 0;
        int totalAttempts = 0;

        try
        {
            for (int i = 0; i < maxAttempts && !cts.IsCancellationRequested; i++)
            {
                await semaphore.WaitAsync(cts.Token);

                var task = Task.Run(async () =>
                {
                    try
                    {
                        Interlocked.Increment(ref totalAttempts);
                        var response = await httpClientProvider.SendAddCourseRequestAsync(course, cts.Token);
                        string information = await response.Content.ReadAsStringAsync(cts.Token);

                        logger.LogDebug("返回的信息：{information}", information);

                        if (information.Trim() == "1")
                        {
                            Interlocked.Exchange(ref successFlag, 1);
                            cts.Cancel();
                            logger.LogInformation("高频并发选课成功！课程：{Course}，总请求数：{Total}", course.ToString(), totalAttempts);
                            return;
                        }
                        else
                        {
                            logger.LogError("选课失败，因为：{information}", information);

                            if (information.Contains("选课人数超出，请选其他课程"))
                                logger.LogError("选课失败，因为课程人数超出。");

                            if (information.Contains("现在不是选课时间"))
                                logger.LogError("选课失败，因为现在不是选课时间。");

                            if (information.Contains("超出选课要求门数"))
                            {
                                logger.LogError("选课失败，因为超出了选课要求门数。");
                                cts.Cancel();
                            }

                            if (information.Contains("您已经选了该门课程"))
                            {
                                logger.LogError("选课失败，因为你已经选择了该课程。");
                                cts.Cancel();
                            }

                            if (response.RequestMessage?.RequestUri?.ToString() == CAPTCHA_PAGE_URL)
                            {
                                logger.LogError("要求输入验证码。");
                                WeakReferenceMessenger.Default.Send<NeedCaptchaMessage>(new NeedCaptchaMessage());
                                cts.Cancel();
                            }
                        }
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                tasks.Add(task);

                await Task.Delay(interval, cts.Token);
            }

            // 等待所有任务完成（忽略取消异常）
            await Task.WhenAll(tasks).WaitAsync(cts.Token);
        }
        catch (OperationCanceledException) { }

        return successFlag == 1;
    }

    /// <summary>
    /// 发送连续的低频请求。选课与退选。
    /// </summary>
    /// <param name="course">要操作的课程</param>
    /// <param name="operationType">操作方式（选课、退选）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <param name="countLimit">次数限制，设为 0 表示无限制</param>
    /// <param name="interval">请求间隔</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private async Task<bool> SequentialRequest(CourseItem course, OperationType operationType, CancellationToken cancellationToken, int countLimit = 0, int interval = 2000)
    {
        if (countLimit < 0)
            throw new ArgumentException("限制次数不能小于零：" + countLimit);

        if (operationType == OperationType.WithdrawToSelect)
            throw new ArgumentException($"输入的的操作种类枚举不能为 {OperationType.WithdrawToSelect}");

        logger.LogInformation("即将开始选择课程：{Course}。间隔：{Interval}，限制次数：{LimitCount}", course.ToString(), interval, countLimit);

        int cycleCount = 0;

        while (countLimit == 0 || cycleCount < countLimit)
        {
            if (operationType == OperationType.Select)
            {
                var response = await httpClientProvider.SendAddCourseRequestAsync(course, cancellationToken);

                string information = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogDebug("返回的信息：{inmformation}", information);

                if (information.Trim() == "1") // 选课成功返回1！你是真幽默，返回个1！
                {
                    logger.LogInformation("成功请求添加课程操作。");
                    return true;
                }
                else
                {
                    logger.LogError("选课失败，因为：{information}", information);
                    if (information.Contains("选课人数超出，请选其他课程"))
                    {
                        logger.LogError("选课失败，因为课程人数超出。");
                    }

                    if (information.Contains("超出选课要求门数"))
                    {
                        logger.LogError("选课失败，因为超出了选课要求门数。");
                        return false;
                    }

                    if (information.Contains("您已经选了该门课程"))
                    {
                        logger.LogError("选课失败，因为你已经选择了该课程。");
                        return false;
                    }

                    if (response.RequestMessage?.RequestUri?.ToString() == CAPTCHA_PAGE_URL)
                    {
                        logger.LogError("要求输入验证码。");
                        WeakReferenceMessenger.Default.Send<NeedCaptchaMessage>(new NeedCaptchaMessage());
                        return false;
                    }
                }
            }
            else if (operationType == OperationType.Withdraw)
            {
                var response = await httpClientProvider.SendWithdrawCourseRequestAsync(course, cancellationToken);

                string information = await response.Content.ReadAsStringAsync();

                logger.LogDebug("返回的信息：{inmformation}", information);

                if (information.Trim() == "1")
                {
                    logger.LogInformation("成功请求退选课程操作。");
                    return true;
                }
                else
                {
                    logger.LogError("退选失败，因为：{information}", information);
                    if (response.RequestMessage?.RequestUri?.ToString() == CAPTCHA_PAGE_URL)
                    {
                        logger.LogError("要求输入验证码。");
                        WeakReferenceMessenger.Default.Send<NeedCaptchaMessage>(new NeedCaptchaMessage());
                        return false;
                    }

                    return false;
                }
            }

            cycleCount++;

            logger.LogInformation("选课失败。等待 {Interval} 毫秒重试。当前循环次数为 {Count}", interval, cycleCount);

            await Task.Delay(interval, cancellationToken);
        }

        logger.LogError("选课失败，已达最大次数限制：{Limit}", countLimit);
        return false;
    }

    public async Task<int?> GetSelectLimitCountAsync(CancellationToken cancellationToken)
    {
        if (selectCountLimit != 0)
            return selectCountLimit;

        selectCountLimit = await httpClientProvider.FetchCourseSelectLimitCountAsync(cancellationToken);

        return selectCountLimit;
    }

    private static void ResetCts(ref CancellationTokenSource? cts, CancellationTokenSource? ctsToUse = null)
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = ctsToUse ?? new CancellationTokenSource();
    }

    private static void CancelCts(ref CancellationTokenSource? cts)
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = null;
    }
}
