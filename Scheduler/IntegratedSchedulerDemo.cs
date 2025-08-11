using SmartCrowd.Core.Entities;
using SmartCrowd.Core.Actions;
using SmartCrowd.Core.Goals;
using SmartCrowd.Core.WorldAdapter;

namespace SmartCrowd.Core.Scheduler;

public static class IntegratedSchedulerDemo
{
    public static void RunDemo()
    {
        Console.WriteLine("=== Демонстрация интегрированного планировщика ===\n");
        
        // Создаем мир
        var world = new InMemoryWorldAdapter();
        
        // Создаем объекты в мире
        var table = new Entity("table_01");
        table.Tags.Add("table");
        table.Tags.Add("eating");
        
        var tree1 = new Entity("tree_01");
        tree1.Tags.Add("choppable");
        tree1.Props["hp"] = 100;
        
        var tree2 = new Entity("tree_02");
        tree2.Tags.Add("choppable");
        tree2.Props["hp"] = 100;
        
        var tree3 = new Entity("tree_03");
        tree3.Tags.Add("choppable");
        tree3.Props["hp"] = 100;
        
        var trader = new Entity("trader_01");
        trader.Tags.Add("trader");
        
        var bed = new Entity("bed_01");
        bed.Tags.Add("bed");
        bed.Tags.Add("resting");
        
        // Добавляем объекты в мир
        world.UpdateEntity(table);
        world.UpdateEntity(tree1);
        world.UpdateEntity(tree2);
        world.UpdateEntity(tree3);
        world.UpdateEntity(trader);
        world.UpdateEntity(bed);
        
        // Создаем агента
        var agent = new AgentState("farmer_001");
        agent.SetStat("hunger", 50);
        agent.SetStat("energy", 80);
        agent.SetStat("level", 1);
        agent.SetStat("questActive", 0);
        agent.SetStat("inCombat", 0);
        agent.SetStat("wantsToTrade", 0);
        agent.SetStat("socialNeed", 30);
        agent.SetStat("craftingSkill", 5);
        
        agent.AddItem("axe", 1);
        agent.AddItem("gold", 20);
        
        // Регистрируем действия
        agent.Actions.Register(ExampleActions.CreateChopTreeAction());
        agent.Actions.Register(ExampleActions.CreateTradeAction());
        agent.Actions.Register(ExampleActions.CreateOpenDoorAction());
        agent.Actions.Register(ExampleActions.CreateEatBreakfastAction());
        agent.Actions.Register(ExampleActions.CreateEatLunchAction());
        agent.Actions.Register(ExampleActions.CreateEatDinnerAction());
        agent.Actions.Register(ExampleActions.CreateRestAction());
        
        // Создаем интегрированный планировщик
        var scheduler = new IntegratedScheduler(world);
        agent.Scheduler = scheduler;
        
        // Добавляем цели для GOAP
        scheduler.AddGoal(ExampleGoals.CreateGatherWoodGoal());
        scheduler.AddGoal(ExampleGoals.CreateGetFoodGoal());
        scheduler.AddGoal(ExampleGoals.CreateRestGoal());
        
        // Устанавливаем расписание
        var schedule = ExampleSchedules.CreateFarmerSchedule();
        scheduler.SetSchedule(schedule);
        
        // Регистрируем сценарии
        scheduler.RegisterScenario(ExampleScenarios.CreateIntroScene());
        scheduler.RegisterScenario(ExampleScenarios.CreateQuestSequence());
        scheduler.RegisterScenario(ExampleScenarios.CreateCombatSequence());
        scheduler.RegisterScenario(ExampleScenarios.CreateTradeSequence());
        scheduler.RegisterScenario(ExampleScenarios.CreateSocialSequence());
        scheduler.RegisterScenario(ExampleScenarios.CreateCraftingSequence());
        
        // Подписываемся на события
        scheduler.OnModeChanged += (mode) => Console.WriteLine($"Режим изменён: {mode}");
        scheduler.OnScheduleActionExecuted += (entry) => Console.WriteLine($"Расписание: выполнено {entry.ActionName}");
        
        // Демонстрируем работу по расписанию
        Console.WriteLine("=== Демонстрация работы по расписанию ===");
        
        // Отладочная информация
        Console.WriteLine($"Доступные действия агента: {string.Join(", ", agent.Actions.GetAllActions().Select(a => a.Name))}");
        Console.WriteLine($"Объекты в мире: {string.Join(", ", world.GetAllEntities().Select(e => $"{e.Id}({string.Join(",", e.Tags)})"))}");
        Console.WriteLine($"Начальные статы: Голод={agent.GetStat("hunger")}, Энергия={agent.GetStat("energy")}");
        Console.WriteLine();
        
        var currentTime = new TimeSpan(6, 0, 0); // 6:00 утра
        
        for (int hour = 6; hour <= 22; hour++)
        {
            currentTime = new TimeSpan(hour, 0, 0);
            Console.WriteLine($"\n--- {currentTime:hh\\:mm} ---");
            
            var status = scheduler.GetStatus();
            Console.WriteLine($"Режим: {status.Mode}");
            
            // Обновляем планировщик
            var actionPerformed = scheduler.Update(agent, currentTime);
            
            if (actionPerformed)
            {
                Console.WriteLine("Действие выполнено");
            }
            else
            {
                Console.WriteLine("Действий не было");
            }
            
            // Показываем состояние агента
            Console.WriteLine($"Голод: {agent.GetStat("hunger")}, Энергия: {agent.GetStat("energy")}");
            Console.WriteLine($"Дерево в инвентаре: {agent.GetItemCount("wood")}");
            
            // Имитируем изменение условий (например, критический голод)
            if (hour == 10 && agent.GetStat("hunger") > 60)
            {
                Console.WriteLine("!!! Критический голод - переключение в GOAP-режим !!!");
                agent.SetStat("hunger", 85); // Делаем голод критическим
            }
            
            // Имитируем запуск сценария
            if (hour == 14 && agent.GetStat("questActive") == 0)
            {
                Console.WriteLine("!!! Запуск сценария квеста !!!");
                agent.SetStat("questActive", 1);
                scheduler.StartScenario("QuestSequence", agent);
            }
            
            // Имитируем прерывание сценария
            if (hour == 15 && scheduler.GetCurrentMode() == "Scenario")
            {
                Console.WriteLine("!!! Прерывание сценария - угроза !!!");
                agent.SetStat("inCombat", 1);
                // Сценарий автоматически прервется из-за критической цели
            }
        }
        
        // Демонстрируем статистику
        Console.WriteLine("\n=== Итоговая статистика ===");
        var finalStatus = scheduler.GetStatus();
        Console.WriteLine($"Финальный режим: {finalStatus.Mode}");
        Console.WriteLine($"Выполнено действий по расписанию: {finalStatus.ScheduleEntries}");
        Console.WriteLine($"Активен сценарий: {finalStatus.ScenarioActive}");
        Console.WriteLine($"Активен GOAP: {finalStatus.GoapActive}");
        Console.WriteLine($"Количество целей: {finalStatus.GoapGoals}");
    }
} 