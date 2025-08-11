# SmartCrowd - Фреймворк ИИ для игровых движков

## Этап 1: Каркас мира и данных ✅

Реализованы базовые структуры для описания игрового мира:

### Entity
- Уникальный идентификатор, теги, произвольные свойства
- Поддержка занятости (IsBusy, BusyByAgentId)
- Автоматическое добавление тега "agent" для NPC

### AgentState
- Статы, инвентарь, навыки NPC
- Текущая цель для действий
- Реестр доступных действий

### IWorldAdapter
- Интерфейс для связи с игровым движком
- Атомарные операции резервирования/освобождения объектов
- Пакетная загрузка данных

## Этап 2: Система действий ✅

Реализована универсальная модель действий с предусловиями и эффектами.

### GameAction
```csharp
var chopTree = new GameAction("ChopTree", 5.0f)
{
    Preconditions = (agentState, world) =>
    {
        var target = world.GetEntityById(agentState.CurrentTargetId);
        return target != null 
            && target.Tags.Contains("choppable")
            && !target.IsBusy
            && agentState.Inventory.ContainsKey("axe");
    },
    Effects = (agentState, world) =>
    {
        var target = world.GetEntityById(agentState.CurrentTargetId);
        // Логика действия
    },
    IsApplicableTo = (target) =>
        target.Tags.Contains("choppable") && !target.IsBusy
};
```

### Механика резервации
- Автоматическая проверка занятости целей
- Атомарное резервирование через `TryReserveEntity`
- Гарантированное освобождение после выполнения

### Регистрация действий
```csharp
var agent = new AgentState("woodcutter_001");
agent.Actions.Register(chopTree);
agent.Actions.Register(tradeAction);
```

## Этап 3: GOAP-планировщик ✅

Реализован Goal-Oriented Action Planning для автоматического построения планов действий.

### Goal
```csharp
var gatherWood = new Goal("GatherWood", 5)
{
    IsAchieved = (state) => state.Inventory.ContainsKey("wood") && state.Inventory["wood"] >= 10,
    GetRelevance = (state) => 
    {
        var woodCount = state.GetItemCount("wood");
        return Math.Max(0.1f, 1.0f - (woodCount / 10.0f));
    }
};
```

### Автоматическое планирование
```csharp
var planner = new AgentPlanner(world);
planner.AddGoal(gatherWood);

// Строим план
planner.BuildPlan(agentState);

// Выполняем пошагово
while (planner.ExecuteStep(agentState, world))
{
    // План выполняется автоматически
}
```

### Особенности GOAP
- **Динамический выбор целей**: по релевантности и приоритету
- **Критические цели**: автоматически перекрывают остальные
- **Алгоритм A***: эффективное построение планов
- **Реплан**: при изменении условий мира
- **Учёт занятости**: исключение конфликтов за ресурсы

## Примеры использования

### Создание мира
```csharp
var world = new InMemoryWorldAdapter();

var tree = new Entity("tree_001");
tree.Tags.Add("choppable");
tree.Props["hp"] = 100;

var agent = new AgentState("woodcutter_001");
agent.AddSkill("chop_wood");
agent.AddItem("axe", 1);

world.UpdateEntity(tree);
```

### Выполнение действия
```csharp
agent.SetTarget(tree.Id);
var action = agent.Actions.GetAction("ChopTree");

if (action.CanExecute(agent, world, tree))
{
    var success = action.Execute(agent, world, tree);
}
```

## Архитектура

