using SmartCrowd.Core.Entities;
using SmartCrowd.Core.Actions;
using SmartCrowd.Core.Goals;
using SmartCrowd.Core.WorldAdapter;

namespace SmartCrowd.Core.Planner;

public static class GoapPlannerDemo
{
    public static void RunDemo()
    {
        Console.WriteLine("=== Демонстрация GOAP-планировщика ===\n");
        
        // Создаем мир
        var world = new InMemoryWorldAdapter();
        
        // Создаем агента
        var agent = new AgentState("survivor_001");
        agent.SetStat("hunger", 70);
        agent.SetStat("energy", 30);
        agent.SetStat("wealth", 0);
        agent.AddItem("axe", 1);
        agent.AddItem("gold", 5);
        
        // Создаем планировщик
        var planner = new AgentPlanner(world);
        agent.Planner = planner;
        
        // Регистрируем действия
        agent.Actions.Register(ExampleActions.CreateChopTreeAction());
        agent.Actions.Register(ExampleActions.CreateTradeAction());
        agent.Actions.Register(ExampleActions.CreateOpenDoorAction());
        
        // Добавляем цели
        planner.AddGoals(new[]
        {
            ExampleGoals.CreateGatherWoodGoal(),
            ExampleGoals.CreateGetFoodGoal(),
            ExampleGoals.CreateRestGoal(),
            ExampleGoals.CreateBuildHouseGoal(),
            ExampleGoals.CreateTradeGoal()
        });
        
        // Создаем объекты в мире
        var tree1 = new Entity("tree_001");
        tree1.Tags.Add("choppable");
        tree1.Props["hp"] = 100;
        
        var tree2 = new Entity("tree_002");
        tree2.Tags.Add("choppable");
        tree2.Props["hp"] = 100;
        
        var trader = new Entity("trader_001");
        trader.Tags.Add("trader");
        
        world.UpdateEntity(tree1);
        world.UpdateEntity(tree2);
        world.UpdateEntity(trader);
        
        // Демонстрируем выбор целей
        Console.WriteLine("=== Анализ целей ===");
        var goals = planner.GetAllGoals().ToList();
        foreach (var goal in goals)
        {
            var relevance = goal.CalculateFinalRelevance(agent);
            var isCritical = goal.IsCritical(agent);
            Console.WriteLine($"Цель: {goal.Name}");
            Console.WriteLine($"  Приоритет: {goal.Priority}");
            Console.WriteLine($"  Релевантность: {relevance:F2}");
            Console.WriteLine($"  Критическая: {isCritical}");
            Console.WriteLine($"  Достигнута: {goal.IsAchieved(agent)}");
            Console.WriteLine();
        }
        
        // Выбираем лучшую цель
        var bestGoal = planner.GetCurrentGoal();
        if (bestGoal == null)
        {
            bestGoal = planner.GetAllGoals().OrderByDescending(g => g.CalculateFinalRelevance(agent)).First();
        }
        
        Console.WriteLine($"=== Выбрана цель: {bestGoal.Name} ===\n");
        
        // Строим план
        Console.WriteLine("=== Построение плана ===");
        var planBuilt = planner.BuildPlan(agent);
        if (planBuilt)
        {
            var plan = planner.GetCurrentPlan();
            Console.WriteLine($"План построен успешно!");
            Console.WriteLine($"Количество действий: {plan?.Actions.Count}");
            Console.WriteLine($"Общая стоимость: {plan?.GetTotalCost():F1}");
            Console.WriteLine();
            
            // Выполняем план пошагово
            Console.WriteLine("=== Выполнение плана ===");
            var step = 1;
            while (plan != null && !plan.IsCompleted)
            {
                var currentAction = plan.GetCurrentAction();
                if (currentAction.HasValue)
                {
                    Console.WriteLine($"Шаг {step}: {currentAction.Value.Action.Name}");
                    if (currentAction.Value.Target != null)
                    {
                        Console.WriteLine($"  Цель: {currentAction.Value.Target.Id}");
                    }
                    
                    var success = planner.ExecuteStep(agent, world);
                    Console.WriteLine($"  Результат: {(success ? "Успех" : "Неудача")}");
                    
                    if (success)
                    {
                        // Показываем изменения в состоянии
                        Console.WriteLine($"  Дерево HP: {tree1.Props["hp"]}");
                        Console.WriteLine($"  Дерево в инвентаре: {agent.GetItemCount("wood")}");
                        Console.WriteLine($"  Голод: {agent.GetStat("hunger")}");
                        Console.WriteLine($"  Энергия: {agent.GetStat("energy")}");
                    }
                    Console.WriteLine();
                    
                    step++;
                }
                else
                {
                    break;
                }
                
                // Проверяем, не изменились ли условия
                if (plan != null && plan.IsCompleted)
                {
                    Console.WriteLine("План завершён!");
                    break;
                }
            }
        }
        else
        {
            Console.WriteLine("Не удалось построить план!");
        }
        
        // Демонстрируем реплан
        Console.WriteLine("=== Демонстрация реплана ===");
        Console.WriteLine("Имитируем изменение условий: дерево становится недоступным");
        
        // Делаем дерево недоступным
        tree1.Tags.Remove("choppable");
        tree1.Tags.Add("chopped");
        
        // Пытаемся продолжить выполнение
        var replanSuccess = planner.ExecuteStep(agent, world);
        Console.WriteLine($"Реплан: {(replanSuccess ? "Успех" : "Неудача")}");
        
        if (replanSuccess)
        {
            var newPlan = planner.GetCurrentPlan();
            Console.WriteLine($"Новый план: {newPlan?.Actions.Count} действий");
        }
    }
} 