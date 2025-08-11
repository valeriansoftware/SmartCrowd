using SmartCrowd.Core.Entities;
using SmartCrowd.Core.WorldAdapter;

namespace SmartCrowd.Core.Actions;

public static class ExampleActions
{
    public static GameAction CreateChopTreeAction()
    {
        return new GameAction("ChopWood", 5.0f) // Изменил имя на ChopWood
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
                
                // Добавляем дерево в инвентарь при каждой рубке
                agentState.AddItem("wood", 2);
                
                if (newHp <= 0)
                {
                    // Дерево срублено — выдать дополнительные ресурсы
                    agentState.AddItem("wood", 3);
                    // Можно также удалить дерево из мира или пометить как срубленное
                    target.Tags.Remove("choppable");
                    target.Tags.Add("chopped");
                }
                
                // Уменьшаем энергию при рубке дерева
                agentState.ModifyStat("energy", -5);
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
                    // Уменьшаем голод при покупке еды
                    agentState.ModifyStat("hunger", -20);
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
    
    // Добавляем недостающие действия для еды
    public static GameAction CreateEatBreakfastAction()
    {
        return new GameAction("EatBreakfast", 1.0f)
        {
            Preconditions = (agentState, world) =>
            {
                if (string.IsNullOrEmpty(agentState.CurrentTargetId)) return false;
                var target = world.GetEntityById(agentState.CurrentTargetId);
                return target != null 
                    && target.Tags.Contains("eating")
                    && !target.IsBusy;
            },
            Effects = (agentState, world) =>
            {
                // Завтрак уменьшает голод и немного восстанавливает энергию
                agentState.ModifyStat("hunger", -20);
                agentState.ModifyStat("energy", 5);
            },
            IsApplicableTo = (target) =>
                target.Tags.Contains("eating") && !target.IsBusy
        };
    }
    
    public static GameAction CreateEatLunchAction()
    {
        return new GameAction("EatLunch", 1.0f)
        {
            Preconditions = (agentState, world) =>
            {
                if (string.IsNullOrEmpty(agentState.CurrentTargetId)) return false;
                var target = world.GetEntityById(agentState.CurrentTargetId);
                return target != null 
                    && target.Tags.Contains("eating")
                    && !target.IsBusy;
            },
            Effects = (agentState, world) =>
            {
                // Обед значительно уменьшает голод и восстанавливает энергию
                agentState.ModifyStat("hunger", -40);
                agentState.ModifyStat("energy", 10);
            },
            IsApplicableTo = (target) =>
                target.Tags.Contains("eating") && !target.IsBusy
        };
    }
    
    public static GameAction CreateEatDinnerAction()
    {
        return new GameAction("EatDinner", 1.0f)
        {
            Preconditions = (agentState, world) =>
            {
                if (string.IsNullOrEmpty(agentState.CurrentTargetId)) return false;
                var target = world.GetEntityById(agentState.CurrentTargetId);
                return target != null 
                    && target.Tags.Contains("eating")
                    && !target.IsBusy;
            },
            Effects = (agentState, world) =>
            {
                // Ужин уменьшает голод и подготавливает ко сну
                agentState.ModifyStat("hunger", -30);
                agentState.ModifyStat("energy", 5);
            },
            IsApplicableTo = (target) =>
                target.Tags.Contains("eating") && !target.IsBusy
        };
    }
    
    public static GameAction CreateRestAction()
    {
        return new GameAction("Rest", 3.0f)
        {
            Preconditions = (agentState, world) =>
            {
                if (string.IsNullOrEmpty(agentState.CurrentTargetId)) return false;
                var target = world.GetEntityById(agentState.CurrentTargetId);
                return target != null 
                    && target.Tags.Contains("resting")
                    && !target.IsBusy;
            },
            Effects = (agentState, world) =>
            {
                // Отдых значительно восстанавливает энергию
                agentState.ModifyStat("energy", 25);
                // Небольшое увеличение голода во время отдыха
                agentState.ModifyStat("hunger", 5);
            },
            IsApplicableTo = (target) =>
                target.Tags.Contains("resting") && !target.IsBusy
        };
    }
} 