```
SmartCrowd.Core/
├── Entities/                          # Базовые сущности мира
│   ├── Entity.cs                      # Объект мира с тегами и свойствами
│   └── AgentState.cs                  # Состояние NPC (статы, инвентарь, действия)
├── Actions/                           # Система действий
│   ├── GameAction.cs                  # Модель действия с предусловиями и эффектами
│   ├── ActionRegistry.cs              # Реестр доступных действий
│   ├── ExampleActions.cs              # Готовые примеры действий
│   └── ActionSystemDemo.cs            # Демонстрация системы действий
├── Goals/                             # Система целей для GOAP
│   ├── Goal.cs                        # Модель цели с приоритетом и критичностью
│   ├── GoalManager.cs                 # Управление целями агента
│   └── ExampleGoals.cs                # Примеры целей (выживание, сбор ресурсов)
├── Planner/                           # GOAP-планировщик
│   ├── GoapNode.cs                    # Узел GOAP-алгоритма A*
│   ├── GoapPlanner.cs                 # Основной GOAP-планировщик
│   ├── GoapPlan.cs                    # План действий с последовательностью
│   ├── AgentPlanner.cs                # Интегрированный планировщик для агента
│   └── GoapPlannerDemo.cs             # Демонстрация GOAP-планировщика
├── Scheduler/                         # Система расписаний и сценариев
│   ├── ScheduleEntry.cs               # Запись в расписании
│   ├── ScheduleManager.cs             # Управление расписанием
│   ├── Scenario.cs                    # Сюжетный сценарий
│   ├── ScenarioStep.cs                # Шаг сценария
│   ├── ScenarioManager.cs             # Управление сценариями
│   ├── IntegratedScheduler.cs         # Интегрированный планировщик
│   ├── ExampleSchedules.cs            # Примеры расписаний
│   ├── ExampleScenarios.cs            # Примеры сценариев
│   └── IntegratedSchedulerDemo.cs     # Демонстрация всей системы
├── WorldAdapter/                      # Адаптер для игрового мира
│   ├── IWorldAdapter.cs               # Интерфейс адаптера мира
│   └── InMemoryWorldAdapter.cs        # In-memory реализация для тестирования
└── Utils/                             # Вспомогательные инструменты
    └── Serialization/
        └── JsonSerialization.cs       # JSON сериализация/десериализация
```

## Подробное API

### Entities

#### Entity
```csharp
public class Entity
{
    public string Id { get; set; }                    // Уникальный идентификатор
    public HashSet<string> Tags { get; set; }         // Теги для классификации
    public Dictionary<string, object?> Props { get; set; } // Произвольные свойства
    public bool IsAgent { get; set; }                 // Является ли NPC
    public bool IsBusy { get; private set; }          // Занят ли объект
    public string? BusyByAgentId { get; private set; } // Кем занят
    
    // Методы
    public bool HasTag(string tag)                    // Проверка наличия тега
    public bool TryReserve(string agentId)            // Попытка резервирования
    public bool TryRelease(string agentId)            // Освобождение
}
```

#### AgentState
```csharp
public class AgentState
{
    public string Id { get; set; }                    // ID агента
    public Dictionary<string, float> Stats { get; set; } // Статистики (здоровье, голод)
    public Dictionary<string, int> Inventory { get; set; } // Инвентарь
    public Dictionary<string, int> Skills { get; set; } // Навыки
    public string? CurrentTargetId { get; set; }      // Текущая цель
    public ActionRegistry Actions { get; set; }       // Доступные действия
    
    // Методы
    public void SetStat(string name, float value)     // Установка стата
    public float GetStat(string name)                 // Получение стата
    public void AddItem(string item, int count)       // Добавление предмета
    public int GetItemCount(string item)              // Количество предметов
    public void SetTarget(string targetId)            // Установка цели
}
```

### Actions

#### GameAction
```csharp
public class GameAction
{
    public string Name { get; set; }                  // Название действия
    public float Cost { get; set; }                   // Стоимость для GOAP
    public Func<AgentState, IWorldAdapter, bool> Preconditions { get; set; } // Условия выполнения
    public Action<AgentState, IWorldAdapter> Effects { get; set; } // Эффекты выполнения
    public Func<Entity, bool> IsApplicableTo { get; set; } // Применимость к цели
    
    // Методы
    public bool CanExecute(AgentState, IWorldAdapter, Entity?) // Проверка возможности
    public bool Execute(AgentState, IWorldAdapter, Entity)     // Выполнение действия
}
```

