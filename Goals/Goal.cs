using SmartCrowd.Core.Entities;

namespace SmartCrowd.Core.Goals;

public class Goal
{
    public string Name { get; set; } = string.Empty;
    
    public int Priority { get; set; } = 50; // 0-100
    
    public Func<AgentState, bool> IsAchieved { get; set; } = _ => false;
    
    public Func<AgentState, float> GetRelevance { get; set; } = _ => 0.5f;
    
    public Goal() { }
    
    public Goal(string name, int priority = 50)
    {
        Name = name;
        Priority = Math.Clamp(priority, 0, 100);
    }
    
    /// <summary>
    /// Вычисляет финальную релевантность цели с учётом приоритета и динамической оценки
    /// </summary>
    public float CalculateFinalRelevance(AgentState agentState)
    {
        var dynamicRelevance = GetRelevance(agentState);
        var priorityMultiplier = Priority / 100.0f;
        
        // Если цель уже достигнута, её релевантность минимальна
        if (IsAchieved(agentState))
        {
            return 0.0f;
        }
        
        // Комбинируем приоритет и динамическую релевантность
        return dynamicRelevance * priorityMultiplier;
    }
    
    /// <summary>
    /// Проверяет, является ли цель критической (должна перекрывать остальные)
    /// </summary>
    public bool IsCritical(AgentState agentState)
    {
        // Критические цели имеют высокую релевантность и приоритет
        var relevance = CalculateFinalRelevance(agentState);
        return relevance > 0.8f && Priority > 80;
    }
} 