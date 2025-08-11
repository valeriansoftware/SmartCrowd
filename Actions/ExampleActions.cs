using SmartCrowd.Core.Entities;
using SmartCrowd.Core.WorldAdapter;

namespace SmartCrowd.Core.Actions;

public static class ExampleActions
{
    public static GameAction CreateChopTreeAction()
    {
        return new GameAction("ChopTree", 5.0f)
        {
            Preconditions = (agentState, world) =>
            {
                if (string.IsNullOrEmpty(agentState.CurrentTargetId)) return false;
                var target = world.GetEntityById(agentState.CurrentTargetId);
                return target != null 
                    && target.Tags.Contains("choppable")
                    && !target.IsBusy
                    && agentState.Inventory.ContainsKey("axe");
            },
            Effects = (agentState, world) =>
            {
                if (string.IsNullOrEmpty(agentState.CurrentTargetId)) return;
                var target = world.GetEntityById(agentState.CurrentTargetId);
                if (target == null) return;
                
                var currentHp = target.Props.TryGetValue("hp", out var hp) ? Convert.ToInt32(hp) : 100;
                var newHp = Math.Max(0, currentHp - 10);
                target.Props["hp"] = newHp;
                
                if (newHp <= 0)
                {
                    // Дерево срублено — выдать ресурсы
                    agentState.AddItem("wood", 5);
                    // Можно также удалить дерево из мира или пометить как срубленное
                    target.Tags.Remove("choppable");
                    target.Tags.Add("chopped");
                }
            },
            IsApplicableTo = (target) =>
                target.Tags.Contains("choppable") && !target.IsBusy
        };
    }
    
    public static GameAction CreateTradeAction()
    {
        return new GameAction("Trade", 2.0f)
        {
            Preconditions = (agentState, world) =>
            {
                if (string.IsNullOrEmpty(agentState.CurrentTargetId)) return false;
                var target = world.GetEntityById(agentState.CurrentTargetId);
                return target != null 
                    && target.Tags.Contains("trader")
                    && !target.IsBusy
                    && agentState.Inventory.ContainsKey("gold");
            },
            Effects = (agentState, world) =>
            {
                if (string.IsNullOrEmpty(agentState.CurrentTargetId)) return;
                var target = world.GetEntityById(agentState.CurrentTargetId);
                if (target == null) return;
                
                // Простая логика торговли
                if (agentState.GetItemCount("gold") >= 10)
                {
                    agentState.RemoveItem("gold", 10);
                    agentState.AddItem("food", 3);
                }
            },
            IsApplicableTo = (target) =>
                target.Tags.Contains("trader") && !target.IsBusy
        };
    }
    
    public static GameAction CreateOpenDoorAction()
    {
        return new GameAction("OpenDoor", 1.0f)
        {
            Preconditions = (agentState, world) =>
            {
                if (string.IsNullOrEmpty(agentState.CurrentTargetId)) return false;
                var target = world.GetEntityById(agentState.CurrentTargetId);
                return target != null 
                    && target.Tags.Contains("door")
                    && !target.IsBusy;
            },
            Effects = (agentState, world) =>
            {
                if (string.IsNullOrEmpty(agentState.CurrentTargetId)) return;
                var target = world.GetEntityById(agentState.CurrentTargetId);
                if (target == null) return;
                
                // Изменить состояние двери
                target.Props["isOpen"] = true;
                target.Tags.Remove("closed");
                target.Tags.Add("open");
            },
            IsApplicableTo = (target) =>
                target.Tags.Contains("door") && !target.IsBusy
        };
    }
} 