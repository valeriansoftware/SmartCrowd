using SmartCrowd.Core.Entities;
using SmartCrowd.Core.WorldAdapter;

namespace SmartCrowd.Core.Actions;

public class GameAction
{
    public string Name { get; set; } = string.Empty;
    
    public float Cost { get; set; } = 1.0f;
    
    public Func<AgentState, IWorldAdapter, bool> Preconditions { get; set; } = (_, _) => true;
    
    public System.Action<AgentState, IWorldAdapter> Effects { get; set; } = (_, _) => { };
    
    public Func<Entity, bool> IsApplicableTo { get; set; } = _ => true;
    
    public GameAction() { }
    
    public GameAction(string name, float cost = 1.0f)
    {
        Name = name;
        Cost = cost;
    }
    
    public bool CanExecute(AgentState agentState, IWorldAdapter world, Entity? target = null)
    {
        // Проверяем предусловия
        if (!Preconditions(agentState, world))
        {
            return false;
        }
        
        // Если есть цель, проверяем применимость
        if (target != null && !IsApplicableTo(target))
        {
            return false;
        }
        
        return true;
    }
    
    public bool Execute(AgentState agentState, IWorldAdapter world, Entity target)
    {
        // Проверяем, что действие можно выполнить
        if (!CanExecute(agentState, world, target))
        {
            return false;
        }
        
        // Пытаемся зарезервировать цель
        if (!world.TryReserveEntity(target.Id, agentState.Id))
        {
            return false;
        }
        
        try
        {
            // Выполняем эффекты
            Effects(agentState, world);
            return true;
        }
        finally
        {
            // Всегда освобождаем цель
            world.ReleaseEntity(target.Id, agentState.Id);
        }
    }
} 