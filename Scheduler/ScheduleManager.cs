using SmartCrowd.Core.Entities;
using SmartCrowd.Core.Actions;
using SmartCrowd.Core.WorldAdapter;

namespace SmartCrowd.Core.Scheduler;

public class ScheduleManager
{
    private readonly List<ScheduleEntry> _schedule = new();
    private readonly Dictionary<string, int> _retryCounts = new(StringComparer.Ordinal);
    private readonly Dictionary<string, TimeSpan> _lastRetryTimes = new(StringComparer.Ordinal);
    
    public bool IsActive { get; private set; } = true;
    public TimeSpan CurrentTime { get; private set; } = TimeSpan.Zero;
    
    public event Action<ScheduleEntry>? OnActionScheduled;
    public event Action<ScheduleEntry>? OnActionCompleted;
    public event Action<ScheduleEntry>? OnActionSkipped;
    
    /// <summary>
    /// Устанавливает расписание на день
    /// </summary>
    public void SetSchedule(IEnumerable<ScheduleEntry> entries)
    {
        _schedule.Clear();
        _retryCounts.Clear();
        _lastRetryTimes.Clear();
        
        foreach (var entry in entries.OrderBy(e => e.Time))
        {
            _schedule.Add(entry);
        }
    }
    
    /// <summary>
    /// Добавляет действие в расписание
    /// </summary>
    public void AddEntry(ScheduleEntry entry)
    {
        _schedule.Add(entry);
        _schedule.Sort((a, b) => a.Time.CompareTo(b.Time));
    }
    
    /// <summary>
    /// Удаляет действие из расписания
    /// </summary>
    public void RemoveEntry(string actionName)
    {
        _schedule.RemoveAll(e => string.Equals(e.ActionName, actionName, StringComparison.OrdinalIgnoreCase));
    }
    
    /// <summary>
    /// Очищает расписание
    /// </summary>
    public void Clear()
    {
        _schedule.Clear();
        _retryCounts.Clear();
        _lastRetryTimes.Clear();
    }
    
    /// <summary>
    /// Обновляет время и возвращает действия для выполнения
    /// </summary>
    public List<ScheduleEntry> UpdateTime(TimeSpan newTime)
    {
        CurrentTime = newTime;
        var actionsToExecute = new List<ScheduleEntry>();
        
        foreach (var entry in _schedule)
        {
            if (ShouldExecuteEntry(entry))
            {
                actionsToExecute.Add(entry);
            }
        }
        
        return actionsToExecute;
    }
    
    /// <summary>
    /// Проверяет, нужно ли выполнить действие
    /// </summary>
    private bool ShouldExecuteEntry(ScheduleEntry entry)
    {
        if (!entry.CanExecuteAt(CurrentTime))
        {
            return false;
        }
        
        var retryCount = _retryCounts.GetValueOrDefault(entry.ActionName, 0);
        var lastRetryTime = _lastRetryTimes.GetValueOrDefault(entry.ActionName, TimeSpan.MinValue);
        
        // Если это повторная попытка, проверяем интервал
        if (retryCount > 0)
        {
            var nextRetryTime = entry.GetNextExecutionTime(entry.Time, retryCount);
            if (CurrentTime < nextRetryTime)
            {
                return false;
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// Отмечает действие как выполненное
    /// </summary>
    public void MarkActionCompleted(string actionName)
    {
        _retryCounts.Remove(actionName);
        _lastRetryTimes.Remove(actionName);
        
        var entry = _schedule.FirstOrDefault(e => string.Equals(e.ActionName, actionName, StringComparison.OrdinalIgnoreCase));
        if (entry != null)
        {
            OnActionCompleted?.Invoke(entry);
        }
    }
    
    /// <summary>
    /// Отмечает действие как пропущенное (цель занята)
    /// </summary>
    public void MarkActionSkipped(string actionName, bool wasBusy = false)
    {
        var entry = _schedule.FirstOrDefault(e => string.Equals(e.ActionName, actionName, StringComparison.OrdinalIgnoreCase));
        if (entry == null) return;
        
        if (wasBusy && entry.RetryIfBusy)
        {
            // Увеличиваем счётчик повторов
            var retryCount = _retryCounts.GetValueOrDefault(actionName, 0) + 1;
            _retryCounts[actionName] = retryCount;
            _lastRetryTimes[actionName] = CurrentTime;
            
            if (entry.CanRetry(retryCount))
            {
                // Действие будет повторено позже
                return;
            }
        }
        
        OnActionSkipped?.Invoke(entry);
    }
    
    /// <summary>
    /// Приостанавливает расписание
    /// </summary>
    public void Pause()
    {
        IsActive = false;
    }
    
    /// <summary>
    /// Возобновляет расписание
    /// </summary>
    public void Resume()
    {
        IsActive = true;
    }
    
    /// <summary>
    /// Получает все действия в расписании
    /// </summary>
    public IEnumerable<ScheduleEntry> GetAllEntries() => _schedule.ToList();
    
    /// <summary>
    /// Получает количество действий в расписании
    /// </summary>
    public int Count => _schedule.Count;
    
    /// <summary>
    /// Проверяет, есть ли действия в расписании
    /// </summary>
    public bool HasEntries => _schedule.Any();
} 