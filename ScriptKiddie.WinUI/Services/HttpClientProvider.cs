using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using ScriptKiddie.WinUI.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ScriptKiddie.WinUI.Services;

/// <summary>
/// 提供全局唯一的 HttpClient 实例，支持线程安全的 Cookie 管理。
/// </summary>
public partial class HttpClientProvider : IHttpClientProvider
{
    private readonly ReaderWriterLockSlim rwLock = new();
    private readonly ILogger<HttpClientProvider> logger;

    private HttpClient httpClient = null!;
    private HttpClientHandler? httpClientHandler = null!;

    private const string BASE_URL = "https://jxfw.gdut.edu.cn";
    private const string MAIN_PAGE_URL = BASE_URL + "/login!welcome.action";

    private const string GET_SELECTED_COURSES_URL = BASE_URL + "/xsxklist!getXzkcList.action";
    private const string GET_SELECTABLE_COURSES_URL = BASE_URL + "/xsxklist!getDataList.action";

    private const string ADD_COURSE_URL = BASE_URL + "/xsxklist!getAdd.action";
    private const string WITHDRAW_COURSE_URL = BASE_URL + "/xsxklist!getCancel.action";

    private const string GET_COURSE_SELECT_PAGE_URL = BASE_URL + "/xsxklist!xsmhxsxk.action";

    private const string CAPTCHA_PAGE_URL = BASE_URL + "/waf_text_verify.html";

    public HttpClientProvider(ILogger<HttpClientProvider> logger)
    {
        CreateInstance();
        this.logger = logger;
    }

