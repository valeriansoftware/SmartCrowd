using SmartCrowd.Core.Entities;

namespace SmartCrowd.Core.Entities;

/// <summary>
/// Управляет автоматическим изменением статов агента со временем
/// </summary>
public class AgentStatsManager
{
    private TimeSpan _lastUpdateTime = TimeSpan.Zero;
    private readonly Dictionary<string, StatRule> _statRules = new(StringComparer.OrdinalIgnoreCase);
    
    /// <summary>
    /// Добавляет правило для изменения стата
    /// </summary>
    public void AddStatRule(string statName, StatRule rule)
    {
        _statRules[statName] = rule;
    }
    
    /// <summary>
    /// Устанавливает начальное время для менеджера статов
    /// </summary>
    public void SetInitialTime(TimeSpan initialTime)
    {
        _lastUpdateTime = initialTime;
    }
    
    /// <summary>
    /// Обновляет статы агента на основе прошедшего времени
    /// </summary>
    public void UpdateStats(AgentState agentState, TimeSpan currentTime)
    {
        var timeDiff = currentTime - _lastUpdateTime;
        
        if (timeDiff.TotalHours >= 1.0) // Обновляем каждый час
        {
            var hoursPassed = (int)timeDiff.TotalHours;
            
            foreach (var rule in _statRules)
            {
                var statName = rule.Key;
                var statRule = rule.Value;
                
                var currentValue = agentState.GetStat(statName);
                var change = statRule.ChangePerHour * hoursPassed;
                var newValue = Math.Max(statRule.MinValue, 
                                      Math.Min(statRule.MaxValue, currentValue + change));
                
                agentState.SetStat(statName, newValue);
                
                // Логируем изменение стата
                if (change != 0)
                {
                    var changeText = change > 0 ? $"+{change}" : $"{change}";
                    Console.WriteLine($"  [STATS] {statName}: {changeText} (время: {currentTime:hh\\:mm})");
                }
            }
            
            _lastUpdateTime = currentTime;
        }
    }
    
    /// <summary>
    /// Получает информацию о правилах статов
    /// </summary>
    public IEnumerable<KeyValuePair<string, StatRule>> GetStatRules()
    {
        return _statRules.ToList();
    }
}

/// <summary>
/// Правило изменения стата
/// </summary>
public class StatRule
{
    /// <summary>
    /// Изменение в час (положительное - увеличение, отрицательное - уменьшение)
    /// </summary>
    public int ChangePerHour { get; set; }
    
    /// <summary>
    /// Минимальное значение стата
    /// </summary>
    public int MinValue { get; set; }
    
    /// <summary>
    /// Максимальное значение стата
    /// </summary>
    public int MaxValue { get; set; }
    
    /// <summary>
    /// Описание правила
    /// </summary>
    public string Description { get; set; } = string.Empty;
} 