using SmartCrowd.Core.Entities;

namespace SmartCrowd.Core.Goals;

public class GoalManager
{
    private readonly List<Goal> _goals = new();
    
    public void AddGoal(Goal goal)
    {
        if (string.IsNullOrWhiteSpace(goal.Name))
        {
            throw new ArgumentException("Goal name cannot be null or empty", nameof(goal));
        }
        
        // Удаляем существующую цель с таким же именем
        _goals.RemoveAll(g => string.Equals(g.Name, goal.Name, StringComparison.OrdinalIgnoreCase));
        _goals.Add(goal);
    }
    
    public void AddGoals(IEnumerable<Goal> goals)
    {
        foreach (var goal in goals)
        {
            AddGoal(goal);
        }
    }
    
    public void RemoveGoal(string name)
    {
        _goals.RemoveAll(g => string.Equals(g.Name, name, StringComparison.OrdinalIgnoreCase));
    }
    
    public void ClearGoals()
    {
        _goals.Clear();
    }
    
    public Goal? GetGoal(string name)
    {
        return _goals.FirstOrDefault(g => string.Equals(g.Name, name, StringComparison.OrdinalIgnoreCase));
    }
    
    public IEnumerable<Goal> GetAllGoals() => _goals.ToList();
    
    /// <summary>
    /// Выбирает наиболее релевантную цель для агента
    /// </summary>
    public Goal? SelectBestGoal(AgentState agentState)
    {
        if (!_goals.Any())
        {
            return null;
        }
        
        // Сначала проверяем критические цели
        var criticalGoals = _goals.Where(g => g.IsCritical(agentState)).ToList();
        if (criticalGoals.Any())
        {
            return criticalGoals.OrderByDescending(g => g.CalculateFinalRelevance(agentState)).First();
        }
        
        // Затем выбираем по релевантности
        var sortedGoals = _goals
            .Where(g => !g.IsAchieved(agentState))
            .OrderByDescending(g => g.CalculateFinalRelevance(agentState))
            .ToList();
        
        return sortedGoals.FirstOrDefault();
    }
    
    /// <summary>
    /// Получает все достижимые цели, отсортированные по релевантности
    /// </summary>
    public IEnumerable<Goal> GetAchievableGoals(AgentState agentState)
    {
        return _goals
            .Where(g => !g.IsAchieved(agentState))
            .OrderByDescending(g => g.CalculateFinalRelevance(agentState));
    }
    
    /// <summary>
    /// Проверяет, есть ли активные цели
    /// </summary>
    public bool HasActiveGoals(AgentState agentState)
    {
        return _goals.Any(g => !g.IsAchieved(agentState));
    }
    
    public int Count => _goals.Count;
} 