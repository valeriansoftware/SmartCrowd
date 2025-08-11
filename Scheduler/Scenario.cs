using SmartCrowd.Core.Entities;
using SmartCrowd.Core.Actions;

namespace SmartCrowd.Core.Scheduler;

public class Scenario
{
    public string Name { get; set; } = string.Empty;
    
    public List<ScenarioStep> Steps { get; set; } = new();
    
    public Func<AgentState, bool> StartCondition { get; set; } = _ => true;
    
    public Action<AgentState>? OnStart { get; set; }
    
    public Action<AgentState>? OnComplete { get; set; }
    
    public Action<AgentState>? OnInterrupt { get; set; }
    
    public bool IsLooping { get; set; } = false;
    
    public Scenario() { }
    
    public Scenario(string name)
    {
        Name = name;
    }
    
    /// <summary>
    /// Добавляет шаг в сценарий
    /// </summary>
    public Scenario AddStep(ScenarioStep step)
    {
        Steps.Add(step);
        return this;
    }
    
    /// <summary>
    /// Добавляет шаг с действием
    /// </summary>
    public Scenario AddStep(string name, string actionName, string? targetId = null)
    {
        Steps.Add(new ScenarioStep(name, actionName, targetId));
        return this;
    }
    
    /// <summary>
    /// Проверяет, можно ли запустить сценарий
    /// </summary>
    public bool CanStart(AgentState agentState)
    {
        return StartCondition(agentState);
    }
    
    /// <summary>
    /// Получает количество шагов
    /// </summary>
    public int StepCount => Steps.Count;
    
    /// <summary>
    /// Получает шаг по индексу
    /// </summary>
    public ScenarioStep? GetStep(int index)
    {
        if (index >= 0 && index < Steps.Count)
        {
            return Steps[index];
        }
        return null;
    }
} 