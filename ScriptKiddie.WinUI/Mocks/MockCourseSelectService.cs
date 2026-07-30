using Microsoft.Extensions.Logging;
using ScriptKiddie.WinUI.Models;
using ScriptKiddie.WinUI.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ScriptKiddie.WinUI.Mocks;

public class MockCourseSelectService : ICourseSelectService
{
    private readonly HttpClientProvider httpClientProvider;
    private readonly ILogger<MockCourseSelectService> logger;

    private CourseResponse? selectableCourses = null;
    private List<CourseItem>? selectedCourses = null;

    public MockCourseSelectService(HttpClientProvider httpClientProvider, ILogger<MockCourseSelectService> logger)
    {
        this.httpClientProvider = httpClientProvider;
        this.logger = logger;
        SetSelectableCourses();
        SetSelectedCourses();
        _ = SimulateSelectedCountChanged();
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
        await Task.Delay(700, cancellationToken);
        return selectableCourses;
    }

    public async Task<List<CourseItem>?> GetSelectedCoursesAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(700, cancellationToken);
        return selectedCourses;
    }

    private async Task SimulateSelectedCountChanged()
    {
        while (true)
        {
            bool completed = true;
            await Task.Delay(1500);
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

    public async Task<int?> GetSelectLimitCountAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(1000, cancellationToken);
        return 2;
    }
}
