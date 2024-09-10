using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using StargateAPI.Business.Data;
using Microsoft.OpenApi.Extensions;
using Microsoft.Extensions.Options;

public class LogEntry
{
    public int Id { get; set; }
    public string Message { get; set; }
    public string LogLevel { get; set; }
    public DateTime Timestamp { get; set; }
}

public class DbLogger : ILogger
{
    private readonly string _categoryName;
    private readonly Func<string, LogLevel, bool> _filter;
    private readonly IServiceProvider _serviceProvider;

    public DbLogger(
        string categoryName,
        Func<string, LogLevel, bool> filter,
        IServiceProvider serviceProvider)
    {
        _categoryName = categoryName;
        _filter = filter;
        _serviceProvider = serviceProvider;
    }

    public IDisposable BeginScope<TState>(TState state) => null;

    public bool IsEnabled(LogLevel logLevel) => _filter(_categoryName, logLevel);

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel))
        
        {
            return;
        }

        string message = formatter(state, exception);

        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<StargateContext>();

            var logEntry = new LogEntry
            {
                Message = $"{_categoryName}: {message}",
                LogLevel = logLevel.GetDisplayName(),
                Timestamp = DateTime.UtcNow
            };

            dbContext.LogEntries.Add(logEntry);
            dbContext.SaveChanges();
        }
    }
}

public class DbLoggerProvider : ILoggerProvider
{
    private readonly IOptionsMonitor<LoggerFilterOptions> _filterOptions;
    private readonly IServiceProvider _serviceProvider;

    public DbLoggerProvider(
        IOptionsMonitor<LoggerFilterOptions> filterOptions,
        IServiceProvider serviceProvider)
    {
        _filterOptions = filterOptions;
        _serviceProvider = serviceProvider;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new DbLogger(categoryName,
            (category, logLevel) => ShouldLog(category, logLevel),
            _serviceProvider);
    }

    private bool ShouldLog(string categoryName, LogLevel logLevel)
    {
        // Check if the category name starts with "StargateAPI"
        if (categoryName.StartsWith("StargateAPI"))
        {
            Console.WriteLine($"Applying custom log level for StargateAPI: {logLevel}");
            return logLevel >= LogLevel.Information;  // Apply Information level for StargateAPI
        }

        // For other namespaces, apply the default or configured rules
        var rules = _filterOptions.CurrentValue.Rules;

        // Find the most specific rule that applies to the current categoryName
        var rule = rules
            .Where(r => string.IsNullOrEmpty(r.CategoryName) || categoryName.StartsWith(r.CategoryName))
            .OrderByDescending(r => r.CategoryName?.Length ?? 0)
            .FirstOrDefault();

        if (rule != null)
        {
            return logLevel >= rule.LogLevel;
        }

        // Fallback to the global minimum level
        return logLevel >= _filterOptions.CurrentValue.MinLevel;
    }


    public void Dispose() { }
}

public static class DbLoggerExtensions
{
    public static ILoggingBuilder AddDbLogger(this ILoggingBuilder builder)
    {
        builder.Services.AddSingleton<ILoggerProvider, DbLoggerProvider>(serviceProvider =>
        {
            var filterOptions = serviceProvider.GetRequiredService<IOptionsMonitor<LoggerFilterOptions>>();
            return new DbLoggerProvider(filterOptions, serviceProvider);
        });

        return builder;
    }
}