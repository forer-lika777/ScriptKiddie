using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptKiddie.WinUI.Tests;

public class TestOutputLogger<T> : ILogger<T>
{
    private readonly TestContext testContext;

    public TestOutputLogger(TestContext testContext)
    {
        this.testContext = testContext;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var message = formatter(state, exception);
        testContext.WriteLine($"[{logLevel}] {message}");
        Debug.WriteLine($"[{logLevel}] {message}");
    }
}
