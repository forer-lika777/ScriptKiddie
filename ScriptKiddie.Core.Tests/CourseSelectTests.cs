using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using ScriptKiddie.Core.Mocks;
using ScriptKiddie.Core.Models;
using ScriptKiddie.Core.Services;
using ScriptKiddie.Core.ViewModels;

namespace ScriptKiddie.Core.Tests;

[TestClass]
public sealed class CourseSelectTests
{
    public TestContext TestContext { get; set; } = null!;
    public IServiceProvider Services { get; private set; } = null!;
    private ILogger<CourseSelectTests> logger = null!;
    private WeakReferenceMessenger messenger = null!;

    [TestInitialize]
    public void Setup()
    {
        var services = new ServiceCollection();

        // 1) 日志：输出到控制台 + 接到 TestContext
        services.AddLogging(b =>
        {
            b.AddConsole();
            b.AddProvider(new TestContextLoggerProvider(TestContext));
            b.SetMinimumLevel(LogLevel.Debug);
        });

        // 2) 注册被测服务（按需改成你的类）
        services.AddSingleton<ICourseSelectService, CourseSelectService>();
        services.AddSingleton<IHttpClientProvider, MockHttpClientProvider>();
        services.AddSingleton<CourseListPageModel>();
        services.AddSingleton<IAccountManageService, AccountManageService>();

        services.AddSingleton<ISelectScheduleProvider, SelectScheduleProvider>();

        //var loginService = new Mock<ILoginService>();
        services.AddSingleton<ILoginService, MockLoginService>();

        var settingService = new Mock<IAppSettingsService>();
        settingService.Setup(a => a.AccountInfo.Value).Returns(() => new AccountInfo());
        settingService.Setup(a => a.SelectSchedules.Value).Returns(() => []);
        settingService.Setup(a => a.Cookies.Value).Returns(() => []);
        settingService.Setup(a => a.Password.Value).Returns(() => string.Empty);
        settingService.Setup(a => a.IsLoggedIn.Value).Returns(() => true);
        services.AddSingleton<IAppSettingsService>(settingService.Object);

        messenger = new WeakReferenceMessenger();
        services.AddSingleton<IMessenger>(messenger);

        Services = services.BuildServiceProvider();
        //AppServices.Initialize(Services);

        logger = Services.GetRequiredService<ILogger<CourseSelectTests>>();
    }

    private SelectSchedule SetUpFutureSelectSchedule()
    {
        var now = DateTime.Now;

        var startTime = now.AddSeconds(5);
        var endTime = startTime.AddHours(6);

        var time = new ScheduleTime(startTime, endTime);
        var schedule = new SelectSchedule(time, "5 秒后开始的时间表");

        Services.GetRequiredService<ISelectScheduleProvider>().Add(schedule);

        return schedule;
    }

    private SelectSchedule SetUpOpenedSelectSchedule()
    {
        var now = DateTime.Now;

        var startTime = now.AddHours(-1);
        var endTime = startTime.AddHours(6);

        var time = new ScheduleTime(startTime, endTime);
        var schedule = new SelectSchedule(time, "已经开始一小时的时间表");

        Services.GetRequiredService<ISelectScheduleProvider>().Add(schedule);

        return schedule;
    }

    [TestMethod]
    public async Task SelectCourseFutureSelectScheduleTest()
    {
        var schedule = SetUpFutureSelectSchedule();

        var course = new CourseItem("12345678", "何意味何意味何意味", "23333333");

        await Services.GetRequiredService<ICourseSelectService>().AddCourseAsync(course, schedule, OperationType.Select);

        var tasks = Services.GetRequiredService<ICourseSelectService>().SelectTasks;

        var task = tasks![tasks.Count - 1];

        Assert.AreEqual(SelectStatus.Pending, task?.SelectStatus);

        await Task.Delay(3500);

        Assert.AreEqual(SelectStatus.Executing, task?.SelectStatus);

        await Task.Delay(5000);

        Assert.AreEqual(SelectStatus.Completed, task?.SelectStatus);
    }