#### ActionRegistry
```csharp
public class ActionRegistry
{
    public void Register(GameAction action)           // Регистрация действия
    public GameAction? GetAction(string name)         // Получение действия по имени
    public IEnumerable<GameAction> GetAllActions()    // Все доступные действия
    public bool HasAction(string name)                // Проверка наличия действия
}
```

### Goals

#### Goal
```csharp
public class Goal
{
    public string Name { get; set; }                  // Название цели
    public int Priority { get; set; }                 // Приоритет (выше = важнее)
    public Func<AgentState, bool> IsAchieved { get; set; } // Условие достижения
    public Func<AgentState, float> GetRelevance { get; set; } // Релевантность
    public bool IsCritical { get; set; }              // Критическая ли цель
    
    // Методы
    public bool IsAchieved(AgentState)                // Проверка достижения
    public float GetRelevance(AgentState)             // Получение релевантности
}
```

### Scheduler

#### ScheduleEntry
```csharp
public class ScheduleEntry
{
    public TimeSpan Time { get; set; }                // Время выполнения
    public string ActionName { get; set; }            // Имя действия
    public string? TargetId { get; set; }             // ID цели
    public string? TargetTag { get; set; }            // Тег для поиска цели
    public Dictionary<string, object> Parameters { get; set; } // Параметры
    public bool IsInterruptible { get; set; }         // Можно ли прервать
    public bool RetryIfBusy { get; set; }             // Повторить при занятости
    
    // Методы
    public bool CanExecuteAt(TimeSpan time)           // Проверка времени
    public TimeSpan GetNextExecutionTime(TimeSpan baseTime, int retryCount) // Следующая попытка
}
```

#### Scenario
```csharp
public class Scenario
{
    public string Name { get; set; }                  // Название сценария
    public List<ScenarioStep> Steps { get; set; }    // Шаги сценария
    public Action? OnInterrupt { get; set; }         // Обработчик прерывания
    public Action? OnComplete { get; set; }          // Обработчик завершения
    
    // Методы
    public bool IsActive { get; }                     // Активен ли сценарий
    public int CurrentStepIndex { get; }              // Текущий шаг
    public ScenarioStep? CurrentStep { get; }         // Текущий шаг
}
```

#### IntegratedScheduler
```csharp
public class IntegratedScheduler
{
    // События
    public event Action<string>? OnModeChanged;       // Изменение режима
    public event Action<ScheduleEntry>? OnScheduleActionExecuted; // Выполнение по расписанию
    public event Action<Scenario>? OnScenarioActionExecuted; // Выполнение сценария
    
    // Методы
    public void SetSchedule(IEnumerable<ScheduleEntry>) // Установка расписания
    public void RegisterScenario(Scenario)             // Регистрация сценария
    public void AddGoal(Goal)                          // Добавление цели
    public bool Update(AgentState, TimeSpan)           // Обновление планировщика
    public bool StartScenario(string, AgentState)      // Запуск сценария
    public void PauseSchedule()                        // Приостановка расписания
    public void ResumeSchedule()                       // Возобновление расписания
    public string GetCurrentMode()                     // Текущий режим
    public SchedulerStatus GetStatus()                 // Статус планировщика
}
```

## Практические примеры

### Создание фермера с полным циклом жизни

