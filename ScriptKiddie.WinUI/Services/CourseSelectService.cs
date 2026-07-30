using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using ScriptKiddie.WinUI.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ScriptKiddie.WinUI.Services;

public partial class CourseSelectService : ICourseSelectService
{
    private readonly HttpClientProvider httpClientProvider;
    private readonly ILogger<CourseSelectService> logger;

    private const string BASE_URL = "https://jxfw.gdut.edu.cn";
    private const string MAIN_PAGE_URL = BASE_URL + "/login!welcome.action";
    private const string GET_SELECTED_COURSES_URL = BASE_URL + "/xsxklist!getXzkcList.action";
    private const string GET_SELECTABLE_COURSES_URL = BASE_URL + "/xsxklist!getDataList.action";
    private const string ADD_COURSE_URL = BASE_URL + "/xsxklist!getAdd.action";
    private const string GET_COURSE_SELECT_PAGE_URL = BASE_URL + "/xsxklist!xsmhxsxk.action";

    private CourseResponse? selectableCourses = null;
    private List<CourseItem>? selectedCourses = null;

    public CourseSelectService(HttpClientProvider httpClientProvider, ILogger<CourseSelectService> logger)
    {
        this.httpClientProvider = httpClientProvider;
        this.logger = logger;
    }

    public async Task<CourseResponse?> GetSelectableCoursesAsync(CancellationToken cancellationToken)
    {
        await RefreshSelectableCoursesAsync(cancellationToken);
        _ = SyncSelectableCoursesAsync(cancellationToken);
        return selectableCourses;
    }

    public async Task<List<CourseItem>?> GetSelectedCoursesAsync(CancellationToken cancellationToken)
    {
        await RefreshSelectedCoursesAsync(cancellationToken);
        return selectedCourses;
    }

