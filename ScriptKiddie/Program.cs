using Script_Kiddie.Models;
using Script_Kiddie.Services;
using Script_Kiddie.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;

static string ReadPassword()
{
    var password = new StringBuilder();
    ConsoleKeyInfo key;

    do
    {
        key = Console.ReadKey(true);  // true = 不显示输入

        if (key.Key == ConsoleKey.Backspace && password.Length > 0)
        {
            password.Remove(password.Length - 1, 1);
            Console.Write("\b \b");  // 删除最后一个 *
        }
        else if (key.Key != ConsoleKey.Enter && key.Key != ConsoleKey.Backspace)
        {
            password.Append(key.KeyChar);
            Console.Write("*");
        }
    }
    while (key.Key != ConsoleKey.Enter);

    Console.WriteLine();
    return password.ToString();
}

var services = new ServiceCollection();

services.AddLogging(builder =>
{
    builder.AddSimpleConsole(options =>
    {
        options.SingleLine = true;
        options.TimestampFormat = "HH:mm:ss ";
        options.ColorBehavior = Microsoft.Extensions.Logging.Console.LoggerColorBehavior.Enabled;
    });
    builder.SetMinimumLevel(LogLevel.Trace);
});

services.AddSingleton<AccountManageService>();

var provider = services.BuildServiceProvider();
var accountService = provider.GetRequiredService<AccountManageService>();

bool preLoginSuccess = false;

var token = SecureFileStorage.Load("token.dat");

if (!string.IsNullOrWhiteSpace(token))
{
    LoginOption preLoginOption = new LoginOption
    {
        RememberMe = true,
        LoadCookie = true,
        ExportCookie = true,
    };

    preLoginOption.LoadCookie = true;
    preLoginOption.CookieContent = token;

    var result = await accountService.LoginAsync(preLoginOption);

    if (!result.Success)
    {
        Console.WriteLine("Pre login failed.");
    }
    else
    {
        preLoginSuccess = true;
    }
}

if (!preLoginSuccess)
{
    while (true)
    {
        Console.WriteLine("Please select a way to login your account.");
        Console.WriteLine("  1.  Username and password login\n  2.  Cookie login");
        Console.Write("Please enter an number: ");
        string input = Console.ReadLine();

        LoginOption option = new LoginOption()
        {
            ExportCookie = true
        };

        if (input == "1")
        {
            Console.Write("Please enter your username: ");
            string? username = Console.ReadLine();
            Console.Write("Please enter your password: ");
            string? password = ReadPassword();
            Console.WriteLine("Please wait for logining...");

            option.RememberMe = true;
            option.LoadCookie = false;

            option.UserName = username;
            option.Password = password;
        }
        else if (input == "2")
        {
            Console.Write("Please enter your cookie (JSESSIONID field): ");
            string sessionId = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(sessionId))
            {
                Console.WriteLine("Please check your input.");
                continue;
            }
            option.LoadCookie = true;
            option.CookieLoadMode = CookieLoadMode.Cookie;

            Cookie cookie = new Cookie("JSESSIONID", sessionId);
            option.Cookie = cookie;
        }
        else
        {
            Console.WriteLine("Not an available input.");
            continue;
        }

        var result = await accountService.LoginAsync(option);

        if (!result.Success)
        {
            await Task.Delay(10);
            Console.Write("\nLogin failed. Do you want to try again? (y/n) ");
            input = Console.ReadLine();
            if (input == "y" || input == "")
            {
                continue;
            }
            return;
        }

        string cookieString = result.CookieContent;

        if (!string.IsNullOrEmpty(cookieString))
        {
            SecureFileStorage.Save("token.dat", cookieString);
        }

        Console.WriteLine("\nThis is the first time you login this account. Cookie will be saved for auto logining the next time when you open this program.");
        Console.WriteLine("If you login with username and password, the cookie will be available for 7 days.");
        Console.WriteLine("If you login with a cookie session id, cookie available time depends on your login behavior in the browser");
        Console.WriteLine("This program will not save your username or password.");
        Console.WriteLine("If you just want to switch to another account, delete file \"token.dat\" in the location of this program.\n");

        Console.Write("I have read it. (Press ENTER key to continue)");
        Console.ReadLine();
        break;
    }
}

var selectableCourseResponse = await accountService.GetSelectableCoursesAsync();

if (selectableCourseResponse == null) return;

Console.WriteLine("Total courses: " + selectableCourseResponse.Total);

for (int i = 0; i < selectableCourseResponse.Rows.Count; i++)
{
    var course = selectableCourseResponse.Rows[i];
    Console.WriteLine($"  [{i + 1}] " + course.ToString());
}

var selectedCourses = await accountService.GetSelectedCoursesAsync();

if (selectedCourses == null) return;

if (selectedCourses.Count > 0)
{
    Console.WriteLine("Selected courses: " + selectedCourses.Count);

    for (int i = 0; i < selectedCourses.Count; i++)
    {
        var course = selectedCourses[i];
        Console.WriteLine($"  [{i + 1}] " + course.ToString());
    }
}
else
{
    Console.WriteLine("No Selected courses found.");
}

while (true)
{
    Console.Write("Select an index in the list to specify the course to select: ");

    string indexStr = Console.ReadLine();

    if (int.TryParse(indexStr, out int index))
    {
        index--;
        if (index < 0 || index >= selectableCourseResponse.Rows.Count)
        {
            Console.WriteLine($"Index out of range. Total courses count: {selectableCourseResponse.Rows.Count}");
            continue;
        }

        var course = selectableCourseResponse.Rows[index];

        Console.Write($"This will be the course to select: {course.ToString()}. Confirmed? (y/n) ");

        string input = Console.ReadLine();

        if (input != "Y" && input != "y")
        {
            Console.WriteLine("Cancelled.");
            continue;
        }

        DateTime openTime;

        while (true)
        {
            Console.Write("Please enter the open time of the course select session (Format: [yyyy-MM-dd HH:mm:ss]): ");
            input = Console.ReadLine();
            if (!DateTime.TryParse(input, out openTime))
            {
                Console.WriteLine($"Failed to read date time: {input}");
                continue;
            }
            break;
        }

        using var cts = new CancellationTokenSource();

        var planTask = accountService.AddCourseSelectPlan(course, openTime, cts.Token);

        Console.WriteLine("\u001b[38;2;255;255;100mCourse select schedule planned. Press \"ESC\" key to cancel this process.\u001b[0m");

        while (!planTask.IsCompleted)
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine("Select plan has been cancelled.");
                    cts.Cancel();
                    break;
                }
            }
            await Task.Delay(100);
        }
    }
    else
    {
        Console.WriteLine("The content you entered is not a number.");
        continue;
    }
}