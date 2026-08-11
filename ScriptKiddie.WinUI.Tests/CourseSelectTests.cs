using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.AppContainer;
using Moq;
using ScriptKiddie.WinUI.Models;
using ScriptKiddie.WinUI.Services;
using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ScriptKiddie.WinUI.Tests;

[TestClass]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "MSTEST0049:将 TestContext.CancellationToken 传递给 async 操作", Justification = "<挂起>")]
public partial class CourseSelectServiceTests
{
    private TestContext testContext;

    public TestContext TestContext
    {
        get => testContext;
        set => testContext = value;
    }

    private ILogger<CourseSelectService>? logger;

    private Mock<IHttpClientProvider>? mockHttpClientProvider;
    //private Mock<ILogger<CourseSelectService>>? mockLogger = null!;
    private SelectScheduleProvider? selectScheduleProvider;
    private CourseSelectService? courseSelectService;
    private Mock<IAppSettingsService>? mockAppSettingsService;

    [TestInitialize]
    public void Setup()
    {
        mockAppSettingsService = new Mock<IAppSettingsService>();

        // 1. 创建一个假的 IKeyItem<T>
        var fakeKeyItem = new Mock<IKeyItem<ObservableCollection<SelectSchedule>>>();
        fakeKeyItem
            .Setup(k => k.Value)
            .Returns([]);

        // 2. Mock IAppSettingsService 返回这个 KeyItem
        mockAppSettingsService
            .Setup(a => a.SelectSchedules)
            .Returns(fakeKeyItem.Object);

        // 3. 用真实的 SelectScheduleProvider
        selectScheduleProvider = new SelectScheduleProvider(mockAppSettingsService.Object);

        // 3. 其他依赖
        mockHttpClientProvider = new Mock<IHttpClientProvider>();
        mockHttpClientProvider
            .Setup(x => x.SendAddCourseRequestAsync(It.IsAny<CourseItem>(), It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                await Task.Delay(1000);
                if (DateTime.Now >= selectScheduleProvider.SelectSchedules[0].ScheduleTime.StartTime && DateTime.Now <= selectScheduleProvider.SelectSchedules[0].ScheduleTime.EndTime)
                {
                    return new HttpResponseMessage { Content = new StringContent("1") };
                }
                else
                {
                    return new HttpResponseMessage { Content = new StringContent("现在不是选课时间") };
                }
            });

        mockHttpClientProvider
            .Setup(x => x.SendWithdrawCourseRequestAsync(It.IsAny<CourseItem>(), It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                await Task.Delay(1000);
                if (DateTime.Now >= selectScheduleProvider.SelectSchedules[0].ScheduleTime.StartTime && DateTime.Now <= selectScheduleProvider.SelectSchedules[0].ScheduleTime.EndTime)
                {
                    return new HttpResponseMessage { Content = new StringContent("1") };
                }
                else
                {
                    return new HttpResponseMessage { Content = new StringContent("现在不是选课时间") };
                }
            });

        logger = new TestOutputLogger<CourseSelectService>(testContext);

        // 4. 创建 Service
        courseSelectService = new CourseSelectService(mockHttpClientProvider.Object, logger, selectScheduleProvider);
    }

    private SelectSchedule SetUpFutureSelectSchedule()
    {
        var now = DateTime.Now;

        var startTime = now.AddSeconds(5);
        var endTime = startTime.AddHours(6);

        var time = new ScheduleTime(startTime, endTime);
        var schedule = new SelectSchedule(time, "5 秒后开始的时间表");

        selectScheduleProvider?.Add(schedule);

        return schedule;
    }

    private SelectSchedule SetUpOpenedSelectSchedule()
    {
        var now = DateTime.Now;

        var startTime = now.AddHours(-1);
        var endTime = startTime.AddHours(6);

        var time = new ScheduleTime(startTime, endTime);
        var schedule = new SelectSchedule(time, "已经开始一小时的时间表");

        selectScheduleProvider?.Add(schedule);

        return schedule;
    }

    [TestMethod]
    public async Task SelectCourseFutureSelectScheduleTest()
    {
        var schedule = SetUpFutureSelectSchedule();

        var course = new CourseItem("12345678", "何意味何意味何意味", "23333333");

        courseSelectService?.AddCourse(course, schedule, OperationType.Select);

        var task = courseSelectService?.SelectTasks[courseSelectService.SelectTasks.Count - 1];

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

        courseSelectService?.AddCourse(course, schedule, OperationType.Withdraw);

        var task = courseSelectService?.SelectTasks[courseSelectService.SelectTasks.Count - 1];

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

        courseSelectService?.AddCourse(course, schedule, operationType);

        var task = courseSelectService?.SelectTasks[courseSelectService.SelectTasks.Count - 1];

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

        var course = new CourseItem("12345678", "何意味何意味何意味", "23333333");

        // Act
        bool actual = courseSelectService!.AddCourse(course, schedule, operationType);

        // Assert
        Assert.AreEqual(expected, actual);
    }
}
