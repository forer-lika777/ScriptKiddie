using Microsoft.Extensions.Logging;

internal sealed class TestContextLoggerProvider : ILoggerProvider
{
    private readonly TestContext testContext;
    public TestContextLoggerProvider(TestContext testContext)
    {
        this.testContext = testContext;
    }

    public ILogger CreateLogger(string categoryName) => new TestContextLogger(testContext, categoryName);
    public void Dispose() { }

    private sealed class TestContextLogger : ILogger
    {
        private readonly TestContext ctx;
        private readonly string category;
        public TestContextLogger(TestContext ctx, string category) => (this.ctx, this.category) = (ctx, category);

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            ctx.WriteLine($"[{logLevel}] {category}: {formatter(state, exception)}");
            if (exception is not null)
                ctx.WriteLine(exception.ToString());
        }
    }
}