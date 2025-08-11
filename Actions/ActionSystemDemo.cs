using SmartCrowd.Core.Entities;
using SmartCrowd.Core.WorldAdapter;

namespace SmartCrowd.Core.Actions;

public static class ActionSystemDemo
{
    public static void RunDemo()
    {
        // Создаем адаптер мира
        var world = new InMemoryWorldAdapter();
        
        // Создаем агента (дровосека)
        var woodcutter = new AgentState("woodcutter_001");
        woodcutter.AddSkill("chop_wood");
        woodcutter.AddItem("axe", 1);
        woodcutter.AddItem("gold", 20);
        
        // Создаем дерево
        var tree = new Entity("tree_001");
        tree.Tags.Add("choppable");
        tree.Props["hp"] = 100;
        tree.Props["type"] = "oak";
        
        // Создаем торговца
        var trader = new Entity("trader_001");
        trader.Tags.Add("trader");
        trader.Tags.Add("closed");
        
        // Добавляем объекты в мир
        world.UpdateEntity(tree);
        world.UpdateEntity(trader);
        
        // Регистрируем действия для дровосека
        woodcutter.Actions.Register(ExampleActions.CreateChopTreeAction());
        woodcutter.Actions.Register(ExampleActions.CreateTradeAction());
        woodcutter.Actions.Register(ExampleActions.CreateOpenDoorAction());
        
        // Демонстрируем рубку дерева
        Console.WriteLine("=== Демонстрация рубки дерева ===");
        Console.WriteLine($"Дерево HP: {tree.Props["hp"]}");
        Console.WriteLine($"Дерево занято: {tree.IsBusy}");
        
        woodcutter.SetTarget(tree.Id);
        var chopAction = woodcutter.Actions.GetAction("ChopTree");
        
        if (chopAction != null && chopAction.CanExecute(woodcutter, world, tree))
        {
            Console.WriteLine("Выполняем действие 'ChopTree'...");
            var success = chopAction.Execute(woodcutter, world, tree);
            Console.WriteLine($"Действие выполнено: {success}");
            Console.WriteLine($"Дерево HP после: {tree.Props["hp"]}");
            Console.WriteLine($"Дерево занято после: {tree.IsBusy}");
            Console.WriteLine($"Дровосек получил дерево: {woodcutter.GetItemCount("wood")}");
        }
        
        // Демонстрируем торговлю
        Console.WriteLine("\n=== Демонстрация торговли ===");
        Console.WriteLine($"Дровосек имеет золота: {woodcutter.GetItemCount("gold")}");
        Console.WriteLine($"Дровосек имеет еды: {woodcutter.GetItemCount("food")}");
        
        woodcutter.SetTarget(trader.Id);
        var tradeAction = woodcutter.Actions.GetAction("Trade");
        
        if (tradeAction != null && tradeAction.CanExecute(woodcutter, world, trader))
        {
            Console.WriteLine("Выполняем действие 'Trade'...");
            var success = tradeAction.Execute(woodcutter, world, trader);
            Console.WriteLine($"Действие выполнено: {success}");
            Console.WriteLine($"Дровосек имеет золота после: {woodcutter.GetItemCount("gold")}");
            Console.WriteLine($"Дровосек имеет еды после: {woodcutter.GetItemCount("food")}");
        }
        
        // Демонстрируем открытие двери
        Console.WriteLine("\n=== Демонстрация открытия двери ===");
        Console.WriteLine($"Дверь открыта: {trader.Tags.Contains("open")}");
        Console.WriteLine($"Дверь закрыта: {trader.Tags.Contains("closed")}");
        
        var openDoorAction = woodcutter.Actions.GetAction("OpenDoor");
        
        if (openDoorAction != null && openDoorAction.CanExecute(woodcutter, world, trader))
        {
            Console.WriteLine("Выполняем действие 'OpenDoor'...");
            var success = openDoorAction.Execute(woodcutter, world, trader);
            Console.WriteLine($"Действие выполнено: {success}");
            Console.WriteLine($"Дверь открыта после: {trader.Tags.Contains("open")}");
            Console.WriteLine($"Дверь закрыта после: {trader.Tags.Contains("closed")}");
        }
    }
} 