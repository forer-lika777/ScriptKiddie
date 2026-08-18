using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using ScriptKiddie.WinUI.Models;
using ScriptKiddie.WinUI.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ScriptKiddie.WinUI.Mocks;

public class MockCourseSelectService : ICourseSelectService, IRecipient<SelectScheduleRemoveMessage>
{
    private readonly IHttpClientProvider httpClientProvider;
    private readonly ILogger<MockCourseSelectService> logger;
    private readonly SelectScheduleProvider selectScheduleProvider;

    private CourseResponse? selectableCourses = null;
    private List<CourseItem>? selectedCourses = null;

    private ObservableCollection<CourseSelectTask> selectTasks = [];

    private CancellationTokenSource? cancellationTokenSource;

    public MockCourseSelectService(IHttpClientProvider httpClientProvider, ILogger<MockCourseSelectService> logger, SelectScheduleProvider selectScheduleProvider)
    {
        this.httpClientProvider = httpClientProvider;
        this.logger = logger;
        this.selectScheduleProvider = selectScheduleProvider;
        SetSelectableCourses();
        SetSelectedCourses();
    }

    private void SetSelectableCourses()
    {
        string content = File.ReadAllText(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Mocks", "Data", "SelectableCoursesData.json"));
        selectableCourses = (CourseResponse?)JsonSerializer.Deserialize(content, typeof(CourseResponse), CourseResponseJsonContext.Default);
    }

    private void SetSelectedCourses()
    {
        string content = File.ReadAllText(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Mocks", "Data", "SelectedCoursesData.json"));
        selectedCourses = (List<CourseItem>?)JsonSerializer.Deserialize(content, typeof(List<CourseItem>), CourseItemListJsonContext.Default);
    }

    public Task AddCourseSelectPlan(CourseItem course, DateTime openTime, CancellationToken cancellationToken, int interval = 2000)
    {
        throw new NotImplementedException();
    }

    public async Task<CourseResponse?> GetSelectableCoursesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(700, cancellationToken);
            return selectableCourses;
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("请求已终止。");
            return null;
        }
    }

    public async Task<List<CourseItem>?> GetSelectedCoursesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(700, cancellationToken);
            return selectedCourses;
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("请求已终止。");
            return null;
        }
    }

    private async Task SimulateSelectedCountChanged(CancellationToken cancellationToken)
    {
        try
        {
            while (true)
            {
                bool completed = true;
                await Task.Delay(1500, cancellationToken);
                foreach (var course in selectableCourses!.Rows)
                {
                    if (int.Parse(course.SelectedStudentCount!) < int.Parse(course.PlannedStudentCount!))
                    {
                        int count = int.Parse(course.SelectedStudentCount!);
                        count++;
                        course.SelectedStudentCount = count.ToString();
                        completed = false;
                    }
                }

                if (completed)
                    return;
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("请求已终止。");
        }
    }

    public async Task<int?> GetSelectLimitCountAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(1000, cancellationToken);
            return 2;
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("请求已终止。");
            return null;
        }
    }

    public async Task BeginSyncCourses(CancellationToken cancellationToken)
    {
        ResetCts(ref cancellationTokenSource);
        _ = SimulateSelectedCountChanged(cancellationTokenSource!.Token);
    }

    public async Task StopSyncCourses()
    {
        CancelCts(ref cancellationTokenSource);
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

    public bool AddCourse(CourseItem course, SelectSchedule selectSchedule, OperationType operationType)
    {
        throw new NotImplementedException();
    }

    public bool RemoveCourse(CourseItem course)
    {
        throw new NotImplementedException();
    }

    public ObservableCollection<CourseSelectTask> GetSelectTasks()
    {
        throw new NotImplementedException();
    }

    public bool AddCourse(CourseItem course, CourseItem courseToWithdraw, SelectSchedule selectSchedule)
    {
        throw new NotImplementedException();
    }

    public void Receive(SelectScheduleRemoveMessage message)
    {

    }
}
