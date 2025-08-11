using System.Text.Json.Serialization;

namespace SmartCrowd.Core.Scheduler;

public class ScheduleEntry
{
    public TimeSpan Time { get; set; }
    
    public string ActionName { get; set; } = string.Empty;
    
    public string? TargetId { get; set; }
    
    public Dictionary<string, object> Parameters { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    
    public bool IsInterruptible { get; set; } = true;
    
    public bool RetryIfBusy { get; set; } = false;
    
    public int MaxRetries { get; set; } = 3;
    
    public TimeSpan RetryInterval { get; set; } = TimeSpan.FromMinutes(5);
    
    public ScheduleEntry() { }
    
    public ScheduleEntry(TimeSpan time, string actionName, string? targetId = null)
    {
        Time = time;
        ActionName = actionName;
        TargetId = targetId;
    }
    
    /// <summary>
    /// Проверяет, можно ли выполнить действие в указанное время
    /// </summary>
    public bool CanExecuteAt(TimeSpan currentTime)
    {
        return currentTime >= Time;
    }
    
    /// <summary>
    /// Получает время следующего выполнения с учётом интервала повтора
    /// </summary>
    public TimeSpan GetNextExecutionTime(TimeSpan currentTime, int retryCount = 0)
    {
        if (retryCount == 0)
        {
            return Time;
        }
        
        if (retryCount > MaxRetries)
        {
            return TimeSpan.MaxValue; // Больше не повторяем
        }
        
        return Time.Add(RetryInterval.Multiply(retryCount));
    }
    
    /// <summary>
    /// Проверяет, можно ли повторить попытку
    /// </summary>
    public bool CanRetry(int currentRetryCount)
    {
        return RetryIfBusy && currentRetryCount < MaxRetries;
    }
} 