using SmartCrowd.Core.Entities;

namespace SmartCrowd.Core.Goals;

public static class ExampleGoals
{
    public static Goal CreateGatherWoodGoal()
    {
        return new Goal("GatherWood", 5)
        {
            IsAchieved = (state) => state.Inventory.ContainsKey("wood") && state.Inventory["wood"] >= 10,
            GetRelevance = (state) => 
            {
                var woodCount = state.GetItemCount("wood");
                if (woodCount >= 10) return 0.0f; // Цель достигнута
                
                // Чем меньше дерева, тем выше релевантность
                return Math.Max(0.1f, 1.0f - (woodCount / 10.0f));
            }
        };
    }
    
    public static Goal CreateGetFoodGoal()
    {
        return new Goal("GetFood", 8)
        {
            IsAchieved = (state) => state.Inventory.ContainsKey("food") && state.Inventory["food"] >= 5,
            GetRelevance = (state) =>
            {
                var foodCount = state.GetItemCount("food");
                if (foodCount >= 5) return 0.0f;
                
                // Критическая цель при голоде
                var hunger = state.GetStat("hunger");
                if (hunger > 80) return 1.0f;
                if (hunger > 60) return 0.8f;
                
                return Math.Max(0.2f, 1.0f - (foodCount / 5.0f));
            }
        };
    }
    
    public static Goal CreateRestGoal()
    {
        return new Goal("Rest", 3)
        {
            IsAchieved = (state) => state.GetStat("energy") >= 80,
            GetRelevance = (state) =>
            {
                var energy = state.GetStat("energy");
                if (energy >= 80) return 0.0f;
                
                // Критическая цель при низкой энергии
                if (energy < 20) return 0.9f;
                if (energy < 40) return 0.7f;
                
                return Math.Max(0.1f, (80.0f - energy) / 80.0f);
            }
        };
    }
    
    public static Goal CreateBuildHouseGoal()
    {
        return new Goal("BuildHouse", 10)
        {
            IsAchieved = (state) => state.Inventory.ContainsKey("house") && state.Inventory["house"] >= 1,
            GetRelevance = (state) =>
            {
                // Проверяем, есть ли необходимые ресурсы
                var woodCount = state.GetItemCount("wood");
                var stoneCount = state.GetItemCount("stone");
                
                if (woodCount < 20 || stoneCount < 15) return 0.1f; // Низкий приоритет без ресурсов
                
                return 0.6f; // Средний приоритет при наличии ресурсов
            }
        };
    }
    
    public static Goal CreateTradeGoal()
    {
        return new Goal("Trade", 4)
        {
            IsAchieved = (state) => state.GetStat("wealth") >= 100,
            GetRelevance = (state) =>
            {
                var wealth = state.GetStat("wealth");
                if (wealth >= 100) return 0.0f;
                
                var goldCount = state.GetItemCount("gold");
                if (goldCount > 0) return 0.5f; // Можно торговать
                
                return 0.2f; // Низкий приоритет без золота
            }
        };
    }
} 