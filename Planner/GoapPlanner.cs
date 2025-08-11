using SmartCrowd.Core.Entities;
using SmartCrowd.Core.Actions;
using SmartCrowd.Core.Goals;
using SmartCrowd.Core.WorldAdapter;

namespace SmartCrowd.Core.Planner;

public class GoapPlanner
{
    private readonly IWorldAdapter _world;
    private readonly int _maxIterations;
    
    public GoapPlanner(IWorldAdapter world, int maxIterations = 1000)
    {
        _world = world;
        _maxIterations = maxIterations;
    }
    
    /// <summary>
    /// Строит план действий для достижения цели
    /// </summary>
    public GoapPlan? BuildPlan(AgentState agentState, Goal goal)
    {
        var startNode = new GoapNode(agentState);
        var openSet = new PriorityQueue<GoapNode, float>();
        var closedSet = new HashSet<string>();
        
        openSet.Enqueue(startNode, startNode.F);
        
        var iterations = 0;
        
        while (openSet.Count > 0 && iterations < _maxIterations)
        {
            iterations++;
            
            var currentNode = openSet.Dequeue();
            
            // Проверяем, достигнута ли цель
            if (goal.IsAchieved(currentNode.AgentState))
            {
                return new GoapPlan(currentNode.GetActionSequence(), goal);
            }
            
            // Добавляем узел в закрытое множество
            var stateHash = GetStateHash(currentNode.AgentState);
            if (closedSet.Contains(stateHash))
            {
                continue;
            }
            closedSet.Add(stateHash);
            
            // Генерируем соседние узлы
            var neighbors = GenerateNeighbors(currentNode, agentState);
            
            foreach (var neighbor in neighbors)
            {
                if (neighbor == null) continue;
                
                var neighborHash = GetStateHash(neighbor.AgentState);
                if (closedSet.Contains(neighborHash))
                {
                    continue;
                }
                
                // Вычисляем эвристику до цели
                neighbor.H = CalculateHeuristic(neighbor.AgentState, goal);
                
                openSet.Enqueue(neighbor, neighbor.F);
            }
        }
        
        // План не найден
        return null;
    }
    
    /// <summary>
    /// Генерирует соседние узлы, применяя доступные действия
    /// </summary>
    private IEnumerable<GoapNode> GenerateNeighbors(GoapNode currentNode, AgentState originalAgentState)
    {
        var neighbors = new List<GoapNode>();
        
        // Получаем все доступные действия агента
        var availableActions = originalAgentState.Actions.GetAllActions();
        
        // Получаем все доступные цели в мире
        var availableTargets = _world.GetAllEntities()
            .Where(e => !e.IsBusy || string.Equals(e.BusyByAgentId, originalAgentState.Id, StringComparison.Ordinal))
            .ToList();
        
        foreach (var action in availableActions)
        {
            foreach (var target in availableTargets)
            {
                // Проверяем, применимо ли действие к цели
                if (!action.IsApplicableTo(target))
                {
                    continue;
                }
                
                // Применяем действие
                var newNode = currentNode.ApplyAction(action, target, _world);
                if (newNode != null)
                {
                    neighbors.Add(newNode);
                }
            }
        }
        
        return neighbors;
    }
    
    /// <summary>
    /// Вычисляет эвристическую оценку расстояния до цели
    /// </summary>
    private float CalculateHeuristic(AgentState agentState, Goal goal)
    {
        // Простая эвристика: если цель достигнута, расстояние = 0
        if (goal.IsAchieved(agentState))
        {
            return 0;
        }
        
        // Иначе возвращаем базовую стоимость
        return 1.0f;
    }
    
    /// <summary>
    /// Создаёт хеш состояния агента для отслеживания посещённых узлов
    /// </summary>
    private string GetStateHash(AgentState agentState)
    {
        // Простой хеш на основе ключевых параметров
        var statsHash = string.Join("|", agentState.Stats.OrderBy(s => s.Key).Select(s => $"{s.Key}:{s.Value}"));
        var inventoryHash = string.Join("|", agentState.Inventory.OrderBy(i => i.Key).Select(i => $"{i.Key}:{i.Value}"));
        var skillsHash = string.Join("|", agentState.Skills.OrderBy(s => s));
        
        return $"{agentState.Id}|{statsHash}|{inventoryHash}|{skillsHash}";
    }
    
    /// <summary>
    /// Быстрый реплан при изменении условий
    /// </summary>
    public GoapPlan? Replan(AgentState agentState, Goal goal, GoapPlan? currentPlan)
    {
        // Если план существует и цель всё ещё актуальна, пытаемся продолжить
        if (currentPlan != null && !goal.IsAchieved(agentState))
        {
            // Проверяем, можно ли продолжить выполнение текущего плана
            var remainingActions = currentPlan.GetRemainingActions(agentState, _world);
            if (remainingActions.Any())
            {
                return new GoapPlan(remainingActions, goal);
            }
        }
        
        // Строим новый план
        return BuildPlan(agentState, goal);
    }
} 