    private void CreateInstance(CookieCollection? cookies = null)
    {
        rwLock.EnterWriteLock();
        try
        {
            httpClient?.Dispose();
            httpClientHandler?.Dispose();

            httpClientHandler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
                UseCookies = true,
                CookieContainer = new CookieContainer(),
            };

            httpClient = new HttpClient(httpClientHandler);
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/150.0.0.0 Safari/537.36 Edg/150.0.0.0");
            httpClient.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            httpClient.DefaultRequestHeaders.AcceptLanguage.ParseAdd("zh-CN,zh;q=0.9,en;q=0.8");
            httpClient.DefaultRequestHeaders.Connection.Add("keep-alive");

            if (cookies != null)
            {
                httpClientHandler.CookieContainer.Add(cookies);
            }
        }
        finally
        {
            rwLock.ExitWriteLock();
        }
    }

    public void SetCookies(CookieCollection cookies)
    {
        CreateInstance(cookies);
    }

    public CookieCollection GetCookies()
    {
        rwLock.EnterReadLock();
        try
        {
            return httpClientHandler!.CookieContainer.GetAllCookies();
        }
        finally
        {
            rwLock.ExitReadLock();
        }
    }

    public HttpClient GetCurrentClient()
    {
        rwLock.EnterReadLock();
        try
        {
            return httpClient!;
        }
        finally
        {
            rwLock.ExitReadLock();
        }
    }

    public async Task<CourseResponse> FetchSelectableCoursesAsync(CancellationToken cancellationToken)
    {
        int page = 1;
        int pageSize = 100;
        int total = -1;

        var courses = new CourseResponse();

        while (total == -1 || pageSize * (page - 1) < total)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, GET_SELECTABLE_COURSES_URL);

            var formData = new Dictionary<string, string>
            {
                { "page", page.ToString() },
                { "rows", pageSize.ToString() },
                { "sort", "kcrwdm" },
                { "order", "asc" }
            };

            using var content = new FormUrlEncodedContent(formData);

            request.Content = content;
            request.Headers.Referrer = new Uri(BASE_URL);

            using var response = await GetCurrentClient().SendAsync(request, cancellationToken);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogError("Fail to fetch selectable courses. Raw data: {responseContent}", responseContent);
                break;
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var courseResponse = await JsonSerializer.DeserializeAsync(stream, CourseResponseJsonContext.Default.CourseResponse, cancellationToken);

            if (courseResponse is null)
            {
                logger.LogError("Could not deserialize raw data to type {typeof(CourseResponseJsonContext)}. Raw data: {response.Content}", typeof(CourseResponseJsonContext), await response.Content.ReadAsStringAsync(cancellationToken));
                break;
            }

            courses.Total = total = courseResponse.Total;
            courses.Rows.AddRange(courseResponse.Rows);

            logger.LogInformation("Successfully get selectable courses. Current response count is: {count2}. Current total count is: {count3}", courseResponse.Rows.Count, courses.Total);

            page++;
        }

        return courses;
    }

    public async Task<List<CourseItem>> FetchSelectedCoursesAsync(CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, GET_SELECTED_COURSES_URL);

        var formData = new Dictionary<string, string>
        {
            { "sort", "kcrwdm" },
            { "order", "asc" },
        };

        using var content = new FormUrlEncodedContent(formData);
        request.Content = content;
        request.Headers.Referrer = new Uri(BASE_URL);

        using var response = await GetCurrentClient().SendAsync(request, cancellationToken);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogError("Fail to fetch selected courses. Raw data: {responseContent}", responseContent);
            return [];
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var courses = await JsonSerializer.DeserializeAsync(stream, CourseItemListJsonContext.Default.ListCourseItem, cancellationToken);

        if (courses is null)
        {
            logger.LogError("Could not deserialize raw data to type {Type}. Raw data: {response.Content}", typeof(List<CourseItem>), await response.Content.ReadAsStringAsync(cancellationToken));
            return [];
        }

        logger.LogInformation("Successfully request selected courses. Courses count: {Count}", courses.Count);

        return courses;
    }

    public async Task<int> FetchCourseSelectLimitCountAsync(CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, GET_COURSE_SELECT_PAGE_URL);
        request.Headers.Referrer = new Uri(MAIN_PAGE_URL);

        using var response = await GetCurrentClient().SendAsync(request, cancellationToken);

        string htmlContent = await response.Content.ReadAsStringAsync(cancellationToken);

        var doc = new HtmlDocument();
        doc.LoadHtml(htmlContent);

        // 通过 id="header" 定位到 h2 标签
        var headerNode = doc.GetElementbyId("header");
        if (headerNode == null)
        {
            logger.LogError("无法找到 header 元素。提取限选字段失败。");
            return 0;
        }

        // 获取包含文本的完整内容
        string fullText = headerNode.InnerText.Trim();

        // 使用正则表达式提取 "限选 X" 中的数字
        var match = SelectLimitCountRegex().Match(fullText);
        if (!match.Success)
        {
            logger.LogError("没有匹配到目标数字。提取限选字段失败。");
            return 0;
        }

        int count = int.Parse(match.Groups[1].Value);

        return count;
    }

    [GeneratedRegex(@"限选(?:&nbsp;|\s)*(\d+)")]
    private static partial Regex SelectLimitCountRegex();

    public async Task<HttpResponseMessage> SendAddCourseRequestAsync(CourseItem course, CancellationToken cancellationToken)
    {
        if (course.CourseTaskCode == null || course.CourseName == null)
            throw new NullReferenceException($"发送选课请求时，字段 {nameof(course.CourseTaskCode)} 和 {nameof(course.CourseName)} 是必须的，但他们当中的至少一个为 null。");

        using var request = new HttpRequestMessage(HttpMethod.Post, ADD_COURSE_URL);

        var formData = new Dictionary<string, string>
        {
            { "kcrwdm", course.CourseTaskCode },
            { "kcmc", course.CourseName },
        };

        using var content = new FormUrlEncodedContent(formData);

        request.Content = content;
        request.Headers.Referrer = new Uri(BASE_URL);

        return await GetCurrentClient().SendAsync(request, cancellationToken);
    }

    public async Task<HttpResponseMessage> SendWithdrawCourseRequestAsync(CourseItem course, CancellationToken cancellationToken)
    {
        if (course.CourseTaskCode == null || course.CourseName == null || course.TeachingClassCode == null)
            throw new NullReferenceException($"发送退选请求时，字段 {nameof(course.CourseTaskCode)}、{nameof(course.CourseName)} 和 {nameof(course.TeachingClassCode)} 是必须的，但他们当中的至少一个为 null。");

        using var request = new HttpRequestMessage(HttpMethod.Post, WITHDRAW_COURSE_URL);

        var formData = new Dictionary<string, string>
        {
            { "jxbdm", course.TeachingClassCode }, // 教学班代码，需要确认字段名
            { "kcrwdm", course.CourseTaskCode },
            { "kcmc", course.CourseName },
        };

        using var content = new FormUrlEncodedContent(formData);

        request.Content = content;
        request.Headers.Referrer = new Uri(BASE_URL);

        return await GetCurrentClient().SendAsync(request, cancellationToken);
    }
}