```csharp
// 1. Создаем мир
var world = new InMemoryWorldAdapter();

// 2. Создаем объекты
var farm = new Entity("farm_001");
farm.Tags.Add("farm");
farm.Tags.Add("workplace");

var field = new Entity("field_001");
field.Tags.Add("field");
field.Tags.Add("plantable");
field.Props["fertility"] = 0.8f;

var house = new Entity("house_001");
house.Tags.Add("house");
house.Tags.Add("resting");

var market = new Entity("market_001");
market.Tags.Add("market");
market.Tags.Add("trading");

world.UpdateEntity(farm);
world.UpdateEntity(field);
world.UpdateEntity(house);
world.UpdateEntity(market);

// 3. Создаем фермера
var farmer = new AgentState("farmer_001");
farmer.SetStat("health", 100);
farmer.SetStat("hunger", 30);
farmer.SetStat("energy", 100);
farmer.SetStat("money", 50);
farmer.AddItem("seeds", 10);
farmer.AddItem("tools", 1);

// 4. Регистрируем действия
farmer.Actions.Register(ExampleActions.CreatePlantAction());
farmer.Actions.Register(ExampleActions.CreateHarvestAction());
farmer.Actions.Register(ExampleActions.CreateSellAction());
farmer.Actions.Register(ExampleActions.CreateRestAction());

// 5. Создаем расписание
var dailySchedule = new List<ScheduleEntry>
{
    new ScheduleEntry
    {
        Time = new TimeSpan(6, 0, 0),
        ActionName = "WakeUp",
        IsInterruptible = false
    },
    new ScheduleEntry
    {
        Time = new TimeSpan(7, 0, 0),
        ActionName = "Plant",
        TargetId = "field_001",
        RetryIfBusy = true
    },
    new ScheduleEntry
    {
        Time = new TimeSpan(12, 0, 0),
        ActionName = "Rest",
        TargetId = "house_001",
        IsInterruptible = true
    },
    new ScheduleEntry
    {
        Time = new TimeSpan(14, 0, 0),
        ActionName = "Harvest",
        TargetId = "field_001",
        RetryIfBusy = true
    },
    new ScheduleEntry
    {
        Time = new TimeSpan(16, 0, 0),
        ActionName = "Sell",
        TargetId = "market_001",
        RetryIfBusy = false
    },
    new ScheduleEntry
    {
        Time = new TimeSpan(20, 0, 0),
        ActionName = "Rest",
        TargetId = "house_001",
        IsInterruptible = false
    }
};

// 6. Создаем цели
var survivalGoal = new Goal("Survive", 10)
{
    IsCritical = (state) => state.GetStat("health") < 20,
    IsAchieved = (state) => state.GetStat("health") > 80,
    GetRelevance = (state) => Math.Max(0.1f, 1.0f - (state.GetStat("health") / 100.0f))
};

var wealthGoal = new Goal("GetRich", 5)
{
    IsAchieved = (state) => state.GetStat("money") >= 1000,
    GetRelevance = (state) => Math.Max(0.1f, state.GetStat("money") / 1000.0f)
};

// 7. Создаем планировщик
var scheduler = new IntegratedScheduler(world);
scheduler.SetSchedule(dailySchedule);
scheduler.AddGoal(survivalGoal);
scheduler.AddGoal(wealthGoal);

// 8. Запускаем симуляцию
var currentTime = new TimeSpan(6, 0, 0);
var dayCount = 0;

while (dayCount < 7) // Симулируем неделю
{
    Console.WriteLine($"\n=== День {dayCount + 1} ===");
    
    for (int hour = 6; hour <= 22; hour++)
    {
        currentTime = new TimeSpan(hour, 0, 0);
        
        // Обновляем планировщик
        var actionPerformed = scheduler.Update(farmer, currentTime);
        
        // Показываем состояние
        var status = scheduler.GetStatus();
        Console.WriteLine($"{currentTime:hh\\:mm} - Режим: {status.Mode}");
        
        if (actionPerformed)
        {
            Console.WriteLine($"  Выполнено действие");
        }
        
        // Показываем статистику
        Console.WriteLine($"  Здоровье: {farmer.GetStat("health"):F1}");
        Console.WriteLine($"  Голод: {farmer.GetStat("hunger"):F1}");
        Console.WriteLine($"  Энергия: {farmer.GetStat("energy"):F1}");
        Console.WriteLine($"  Деньги: {farmer.GetStat("money"):F1}");
        
        // Имитируем случайные события
        if (Random.Shared.NextDouble() < 0.1) // 10% шанс
        {
            Console.WriteLine("  !!! Случайное событие: дождь !!!");
            farmer.SetStat("energy", Math.Max(0, farmer.GetStat("energy") - 10));
        }
    }
    
    // Сброс времени для следующего дня
    currentTime = new TimeSpan(6, 0, 0);
    dayCount++;
}

// 9. Итоговая статистика
var finalStatus = scheduler.GetStatus();
Console.WriteLine($"\n=== Итоги недели ===");
Console.WriteLine($"Финальный режим: {finalStatus.Mode}");
Console.WriteLine($"Выполнено действий по расписанию: {finalStatus.ScheduleEntries}");
Console.WriteLine($"Финальное состояние фермера:");
Console.WriteLine($"  Здоровье: {farmer.GetStat("health"):F1}");
Console.WriteLine($"  Деньги: {farmer.GetStat("money"):F1}");
Console.WriteLine($"  Семена: {farmer.GetItemCount("seeds")}");
```

