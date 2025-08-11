using SmartCrowd.Core.Entities;
using SmartCrowd.Core.WorldAdapter;

namespace SmartCrowd.Core.Scheduler;

public class ScenarioStep
{
    public string Name { get; set; } = string.Empty;
    
    public string ActionName { get; set; } = string.Empty;
    
    public string? TargetId { get; set; }
    
    public string? TargetTag { get; set; } // Альтернатива TargetId для динамического поиска
    
    public Dictionary<string, object> Parameters { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    
    public Func<AgentState, bool> Condition { get; set; } = _ => true;
    
    public bool IsInterruptible { get; set; } = true;
    
    public bool WaitForCompletion { get; set; } = true;
    
    public ScenarioStep() { }
    
    public ScenarioStep(string name, string actionName, string? targetId = null)
    {
        Name = name;
        ActionName = actionName;
        TargetId = targetId;
    }
    
    /// <summary>
    /// Проверяет, можно ли перейти к следующему шагу
    /// </summary>
    public bool CanProceed(AgentState agentState)
    {
        return Condition(agentState);
    }
    
    /// <summary>
    /// Получает ID цели, используя TargetId или поиск по тегу
    /// </summary>
    public string? GetTargetId(IWorldAdapter world, AgentState agentState)
    {
        if (!string.IsNullOrEmpty(TargetId))
        {
            return TargetId;
        }
        
        if (!string.IsNullOrEmpty(TargetTag))
        {
            // Ищем доступную цель с нужным тегом
            var availableTargets = world.GetAllEntities()
                .Where(e => e.HasTag(TargetTag) && !e.IsBusy)
                .ToList();
            
            if (availableTargets.Any())
            {
                return availableTargets.First().Id;
            }
        }
        
        return null;
    }
} 