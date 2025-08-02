using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace YouTubeMusicAPI.Sample.WPF.Logging;

internal class CollectionLogger : ILogger
{
    private readonly ObservableCollection<string> logMessages;
    private readonly LogLevel minLevel;

    public CollectionLogger(
        ObservableCollection<string> logMessages,
        LogLevel minLevel = LogLevel.Information)
    {
        this.logMessages = logMessages;
        this.minLevel = minLevel;
    }


    public IDisposable? BeginScope<TState>(
        TState state) where TState : notnull =>
        null;

    public bool IsEnabled(
        LogLevel logLevel) =>
        logLevel >= minLevel;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        string message = formatter(state, exception);
        if (exception != null)
            message += $" | Exception: {exception}";

        logMessages.Insert(0, $"[{DateTime.Now.TimeOfDay:hh\\:mm\\:ss}-{logLevel}]-{message}");
    }
}