### Создание квестового NPC

```csharp
// 1. Создаем квестовый сценарий
var questSteps = new List<ScenarioStep>
{
    new ScenarioStep("ApproachPlayer", "Walk")
    {
        TargetTag = "player",
        Condition = (agent) => agent.GetStat("questActive") == 1,
        IsInterruptible = true
    },
    new ScenarioStep("GreetPlayer", "Talk")
    {
        Parameters = new Dictionary<string, object> 
        { 
            ["dialogueId"] = "quest_greeting" 
        },
        Condition = (agent) => agent.GetStat("playerNearby") == 1,
        IsInterruptible = false
    },
    new ScenarioStep("GiveQuest", "GiveQuest")
    {
        Parameters = new Dictionary<string, object> 
        { 
            ["questId"] = "collect_herbs",
            ["questDescription"] = "Собери 5 лечебных трав"
        },
        Condition = (agent) => agent.GetStat("greetingComplete") == 1,
        IsInterruptible = false
    },
    new ScenarioStep("WaitForCompletion", "Wait")
    {
        Condition = (agent) => agent.GetStat("questGiven") == 1,
        IsInterruptible = true,
        WaitForCompletion = false
    },
    new ScenarioStep("RewardPlayer", "GiveReward")
    {
        Parameters = new Dictionary<string, object> 
        { 
            ["rewardType"] = "gold",
            ["rewardAmount"] = 100
        },
        Condition = (agent) => agent.GetStat("questCompleted") == 1,
        IsInterruptible = false
    }
};

var questScenario = new Scenario("HerbQuest")
{
    Steps = questSteps,
    OnInterrupt = () => Console.WriteLine("Квест прерван!"),
    OnComplete = () => Console.WriteLine("Квест завершен!")
};

// 2. Регистрируем в планировщике
scheduler.RegisterScenario(questScenario);

// 3. Запускаем квест
farmer.SetStat("questActive", 1);
scheduler.StartScenario("HerbQuest", farmer);
```

## Отладка и мониторинг

### Логирование событий
```csharp
// Подписываемся на все события планировщика
scheduler.OnModeChanged += (mode) => 
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Режим изменён: {mode}");

scheduler.OnScheduleActionExecuted += (entry) => 
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Расписание: {entry.ActionName}");

scheduler.OnScenarioActionExecuted += (scenario) => 
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Сценарий: {scenario.Name}");

// Логируем изменения состояния агента
farmer.OnStatChanged += (statName, oldValue, newValue) =>
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {statName}: {oldValue} -> {newValue}");

farmer.OnInventoryChanged += (itemName, oldCount, newCount) =>
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {itemName}: {oldCount} -> {newCount}");
```

