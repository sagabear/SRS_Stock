using Microsoft.Extensions.Logging;

namespace SRS_Stock.Tests.Support;

internal sealed class ListLogger<T> : ILogger<T>
{
    public List<LogEntry> Entries { get; } = [];

    public IDisposable BeginScope<TState>(TState state)
        where TState : notnull => NullScope.Instance;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        Entries.Add(new LogEntry(logLevel, formatter(state, exception), exception));
    }

    public bool Has(LogLevel level, string text) =>
        Entries.Any(entry =>
            entry.Level == level &&
            entry.Message.Contains(text, StringComparison.Ordinal));

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();

        public void Dispose()
        {
        }
    }
}

internal sealed record LogEntry(LogLevel Level, string Message, Exception? Exception);
