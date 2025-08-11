using SmartCrowd.Core.Entities;
using SmartCrowd.Core.Actions;
using SmartCrowd.Core.Goals;
using SmartCrowd.Core.WorldAdapter;

namespace SmartCrowd.Core.Planner;

public class GoapPlan
{
    public List<(GameAction Action, Entity? Target)> Actions { get; }
    public Goal Goal { get; }
    public int CurrentStepIndex { get; private set; } = 0;
    
    public GoapPlan(List<(GameAction Action, Entity? Target)> actions, Goal goal)
    {
        Actions = actions;
        Goal = goal;
    }
    
    /// <summary>
    /// Проверяет, завершён ли план
    /// </summary>
    public bool IsCompleted => CurrentStepIndex >= Actions.Count;
    
    /// <summary>
    /// Получает текущее действие
    /// </summary>
    public (GameAction Action, Entity? Target)? GetCurrentAction()
    {
        if (IsCompleted)
        {
            return null;
        }
        
        return Actions[CurrentStepIndex];
    }
    
    /// <summary>
    /// Выполняет следующий шаг плана
    /// </summary>
    public bool ExecuteNextStep(AgentState agentState, IWorldAdapter world)
    {
        if (IsCompleted)
        {
            return false;
        }
        
        var currentAction = Actions[CurrentStepIndex];
        
        // Устанавливаем цель для агента
        if (currentAction.Target != null)
        {
            agentState.SetTarget(currentAction.Target.Id);
        }
        
        // Выполняем действие
        var success = currentAction.Action.Execute(agentState, world, currentAction.Target!);
        
        if (success)
        {
            CurrentStepIndex++;
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Получает оставшиеся действия для реплана
    /// </summary>
    public List<(GameAction Action, Entity? Target)> GetRemainingActions(AgentState agentState, IWorldAdapter world)
    {
        var remainingActions = new List<(GameAction Action, Entity? Target)>();
        
        for (int i = CurrentStepIndex; i < Actions.Count; i++)
        {
            var action = Actions[i];
            
            // Проверяем, можно ли выполнить действие
            if (action.Target != null && action.Action.CanExecute(agentState, world, action.Target))
            {
                remainingActions.Add(action);
            }
            else
            {
                // Если действие недоступно, прерываем цепочку
                break;
            }
        }
        
        return remainingActions;
    }
    
    /// <summary>
    /// Сбрасывает план к началу
    /// </summary>
    public void Reset()
    {
        CurrentStepIndex = 0;
    }
    
    /// <summary>
    /// Получает общую стоимость плана
    /// </summary>
    public float GetTotalCost()
    {
        return Actions.Sum(a => a.Action.Cost);
    }
    
    /// <summary>
    /// Получает количество оставшихся шагов
    /// </summary>
    public int GetRemainingSteps()
    {
        return Math.Max(0, Actions.Count - CurrentStepIndex);
    }
} 