    [TestMethod]
    public async Task WithdrawCourseFutureSelectScheduleTest()
    {
        var schedule = SetUpFutureSelectSchedule();

        var course = new CourseItem("12345678", "何意味何意味何意味", "23333333");

        Services.GetRequiredService<IHttpClientProvider>().SetSelectedCourses([course]);

        await Services.GetRequiredService<ICourseSelectService>().AddCourseAsync(course, schedule, OperationType.Withdraw);

        var tasks = Services.GetRequiredService<ICourseSelectService>().SelectTasks;

        var task = tasks![tasks.Count - 1];

        Assert.AreEqual(SelectStatus.Pending, task?.SelectStatus);

        await Task.Delay(5100);

        Assert.AreEqual(SelectStatus.Executing, task?.SelectStatus);

        await Task.Delay(1600);

        Assert.AreEqual(SelectStatus.Completed, task?.SelectStatus);
    }

    [TestMethod]
    [DataRow(OperationType.Select)]
    [DataRow(OperationType.Withdraw)]
    public async Task OpenedSelectScheduleTest(OperationType operationType)
    {
        var schedule = SetUpOpenedSelectSchedule();

        var course = new CourseItem("12345678", "何意味何意味何意味", "23333333");

        if (operationType == OperationType.Withdraw)
        {
            Services.GetRequiredService<IHttpClientProvider>().SetSelectedCourses([course]);
        }

        await Services.GetRequiredService<ICourseSelectService>().AddCourseAsync(course, schedule, operationType);

        var tasks = Services.GetRequiredService<ICourseSelectService>().SelectTasks;

        var task = tasks![tasks.Count - 1];

        Assert.AreEqual(SelectStatus.Executing, task?.SelectStatus);

        await Task.Delay(1600);

        Assert.AreEqual(SelectStatus.Completed, task?.SelectStatus);
    }