### Отладочная информация
```csharp
// Получаем детальную информацию о состоянии
var status = scheduler.GetStatus();
Console.WriteLine($"=== Статус планировщика ===");
Console.WriteLine($"Режим: {status.Mode}");
Console.WriteLine($"Расписание активно: {status.ScheduleActive}");
Console.WriteLine($"Записей в расписании: {status.ScheduleEntries}");
Console.WriteLine($"Сценарий активен: {status.ScenarioActive}");
Console.WriteLine($"Текущий сценарий: {status.CurrentScenario ?? "нет"}");
Console.WriteLine($"Текущий шаг: {status.CurrentStep}");
Console.WriteLine($"GOAP активен: {status.GoapActive}");
Console.WriteLine($"Количество целей: {status.GoapGoals}");

// Информация о текущем расписании
var scheduleEntries = scheduler.GetScheduleEntries();
Console.WriteLine($"\n=== Текущее расписание ===");
foreach (var entry in scheduleEntries)
{
    var canExecute = entry.CanExecuteAt(DateTime.Now.TimeOfDay);
    Console.WriteLine($"{entry.Time:hh\\:mm} - {entry.ActionName} - {entry.TargetId} - Выполнимо: {canExecute}");
}

// Информация о целях
var goals = scheduler.GetAllGoals();
Console.WriteLine($"\n=== Активные цели ===");
foreach (var goal in goals)
{
    var relevance = goal.GetRelevance(farmer);
    var achieved = goal.IsAchieved(farmer);
    Console.WriteLine($"{goal.Name} - Приоритет: {goal.Priority}, Релевантность: {relevance:F2}, Достигнута: {achieved}");
}
```

## Производительность и оптимизация

### Рекомендации по производительности
1. **Ограничение количества NPC**: Не более 100-200 NPC на планировщик
2. **Кэширование планов**: GOAP-планы можно кэшировать для повторного использования
3. **Пакетная обработка**: Обновляйте несколько NPC одновременно
4. **LOD для планировщика**: Дальние NPC могут обновляться реже

### Профилирование
```csharp
var stopwatch = Stopwatch.StartNew();

// Выполняем обновление
var actionPerformed = scheduler.Update(agent, currentTime);

stopwatch.Stop();
Console.WriteLine($"Обновление заняло: {stopwatch.ElapsedMilliseconds}ms");

// Мониторим использование памяти
var memoryUsage = GC.GetTotalMemory(false);
Console.WriteLine($"Использование памяти: {memoryUsage / 1024 / 1024}MB");
```

## Интеграция с игровыми движками

### Unity
```csharp
// MonoBehaviour для интеграции с Unity
public class SmartCrowdAgent : MonoBehaviour
{
    private AgentState _agentState;
    private IntegratedScheduler _scheduler;
    private IWorldAdapter _worldAdapter;
    
    void Start()
    {
        // Инициализация
        _agentState = new AgentState(gameObject.name);
        _worldAdapter = new UnityWorldAdapter(); // Адаптер для Unity
        _scheduler = new IntegratedScheduler(_worldAdapter);
        
        // Настройка
        SetupAgent();
    }
    
    void Update()
    {
        // Обновляем планировщик каждый кадр
        var currentTime = DateTime.Now.TimeOfDay;
        var actionPerformed = _scheduler.Update(_agentState, currentTime);
        
        if (actionPerformed)
        {
            // Анимация или визуальные эффекты
            PlayActionAnimation();
        }
    }
}
```

### Unreal Engine
```csharp
// C++ класс для интеграции с Unreal
UCLASS()
class ASmartCrowdAgent : public AActor
{
    GENERATED_BODY()
    
private:
    TSharedPtr<AgentState> AgentState;
    TSharedPtr<IntegratedScheduler> Scheduler;
    TSharedPtr<IWorldAdapter> WorldAdapter;
    
public:
    virtual void BeginPlay() override
    {
        Super::BeginPlay();
        
        // Инициализация
        AgentState = MakeShared<AgentState>(GetName());
        WorldAdapter = MakeShared<UnrealWorldAdapter>();
        Scheduler = MakeShared<IntegratedScheduler>(WorldAdapter);
        
        // Настройка
        SetupAgent();
    }
    
    virtual void Tick(float DeltaTime) override
    {
        Super::Tick(DeltaTime);
        
        // Обновляем планировщик
        FDateTime Now = FDateTime::Now();
        FTimespan CurrentTime = Now.GetTimeOfDay();
        
        bool ActionPerformed = Scheduler->Update(*AgentState, CurrentTime);
        
        if (ActionPerformed)
        {
            // Анимация или визуальные эффекты
            PlayActionAnimation();
        }
    }
};
```

## Этап 4: Расписания и сценарии ✅

Реализована система планировщиков, позволяющая NPC жить по расписанию и участвовать в сюжетных сценариях.

