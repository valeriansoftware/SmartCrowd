using SmartCrowd.Core.Entities;
using SmartCrowd.Core.Actions;
using SmartCrowd.Core.WorldAdapter;

namespace SmartCrowd.Core.Planner;

public class GoapNode
{
    public AgentState AgentState { get; }
    public float G { get; set; } // Стоимость пути от начального состояния
    public float H { get; set; } // Эвристическая оценка до цели
    public float F => G + H; // Общая оценка узла
    public GoapNode? Parent { get; set; }
    public GameAction? Action { get; set; }
    public Entity? Target { get; set; }
    
    public GoapNode(AgentState agentState, float g = 0, float h = 0)
    {
        AgentState = agentState;
        G = g;
        H = h;
    }
    
    /// <summary>
    /// Создаёт новый узел, применяя действие к текущему состоянию
    /// </summary>
    public GoapNode? ApplyAction(GameAction action, Entity target, IWorldAdapter world)
    {
        // Проверяем, можно ли выполнить действие
        if (!action.CanExecute(AgentState, world, target))
        {
            return null;
        }
        
        // Создаём копию состояния агента
        var newState = CloneAgentState(AgentState);
        
        // Применяем действие
        var success = action.Execute(newState, world, target);
        if (!success)
        {
            return null;
        }
        
        // Создаём новый узел
        var newNode = new GoapNode(newState, G + action.Cost, H)
        {
            Parent = this,
            Action = action,
            Target = target
        };
        
        return newNode;
    }
    
    /// <summary>
    /// Восстанавливает путь от корневого узла к текущему
    /// </summary>
    public List<GoapNode> GetPath()
    {
        var path = new List<GoapNode>();
        var current = this;
        
        while (current != null)
        {
            path.Insert(0, current);
            current = current.Parent;
        }
        
        return path;
    }
    
    /// <summary>
    /// Получает последовательность действий для достижения цели
    /// </summary>
    public List<(GameAction Action, Entity? Target)> GetActionSequence()
    {
        var actions = new List<(GameAction Action, Entity? Target)>();
        var current = this;
        
        while (current?.Parent != null)
        {
            if (current.Action != null)
            {
                actions.Insert(0, (current.Action, current.Target));
            }
            current = current.Parent;
        }
        
        return actions;
    }
    
    /// <summary>
    /// Клонирует состояние агента для создания нового узла
    /// </summary>
    private static AgentState CloneAgentState(AgentState original)
    {
        var clone = new AgentState(original.Id)
        {
            CurrentTargetId = original.CurrentTargetId
        };
        
        // Копируем статы
        foreach (var stat in original.Stats)
        {
            clone.Stats[stat.Key] = stat.Value;
        }
        
        // Копируем инвентарь
        foreach (var item in original.Inventory)
        {
            clone.Inventory[item.Key] = item.Value;
        }
        
        // Копируем навыки
        foreach (var skill in original.Skills)
        {
            clone.Skills.Add(skill);
        }
        
        return clone;
    }
} 