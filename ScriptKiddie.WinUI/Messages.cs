using ScriptKiddie.WinUI.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ScriptKiddie.WinUI;

// Account management

public record AccountInfoChangedMessage(AccountInfo Value);

public record UpdateLoginStatusMessage(bool IsLoggedIn);

public record AutoLoginFailedNeedCaptchaMessage();

public record NeedCaptchaMessage();

// Select schedules management

public record SelectScheduleAddedMessage();

public record SelectScheduleRemoveMessage(IEnumerable<SelectSchedule> ChangedSelectSchedules, TaskCompletionSource TaskCompletionSource);

public record SelectScheduleRemoveConfirmMessage(List<CourseSelectTask> ChangedCourseSelectTasks, TaskCompletionSource TaskCompletionSource);

public record RequestChooseSelectScheduleMessage(TaskCompletionSource<SelectSchedule> TaskCompletionSource);

public record RequestConfirmWithdrawCourseMessage(ObservableCollection<CourseItem> SelectedCourses, TaskCompletionSource<CourseItem> TaskCompletionSource);

// Select tasks management

public record TaskAddFailedMessage(string Info);