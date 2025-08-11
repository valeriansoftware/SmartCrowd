using SmartCrowd.Core.Entities;

namespace SmartCrowd.Core.Scheduler;

public static class ExampleScenarios
{
    public static Scenario CreateIntroScene()
    {
        return new Scenario("IntroScene")
        {
            StartCondition = (agent) => agent.GetStat("level") == 1,
            OnStart = (agent) => Console.WriteLine($"{agent.Id} начинает вводную сцену"),
            OnComplete = (agent) => Console.WriteLine($"{agent.Id} завершил вводную сцену"),
            OnInterrupt = (agent) => Console.WriteLine($"{agent.Id} прервал вводную сцену")
        }
        .AddStep("ApproachGuide", "MoveTo", "guide_01")
        .AddStep("TalkToGuide", "Talk", "guide_01")
        .AddStep("ReceiveQuest", "ReceiveQuest", "guide_01")
        .AddStep("LeaveGuide", "MoveTo", "village_center");
    }
    
    public static Scenario CreateQuestSequence()
    {
        return new Scenario("QuestSequence")
        {
            StartCondition = (agent) => agent.GetStat("questActive") == 1,
            OnStart = (agent) => Console.WriteLine($"{agent.Id} начинает выполнение квеста"),
            OnComplete = (agent) => Console.WriteLine($"{agent.Id} завершил квест"),
            OnInterrupt = (agent) => Console.WriteLine($"{agent.Id} прервал выполнение квеста")
        }
        .AddStep("FindTarget", "MoveTo", "quest_target")
        .AddStep("InteractWithTarget", "Interact", "quest_target")
        .AddStep("ReturnToQuestGiver", "MoveTo", "quest_giver")
        .AddStep("CompleteQuest", "CompleteQuest", "quest_giver");
    }
    
    public static Scenario CreateCombatSequence()
    {
        return new Scenario("CombatSequence")
        {
            StartCondition = (agent) => agent.GetStat("inCombat") == 1,
            OnStart = (agent) => Console.WriteLine($"{agent.Id} вступает в бой"),
            OnComplete = (agent) => Console.WriteLine($"{agent.Id} завершил бой"),
            OnInterrupt = (agent) => Console.WriteLine($"{agent.Id} прервал бой")
        }
        .AddStep("ApproachEnemy", "MoveTo", "enemy_01")
        .AddStep("AttackEnemy", "Attack", "enemy_01")
        .AddStep("CheckEnemyHealth", "CheckHealth", "enemy_01")
        .AddStep("LootEnemy", "Loot", "enemy_01");
    }
    
    public static Scenario CreateTradeSequence()
    {
        return new Scenario("TradeSequence")
        {
            StartCondition = (agent) => agent.GetStat("wantsToTrade") == 1,
            OnStart = (agent) => Console.WriteLine($"{agent.Id} начинает торговлю"),
            OnComplete = (agent) => Console.WriteLine($"{agent.Id} завершил торговлю"),
            OnInterrupt = (agent) => Console.WriteLine($"{agent.Id} прервал торговлю")
        }
        .AddStep("FindTrader", "MoveTo", "trader_01")
        .AddStep("InitiateTrade", "StartTrade", "trader_01")
        .AddStep("NegotiatePrice", "Negotiate", "trader_01")
        .AddStep("CompleteTrade", "FinishTrade", "trader_01");
    }
    
    public static Scenario CreateSocialSequence()
    {
        return new Scenario("SocialSequence")
        {
            StartCondition = (agent) => agent.GetStat("socialNeed") > 70,
            OnStart = (agent) => Console.WriteLine($"{agent.Id} начинает социальное взаимодействие"),
            OnComplete = (agent) => Console.WriteLine($"{agent.Id} завершил социальное взаимодействие"),
            OnInterrupt = (agent) => Console.WriteLine($"{agent.Id} прервал социальное взаимодействие")
        }
        .AddStep("FindCompanion", "MoveTo", "companion_01")
        .AddStep("GreetCompanion", "Greet", "companion_01")
        .AddStep("HaveConversation", "Talk", "companion_01")
        .AddStep("SayGoodbye", "Farewell", "companion_01");
    }
    
    public static Scenario CreateCraftingSequence()
    {
        return new Scenario("CraftingSequence")
        {
            StartCondition = (agent) => agent.GetStat("craftingSkill") > 0,
            OnStart = (agent) => Console.WriteLine($"{agent.Id} начинает крафтинг"),
            OnComplete = (agent) => Console.WriteLine($"{agent.Id} завершил крафтинг"),
            OnInterrupt = (agent) => Console.WriteLine($"{agent.Id} прервал крафтинг")
        }
        .AddStep("GoToWorkbench", "MoveTo", "workbench_01")
        .AddStep("GatherMaterials", "GatherMaterials", "material_storage")
        .AddStep("CraftItem", "Craft", "workbench_01")
        .AddStep("StoreResult", "StoreItem", "inventory_storage");
    }
} 