    [TestMethod]
    [DataRow(SelectType.SelectOnly, OperationType.Select, true)]
    [DataRow(SelectType.SelectOnly, OperationType.Withdraw, false)]
    [DataRow(SelectType.SelectOnly, OperationType.WithdrawToSelect, false)]
    [DataRow(SelectType.WithdrawOnly, OperationType.Withdraw, true)]
    [DataRow(SelectType.WithdrawOnly, OperationType.Select, false)]
    [DataRow(SelectType.WithdrawOnly, OperationType.WithdrawToSelect, false)]
    [DataRow(SelectType.SelectAndWithdraw, OperationType.Select, true)]
    [DataRow(SelectType.SelectAndWithdraw, OperationType.Withdraw, true)]
    [DataRow(SelectType.SelectAndWithdraw, OperationType.WithdrawToSelect, true)]
    public async Task AddCourse_ShouldValidateOperationTypeAgainstSelectType(SelectType selectType, OperationType operationType, bool expected)
    {
        // Arrange
        var schedule = SetUpFutureSelectSchedule();
        schedule.SelectType = selectType;

        bool actual = false;

        var course = new CourseItem("12345678", "何意味何意味何意味", "23333333");

        if (operationType == OperationType.WithdrawToSelect)
        {
            var courseToWithdraw = new CourseItem("sdfgsdf", "dfg", "023cvds 023023vd");
            Services.GetRequiredService<IHttpClientProvider>().SetSelectedCourses([courseToWithdraw]);
            actual = await Services.GetRequiredService<ICourseSelectService>().AddCourseAsync(course, courseToWithdraw, schedule);
        }
        else if (operationType == OperationType.Withdraw)
        {
            Services.GetRequiredService<IHttpClientProvider>().SetSelectedCourses([course]);
            actual = await Services.GetRequiredService<ICourseSelectService>().AddCourseAsync(course, schedule, operationType);
        }
        else if (operationType == OperationType.Select)
        {
            actual = await Services.GetRequiredService<ICourseSelectService>().AddCourseAsync(course, schedule, operationType);
        }

        // Assert
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    [DataRow(OperationType.Select, true)]
    [DataRow(OperationType.Select, false)]
    [DataRow(OperationType.Withdraw, true)]
    [DataRow(OperationType.Withdraw, false)]
    [DataRow(OperationType.WithdrawToSelect, true)]
    [DataRow(OperationType.WithdrawToSelect, false)]
    public async Task AddCourse_CourseListPageModel_LockSyncingStatusTest(OperationType operationType, bool autoRefreshValue)
    {
        var schedule = SetUpOpenedSelectSchedule();

        var course = new CourseItem("asdfsdf", "dsf", "bfd21b");
        var courseToWithdraw = new CourseItem("乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子", "gdf456g", "乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子");
        var samplerCourse123123123123 = new CourseItem("乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子乐子", "123g3rf6e35dg1rge+dfg+fsdg+gf", "1526gdre156edrfg156edrg156f");

        messenger.Register<RequestChooseSelectScheduleMessage>(this, (r, msg) => msg.TaskCompletionSource.SetResult(schedule));

        messenger.Register<RequestConfirmWithdrawCourseMessage>(this, (r, msg) => msg.TaskCompletionSource.SetResult(courseToWithdraw));

        Services.GetRequiredService<IHttpClientProvider>().SetSelectableCourses([course]);

        if (operationType == OperationType.Select)
        {
            Services.GetRequiredService<IHttpClientProvider>().SetSelectedCourses([]);
        }
        else if (operationType == OperationType.Withdraw)
        {
            Services.GetRequiredService<IHttpClientProvider>().SetSelectedCourses([courseToWithdraw]);
        }
        else if (operationType == OperationType.WithdrawToSelect)
        {
            Services.GetRequiredService<IHttpClientProvider>().SetSelectedCourses([courseToWithdraw, samplerCourse123123123123]);
        }

        var pageModel = Services.GetRequiredService<CourseListPageModel>();
        await pageModel.SyncCoursesContentCommand.ExecuteAsync(this);

        pageModel.AutoRefresh = autoRefreshValue;

        if (operationType == OperationType.Select)
        {
            //Services.GetRequiredService<IHttpClientProvider>().SetSelectedCourses([]);
            await pageModel.AddCourseCommand.ExecuteAsync(course);

            Assert.IsTrue(pageModel.CanModifyAutoRefresh);
            Assert.AreEqual(autoRefreshValue, pageModel.AutoRefresh);

            await Task.Delay(5000, TestContext.CancellationToken);

            Assert.IsTrue(pageModel.CanModifyAutoRefresh);
            Assert.AreEqual(autoRefreshValue, pageModel.AutoRefresh);
        }
        else if (operationType == OperationType.Withdraw)
        {
            //Services.GetRequiredService<IHttpClientProvider>().SetSelectedCourses([courseToWithdraw]);
            await pageModel.WithdrawCourseCommand.ExecuteAsync(courseToWithdraw);

            Assert.IsTrue(pageModel.CanModifyAutoRefresh);
            Assert.AreEqual(autoRefreshValue, pageModel.AutoRefresh);

            await Task.Delay(5000, TestContext.CancellationToken);

            Assert.IsTrue(pageModel.CanModifyAutoRefresh);
            Assert.AreEqual(autoRefreshValue, pageModel.AutoRefresh);
        }
        else if (operationType == OperationType.WithdrawToSelect)
        {
            //Services.GetRequiredService<IHttpClientProvider>().SetSelectedCourses([courseToWithdraw, samplerCourse123123123123]);
            await pageModel.AddCourseCommand.ExecuteAsync(course);

            Assert.IsFalse(pageModel.CanModifyAutoRefresh);
            Assert.IsTrue(pageModel.AutoRefresh);

            await Task.Delay(5000, TestContext.CancellationToken);

            Assert.IsTrue(pageModel.CanModifyAutoRefresh);
            Assert.AreEqual(autoRefreshValue, pageModel.AutoRefresh);
        }
    }


}
