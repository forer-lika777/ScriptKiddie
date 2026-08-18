using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ScriptKiddie.WinUI.Models;
using ScriptKiddie.WinUI.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ScriptKiddie.WinUI.Mocks;

public class MockHttpClientProvider : IHttpClientProvider
{
    private readonly SelectScheduleProvider selectScheduleProvider = App.Current.Services.GetRequiredService<SelectScheduleProvider>();
    private readonly ILogger<MockHttpClientProvider> logger;

    private CookieCollection cookies = [];
    private CourseResponse? selectableCourses = null;
    private List<CourseItem>? selectedCourses = null;

    public MockHttpClientProvider(ILogger<MockHttpClientProvider> logger)
    {
        this.logger = logger;
        SetSelectableCourses();
        SetSelectedCourses();
        _ = SimulateSelectedCountChanged(CancellationToken.None);
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

    public async Task<int> FetchCourseSelectLimitCountAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(1000, cancellationToken);
        return 2;
    }

    public async Task<CourseResponse> FetchSelectableCoursesAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(700, cancellationToken);
        return selectableCourses!;
    }

    public async Task<List<CourseItem>> FetchSelectedCoursesAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(1000, cancellationToken);
        return selectedCourses!;
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
                        Content = new StringContent("不允许选课")
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
                        Content = new StringContent("不允许退选")
                    };
                }

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