### Расписания (Schedules)

Система расписаний позволяет NPC выполнять действия в заданное время, автоматически переключаясь в GOAP-режим при критических ситуациях.

#### ScheduleEntry
```csharp
var breakfastEntry = new ScheduleEntry
{
    Time = new TimeSpan(8, 0, 0),        // 8:00 утра
    ActionName = "EatBreakfast",          // Имя действия
    TargetId = "table_01",                // ID цели (опционально)
    Parameters = new Dictionary<string, object> 
    { 
        ["foodType"] = "porridge" 
    },
    IsInterruptible = true,               // Можно ли прервать
    RetryIfBusy = true                    // Повторить, если цель занята
};
```

#### Управление расписанием
```csharp
// Устанавливаем расписание на день
npc.Schedule.Set(new List<ScheduleEntry>
{
    new ScheduleEntry
    {
        Time = new TimeSpan(6, 0, 0),
        ActionName = "WakeUp",
        IsInterruptible = false
    },
    new ScheduleEntry
    {
        Time = new TimeSpan(8, 0, 0),
        ActionName = "EatBreakfast",
        TargetId = "table_01",
        RetryIfBusy = true
    },
    new ScheduleEntry
    {
        Time = new TimeSpan(9, 0, 0),
        ActionName = "ChopWood",
        TargetId = "tree_05",
        IsInterruptible = false,
        RetryIfBusy = false
    }
});

// Управление расписанием
npc.Schedule.Pause();    // Приостановить
npc.Schedule.Resume();   // Возобновить
```

#### Обработка занятости ресурсов
- **RetryIfBusy = true**: NPC ждет освобождения цели и повторяет попытку
- **RetryIfBusy = false**: NPC переходит к следующему действию
- Автоматическое переключение в GOAP при недоступности ресурсов

### Сценарии (Scripted Sequences)

Сценарии - это жесткие последовательности действий с условиями перехода, используемые для сюжетных NPC.

#### ScenarioStep
```csharp
var questStep = new ScenarioStep("TalkToTrader", "Trade")
{
    TargetId = "trader_01",               // Фиксированная цель
    TargetTag = "trader",                 // Или поиск по тегу
    Parameters = new Dictionary<string, object> 
    { 
        ["questId"] = "main_quest_1" 
    },
    Condition = (agentState) => agentState.GetStat("questActive") == 1,
    IsInterruptible = true,
    WaitForCompletion = true
};
```

#### Управление сценариями
```csharp
// Регистрируем сценарий
var questScenario = new Scenario("MainQuest")
{
    Steps = new List<ScenarioStep> { step1, step2, step3 },
    OnInterrupt = () => Console.WriteLine("Квест прерван!")
};

npc.Scenarios.Register(questScenario);

// Запускаем сценарий
npc.Scenarios.Start("MainQuest", npc);

// Автоматическое прерывание при критических целях
if (npc.GetStat("health") < 20)
{
    // Сценарий автоматически прерывается
    // NPC переключается в GOAP-режим
}
```

### Интегрированный планировщик

`IntegratedScheduler` объединяет все три режима работы NPC:

#### Режимы работы
1. **Schedule Mode**: Выполнение действий по расписанию
2. **Scenario Mode**: Выполнение сюжетных сценариев  
3. **GOAP Mode**: Автономное планирование при критических ситуациях

#### Автоматическое переключение режимов
```csharp
var scheduler = new IntegratedScheduler(world);

// Добавляем критические цели
scheduler.AddGoal(new Goal("Survive", 10)
{
    IsCritical = (state) => state.GetStat("health") < 20
});

// Добавляем обычные цели
scheduler.AddGoal(new Goal("GatherResources", 5)
{
    IsAchieved = (state) => state.GetItemCount("wood") >= 10
});

// Устанавливаем расписание
scheduler.SetSchedule(dailySchedule);

// Обновляем планировщик
while (true)
{
    var actionPerformed = scheduler.Update(agent, currentTime);
    if (actionPerformed)
    {
        Console.WriteLine("Действие выполнено");
    }
    
    // Автоматическое переключение между режимами
    var status = scheduler.GetStatus();
    Console.WriteLine($"Режим: {status.Mode}");
    
    currentTime = currentTime.Add(TimeSpan.FromHours(1));
}
```

