using Microsoft.Extensions.Logging;
using ScriptKiddie.Core.Models;
using ScriptKiddie.Core.Services;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ScriptKiddie.Core.Mocks;

public class MockHttpClientProvider : IHttpClientProvider
{
    private readonly ISelectScheduleProvider selectScheduleProvider;
    private readonly ILogger<MockHttpClientProvider> logger;
    private readonly IUiDispatcher dispatcher;

    private CookieCollection cookies = [];
    private ObservableCollection<CourseItem>? selectableCourses = null;
    private ObservableCollection<CourseItem>? selectedCourses = null;

    public MockHttpClientProvider(ILogger<MockHttpClientProvider> logger, ISelectScheduleProvider selectScheduleProvider, IUiDispatcher dispatcher)
    {
        this.logger = logger;
        this.selectScheduleProvider = selectScheduleProvider;
        this.dispatcher = dispatcher;
        SetSelectableCourses();
        SetSelectedCourses();
        _ = SimulateSelectedCountChanged(CancellationToken.None);
    }

    private async void SetSelectableCourses()
    {
        string content = File.ReadAllText(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Mocks", "Data", "SelectableCoursesData.json"));
        selectableCourses = (ObservableCollection<CourseItem>?)JsonSerializer.Deserialize(content, typeof(ObservableCollection<CourseItem>), CourseItemListJsonContext.Default);
        await SyncActualSideSelectedCountToMockSide(selectableCourses!);
    }

    private async void SetSelectedCourses()
    {
        string content = File.ReadAllText(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Mocks", "Data", "SelectedCoursesData.json"));
        selectedCourses = (ObservableCollection<CourseItem>?)JsonSerializer.Deserialize(content, typeof(ObservableCollection<CourseItem>), CourseItemListJsonContext.Default);
        await SyncActualSideSelectedCountToMockSide(selectedCourses!);
    }

    public async void SetSelectableCourses(ObservableCollection<CourseItem> courses)
    {
        selectableCourses = new ObservableCollection<CourseItem>(courses);
        await SyncActualSideSelectedCountToMockSide(selectableCourses!);
    }

    public async void SetSelectedCourses(ObservableCollection<CourseItem> courses)
    {
        selectedCourses = new ObservableCollection<CourseItem>(courses);
        await SyncActualSideSelectedCountToMockSide(selectedCourses!);
    }

    private async Task SimulateSelectedCountChanged(CancellationToken cancellationToken)
    {
        try
        {
            while (true)
            {
                await Task.Delay(3000, cancellationToken);

                bool completed = true;

                foreach (var course in selectableCourses!)
                {
                    if (int.Parse(course.MockSideSelectedStudentCount!) < int.Parse(course.PlannedStudentCount!))
                    {
                        int count = int.Parse(course.MockSideSelectedStudentCount!);
                        count++;
                        course.MockSideSelectedStudentCount = count.ToString();
                        completed = false;
                    }
                }

                if (completed)
                {
                    foreach (var course in selectableCourses!)
                    {
                        int count = int.Parse(course.MockSideSelectedStudentCount!);
                        count--;
                        course.MockSideSelectedStudentCount = count.ToString();
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("请求已终止。");
        }
    }

    public async Task<int> FetchCourseSelectLimitCountAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(1000, cancellationToken);
        return 2;
    }

    public async Task<ObservableCollection<CourseItem>?> FetchSelectableCoursesAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(700, cancellationToken);
        await SyncMockSideSelectedCountToActual(selectableCourses!);

        return selectableCourses!;
    }

    public async Task<ObservableCollection<CourseItem>> FetchSelectedCoursesAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(1000, cancellationToken);
        await SyncMockSideSelectedCountToActual(selectedCourses!);

        return selectedCourses!;
    }

    private async Task SyncMockSideSelectedCountToActual(ObservableCollection<CourseItem> courses)
    {
        foreach (var course in courses)
        {
            await dispatcher.InvokeAsync(() => course.SelectedStudentCount = course.MockSideSelectedStudentCount);
        }
    }

    private async Task SyncActualSideSelectedCountToMockSide(ObservableCollection<CourseItem> courses)
    {
        foreach (var course in courses)
        {
            await dispatcher.InvokeAsync(() => course.MockSideSelectedStudentCount = course.SelectedStudentCount);
        }
    }

    public CookieCollection GetCookies()
    {
        return cookies;
    }

    public HttpClient GetCurrentClient()
    {
        throw new NotImplementedException();
    }

    public async Task<HttpResponseMessage> SendAddCourseRequestAsync(CourseItem course, CancellationToken cancellationToken)
    {
        await Task.Delay(1500, cancellationToken);

        var selectSchedules = selectScheduleProvider.SelectSchedules;
        var now = DateTime.Now;

        foreach (var schedule in selectSchedules)
        {
            if (now >= schedule.ScheduleTime.StartTime && now <= schedule.ScheduleTime.EndTime)
            {
                if (schedule.SelectType == SelectType.WithdrawOnly)
                {
                    return new HttpResponseMessage
                    {
                        Content = new StringContent("当前不允许选课")
                    };
                }

                if (selectedCourses!.Contains(course))
                {
                    return new HttpResponseMessage
                    {
                        Content = new StringContent("您已经选了该门课程")
                    };
                }

                var c = selectableCourses?.Where(x => x.Equals(course)).FirstOrDefault();

                if (c is null)
                {
                    return new HttpResponseMessage
                    {
                        Content = new StringContent("没有找到课程。")
                    };
                }

                if (int.Parse(c.SelectedStudentCount!) >= int.Parse(c.PlannedStudentCount!))
                {
                    return new HttpResponseMessage
                    {
                        Content = new StringContent("选课人数超出，请选其他课程")
                    };
                }

                selectedCourses!.Add(course);

                return new HttpResponseMessage
                {
                    Content = new StringContent("1"),
                };
            }
        }

        return new HttpResponseMessage
        {
            Content = new StringContent("不是选课时间"),
        };
    }

    public async Task<HttpResponseMessage> SendWithdrawCourseRequestAsync(CourseItem course, CancellationToken cancellationToken)
    {
        await Task.Delay(1500, cancellationToken);

        var selectSchedules = selectScheduleProvider.SelectSchedules;
        var now = DateTime.Now;

        foreach (var schedule in selectSchedules)
        {
            if (now >= schedule.ScheduleTime.StartTime && now <= schedule.ScheduleTime.EndTime)
            {
                if (schedule.SelectType == SelectType.SelectOnly)
                {
                    return new HttpResponseMessage
                    {
                        Content = new StringContent("当前不允许退选")
                    };
                }

                var c = selectedCourses?.Where(x => x.Equals(course)).FirstOrDefault();

                if (c is null)
                {
                    return new HttpResponseMessage
                    {
                        Content = new StringContent("没有找到课程。")
                    };
                }

                selectedCourses!.Remove(course);

                return new HttpResponseMessage
                {
                    Content = new StringContent("1"),
                };
            }
        }

        return new HttpResponseMessage
        {
            Content = new StringContent("不是选课时间"),
        };
    }

    public void SetCookies(CookieCollection cookies)
    {
        this.cookies = cookies;
    }
}