    private async Task SyncSelectableCoursesAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(1500, cancellationToken);
            await RefreshSelectableCoursesAsync(cancellationToken);
        }
    }

    private async Task RefreshSelectableCoursesAsync(CancellationToken cancellationToken)
    {
        int page = 1;
        int pageSize = 100;
        int total = -1;

        var courses = new CourseResponse();

        try
        {
            while (total == -1 || pageSize * (page - 1) <= total)
            {
                var request = new HttpRequestMessage(HttpMethod.Post, GET_SELECTABLE_COURSES_URL);

                var formData = new Dictionary<string, string>
                {
                    { "page", page.ToString() },
                    { "rows", pageSize.ToString() },
                    { "sort", "kcrwdm" },
                    { "order", "asc" }
                };

                var content = new FormUrlEncodedContent(formData);

                request.Content = content;
                request.Headers.Referrer = new Uri(BASE_URL);

                var response = await httpClientProvider.GetCurrentClient().SendAsync(request, cancellationToken);

                logger.LogDebug("Response: {StatusCode}", response.StatusCode);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string d = await response.Content.ReadAsStringAsync();
                    var courseResponse = await JsonSerializer.DeserializeAsync(await response.Content.ReadAsStreamAsync(cancellationToken), CourseResponseJsonContext.Default.CourseResponse, CancellationToken.None);

                    if (courseResponse == null)
                    {
                        logger.LogError("Could not deserialize raw data to type {typeof(CourseResponseJsonContext)}. Raw data: {response.Content}", typeof(CourseResponseJsonContext), response.Content.ReadAsStreamAsync(cancellationToken));
                        selectableCourses = courses.Total == -1 ? null : courses;
                        return;
                    }

                    logger.LogDebug("Successfully request selectable courses.");

                    courses.Total = total = courseResponse.Total;

                    foreach (var item in courseResponse.Rows)
                    {
                        courses.Rows.Add(item);
                    }

                    page++;
                    continue;
                }

                string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

                logger.LogError("Request returned a failure. Raw data: {responseContent}", responseContent);

                selectableCourses = courses.Total == -1 ? null : courses;
                return;
            }

            selectableCourses = courses;
        }
        catch (Exception ex)
        {
            logger.LogError("{Message}", ex.Message);
            return;
        }
    }

    private async Task RefreshSelectedCoursesAsync(CancellationToken cancellationToken)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, GET_SELECTED_COURSES_URL);

            var formData = new Dictionary<string, string>
            {
                { "sort", "kcrwdm" },
                { "order", "asc" },
            };

            var content = new FormUrlEncodedContent(formData);

            request.Content = content;
            request.Headers.Referrer = new Uri(BASE_URL);

            var response = await httpClientProvider.GetCurrentClient().SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                logger.LogDebug("Successfully request selected courses.");
                selectedCourses = await JsonSerializer.DeserializeAsync(await response.Content.ReadAsStreamAsync(cancellationToken), CourseItemListJsonContext.Default.ListCourseItem, CancellationToken.None);
                return;
            }

            logger.LogDebug("Request returned a failure. Raw data: {response.Content}", await response.Content.ReadAsStringAsync(cancellationToken));

            return;
        }
        catch (Exception ex)
        {
            logger.LogDebug("{Message}", ex.Message);
            return;
        }
    }

    public async Task AddCourseSelectPlan(CourseItem course, DateTime openTime, CancellationToken cancellationToken, int interval = 2000)
    {
        var now = DateTime.Now;
        var selectTime = openTime.AddMilliseconds(-4000);
        var delay = selectTime - now;

        if (delay.TotalHours > 24 || delay.TotalHours < -24)
        {
            logger.LogError("Cannot set a schedule that is more than 24 hours away from the current time. Please set this plan later. Time elapsed: {elapse}", delay.ToString());
            return;
        }

        await RefreshSelectableCoursesAsync(cancellationToken);
        await RefreshSelectedCoursesAsync(cancellationToken);

        if (selectableCourses == null || !selectableCourses.Rows.Any(item => item.Equals(course)))
        {
            logger.LogError("Plan cancelled because target course is not contained in the selectable courses list.");
            return;
        }
        if (selectedCourses != null && selectedCourses.Any(item => item.Equals(course)))
        {
            logger.LogError("Plan cancelled because target course has contained in the selected courses list.");
            return;
        }

        if (delay.TotalMilliseconds > 0)
        {
            logger.LogDebug("Course select will start at {selectTime}. Waiting {delay.TotalSeconds} seconds to start selecting...", selectTime, $"{delay.TotalSeconds:F1}");
            await Task.Delay(delay, cancellationToken);
        }
        else
        {
            logger.LogWarning("It seems that the opening time have passed. Start selecting immediately.");
        }

        logger.LogDebug("Start selecting: {CourseName} ({CourseTaskCode})", course.CourseName, course.CourseTaskCode);
        var result = await BeginAddCycle(course, cancellationToken, interval);

        if (result)
        {
            logger.LogDebug("Successfully selected: {CourseName}!", course.CourseName);
            logger.LogDebug($"Please wait for me to confirm select status...");
            await RefreshSelectedCoursesAsync(cancellationToken);
            if (selectedCourses == null)
            {
                logger.LogDebug("Failed to check selected courses. Please manually check it by yourself.");
                return;
            }

            if (selectedCourses.Any(item => item.Equals(course)))
            {
                logger.LogDebug("Found selected course: {CourseName}. Your course has been successfully selected!", course.CourseName);
                return;
            }
            else
            {
                logger.LogError("Course not found in your selected courses list.");
                return;
            }
        }
        else
        {
            logger.LogDebug("Select failed: {CourseName}，attempt limit reached.", course.CourseName);
            return;
        }
    }

    private async Task<bool> BeginAddCycle(CourseItem course, CancellationToken cancellationToken, int interval = 2000, int limitTimes = 1000)
    {
        int cycleTimes = 0;
        var stopWatch = new Stopwatch();

        if (course.CourseTaskCode == null || course.CourseName == null)
        {
            return false;
        }

        while (!cancellationToken.IsCancellationRequested && cycleTimes < limitTimes)
        {
            try
            {
                stopWatch.Restart();

                var request = new HttpRequestMessage(HttpMethod.Post, ADD_COURSE_URL);

                var formData = new Dictionary<string, string>
                {
                    { "kcrwdm", course.CourseTaskCode },
                    { "kcmc", course.CourseName },
                };

                var content = new FormUrlEncodedContent(formData);

                request.Content = content;
                request.Headers.Referrer = new Uri(BASE_URL);

                var response = await httpClientProvider.GetCurrentClient().SendAsync(request, cancellationToken);

                logger.LogDebug("Response: {response.StatusCode}", response.StatusCode);

                // 你妈傻逼选课失败还返回200，我操你妈还要我自己判断

                string information = await response.Content.ReadAsStringAsync();

                logger.LogDebug("{inmformation}", information);

                if (information != "1") // 选课成功返回1！你是真幽默，返回个1！
                {
                    logger.LogDebug("Select failed, because {information}", information);
                    if (information.Contains("超出选课要求门数"))
                    {
                        logger.LogError("Select failed because select count limit arrived.");
                        return false;
                    }
                    if (information.Contains("选课人数超出，请选其他课程"))
                    {
                        logger.LogDebug("Select failed because this course is full.");
                    }
                    if (information.Contains("您已经选了该门课程"))
                    {
                        logger.LogError("Select failed because you have selected this course.");
                        return false;
                    }
                }
                else
                {
                    logger.LogDebug("Successful request add course.");
                    return true;
                }

                stopWatch.Stop();
                int milliseconds = (int)stopWatch.ElapsedMilliseconds;

                if (interval > milliseconds)
                {
                    await Task.Delay((interval - milliseconds));
                }

                logger.LogDebug("Add request returned a failure. Raw data: {response.Content}", await response.Content.ReadAsStringAsync());

                cycleTimes++;
            }
            catch (Exception ex)
            {
                logger.LogDebug("{Message}", ex.Message);
                return false;
            }
        }

        return false;
    }

    public async Task<int?> GetSelectLimitCountAsync(CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, GET_COURSE_SELECT_PAGE_URL);
        request.Headers.Referrer = new Uri(MAIN_PAGE_URL);

        var response = await httpClientProvider.GetCurrentClient().SendAsync(request, cancellationToken);

        string htmlContent = await response.Content.ReadAsStringAsync(cancellationToken);

        var doc = new HtmlDocument();
        doc.LoadHtml(htmlContent);

        // 通过 id="header" 定位到 h2 标签
        var headerNode = doc.GetElementbyId("header");
        if (headerNode == null)
        {
            logger.LogError("无法找到 header 元素。提取限选字段失败。");
            return null;
        }

        // 获取包含文本的完整内容
        string fullText = headerNode.InnerText.Trim();

        // 使用正则表达式提取 "限选 X" 中的数字
        var match = SelectLimitCountRegex().Match(fullText);
        if (!match.Success)
        {
            logger.LogError("没有匹配到目标数字。提取限选字段失败。");
            return null;
        }

        return int.Parse(match.Groups[1].Value);
    }

    [GeneratedRegex(@"限选(?:&nbsp;|\s)*(\d+)")]
    private static partial Regex SelectLimitCountRegex();
}