#### События планировщика
```csharp
scheduler.OnModeChanged += (mode) => 
    Console.WriteLine($"Режим изменён: {mode}");

scheduler.OnScheduleActionExecuted += (entry) => 
    Console.WriteLine($"Расписание: выполнено {entry.ActionName}");

scheduler.OnScenarioActionExecuted += (scenario) => 
    Console.WriteLine($"Сценарий: выполнено {scenario.Name}");
```

### Примеры использования

#### Создание NPC с расписанием
```csharp
// Создаем NPC
var farmer = new AgentState("farmer_001");
farmer.SetStat("hunger", 30);
farmer.SetStat("energy", 100);

// Регистрируем действия
farmer.Actions.Register(ExampleActions.CreateChopTreeAction());
farmer.Actions.Register(ExampleActions.CreateTradeAction());

// Создаем планировщик
var scheduler = new IntegratedScheduler(world);
scheduler.SetSchedule(CreateFarmerSchedule());

// Добавляем цели
scheduler.AddGoal(ExampleGoals.CreateSurvivalGoal());
scheduler.AddGoal(ExampleGoals.CreateResourceGoal());

// Регистрируем сценарии
scheduler.RegisterScenario(ExampleScenarios.CreateQuestSequence());
```

#### Полный цикл работы
```csharp
var currentTime = new TimeSpan(6, 0, 0); // 6:00 утра

for (int hour = 6; hour <= 22; hour++)
{
    currentTime = new TimeSpan(hour, 0, 0);
    Console.WriteLine($"\n--- {currentTime:hh\\:mm} ---");
    
    // Обновляем планировщик
    var actionPerformed = scheduler.Update(farmer, currentTime);
    
    // Показываем состояние
    var status = scheduler.GetStatus();
    Console.WriteLine($"Режим: {status.Mode}");
    Console.WriteLine($"Голод: {farmer.GetStat("hunger")}");
    Console.WriteLine($"Энергия: {farmer.GetStat("energy")}");
    
    // Имитируем критические ситуации
    if (hour == 10 && farmer.GetStat("hunger") > 60)
    {
        Console.WriteLine("!!! Критический голод - переключение в GOAP !!!");
        farmer.SetStat("hunger", 85);
    }
}
```

## Следующие этапы

- **Этап 5**: Интеграция с игровыми движками
- **Этап 6**: Многопоточность и оптимизация
- **Этап 7**: Машинное обучение и адаптация

## Требования

- **.NET 8.0** или выше
- **System.Text.Json** для сериализации
- **Поддержка nullable reference types**
- **Минимум 4GB RAM** для работы с большим количеством NPC
- **Процессор**: Рекомендуется 4+ ядра для многопоточности

## Установка и сборка

### Через NuGet (планируется)
```bash
dotnet add package SmartCrowd.Core
```

### Из исходного кода
```bash
git clone https://github.com/your-username/SmartCrowd.git
cd SmartCrowd
dotnet build
dotnet test
```

### Запуск демонстрации
```bash
dotnet run --project SmartCrowd.Core.csproj
```

## Roadmap

### Версия 1.0 (Текущая)
- ✅ Базовая архитектура
- ✅ Система действий
- ✅ GOAP-планировщик
- ✅ Расписания и сценарии

### Версия 1.1 (Планируется)
- 🔄 Многопоточность
- 🔄 Кэширование планов
- 🔄 Оптимизация производительности

### Версия 2.0 (Долгосрочно)
- 🔮 Машинное обучение
- 🔮 Адаптивное поведение
- 🔮 Интеграция с Unity/Unreal
- 🔮 Сетевые возможности

---

**SmartCrowd Framework** - мощный фреймворк для создания умных NPC в играх. Создавайте живые миры с персонажами, которые думают, планируют и адаптируются к изменениям! 