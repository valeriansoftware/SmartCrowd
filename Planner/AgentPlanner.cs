using SmartCrowd.Core.Entities;
using SmartCrowd.Core.Actions;
using SmartCrowd.Core.Goals;
using SmartCrowd.Core.WorldAdapter;

namespace SmartCrowd.Core.Planner;

public class AgentPlanner
{
    private readonly GoapPlanner _planner;
    private readonly GoalManager _goalManager;
    private GoapPlan? _currentPlan;
    private Goal? _currentGoal;
    
    public AgentPlanner(IWorldAdapter world)
    {
        _planner = new GoapPlanner(world);
        _goalManager = new GoalManager();
    }
    
    /// <summary>
    /// Добавляет цель агенту
    /// </summary>
    public void AddGoal(Goal goal)
    {
        _goalManager.AddGoal(goal);
    }
    
    /// <summary>
    /// Добавляет несколько целей агенту
    /// </summary>
    public void AddGoals(IEnumerable<Goal> goals)
    {
        _goalManager.AddGoals(goals);
    }
    
    /// <summary>
    /// Строит план для достижения текущей цели
    /// </summary>
    public bool BuildPlan(AgentState agentState)
    {
        // Выбираем лучшую цель
        var goal = _goalManager.SelectBestGoal(agentState);
        if (goal == null)
        {
            return false;
        }
        
        _currentGoal = goal;
        
        // Строим план
        _currentPlan = _planner.BuildPlan(agentState, goal);
        
        return _currentPlan != null;
    }
    
    /// <summary>
    /// Выполняет следующий шаг текущего плана
    /// </summary>
    public bool ExecuteStep(AgentState agentState, IWorldAdapter world)
    {
        // Если план не существует или завершён, строим новый
        if (_currentPlan == null || _currentPlan.IsCompleted)
        {
            if (!BuildPlan(agentState))
            {
                return false;
            }
        }
        
        // Проверяем, не изменились ли условия
        if (_currentGoal != null && ShouldReplan(agentState, world))
        {
            _currentPlan = _planner.Replan(agentState, _currentGoal, _currentPlan);
            if (_currentPlan == null)
            {
                return false;
            }
        }
        
        // Выполняем следующий шаг
        if (_currentPlan == null)
        {
            return false;
        }
        
        var success = _currentPlan.ExecuteNextStep(agentState, world);
        
        // Если шаг не выполнился, запускаем реплан
        if (!success && _currentGoal != null)
        {
            _currentPlan = _planner.Replan(agentState, _currentGoal, _currentPlan);
        }
        
        return success;
    }
    
    /// <summary>
    /// Проверяет, нужно ли запускать реплан
    /// </summary>
    private bool ShouldReplan(AgentState agentState, IWorldAdapter world)
    {
        if (_currentGoal == null || _currentPlan == null)
        {
            return true;
        }
        
        // Проверяем, достигнута ли цель
        if (_currentGoal.IsAchieved(agentState))
        {
            return true;
        }
        
        // Проверяем, не изменились ли ключевые показатели
        var currentAction = _currentPlan.GetCurrentAction();
        if (currentAction.HasValue && currentAction.Value.Target != null)
        {
            var target = world.GetEntityById(currentAction.Value.Target.Id);
            
            // Цель исчезла или стала недоступной
            if (target == null || !currentAction.Value.Action.IsApplicableTo(target))
            {
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Получает текущий план
    /// </summary>
    public GoapPlan? GetCurrentPlan() => _currentPlan;
    
    /// <summary>
    /// Получает текущую цель
    /// </summary>
    public Goal? GetCurrentGoal() => _currentGoal;
    
    /// <summary>
    /// Проверяет, есть ли активные цели
    /// </summary>
    public bool HasActiveGoals(AgentState agentState) => _goalManager.HasActiveGoals(agentState);
    
    /// <summary>
    /// Получает все цели агента
    /// </summary>
    public IEnumerable<Goal> GetAllGoals() => _goalManager.GetAllGoals();
    
    /// <summary>
    /// Очищает все цели и планы
    /// </summary>
    public void Clear()
    {
        _goalManager.ClearGoals();
        _currentPlan = null;
        _currentGoal = null;
    }
} 