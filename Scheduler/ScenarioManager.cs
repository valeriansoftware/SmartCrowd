using SmartCrowd.Core.Entities;
using SmartCrowd.Core.Actions;
using SmartCrowd.Core.WorldAdapter;

namespace SmartCrowd.Core.Scheduler;

public class ScenarioManager
{
    private readonly Dictionary<string, Scenario> _scenarios = new(StringComparer.OrdinalIgnoreCase);
    private Scenario? _currentScenario;
    private int _currentStepIndex = -1;
    
    public bool IsActive => _currentScenario != null;
    public Scenario? CurrentScenario => _currentScenario;
    public int CurrentStepIndex => _currentStepIndex;
    
    public event Action<Scenario>? OnScenarioStarted;
    public event Action<Scenario>? OnScenarioCompleted;
    public event Action<Scenario>? OnScenarioInterrupted;
    
    /// <summary>
    /// Регистрирует сценарий
    /// </summary>
    public void RegisterScenario(Scenario scenario)
    {
        _scenarios[scenario.Name] = scenario;
    }
    
    /// <summary>
    /// Регистрирует несколько сценариев
    /// </summary>
    public void RegisterScenarios(IEnumerable<Scenario> scenarios)
    {
        foreach (var scenario in scenarios)
        {
            RegisterScenario(scenario);
        }
    }
    
    /// <summary>
    /// Запускает сценарий
    /// </summary>
    public bool StartScenario(string scenarioName, AgentState agentState)
    {
        if (!_scenarios.TryGetValue(scenarioName, out var scenario))
        {
            return false;
        }
        
        if (!scenario.CanStart(agentState))
        {
            return false;
        }
        
        // Прерываем текущий сценарий, если он есть
        if (_currentScenario != null)
        {
            InterruptCurrentScenario(agentState);
        }
        
        _currentScenario = scenario;
        _currentStepIndex = 0;
        
        scenario.OnStart?.Invoke(agentState);
        OnScenarioStarted?.Invoke(scenario);
        
        return true;
    }
    
    /// <summary>
    /// Выполняет следующий шаг текущего сценария
    /// </summary>
    public bool ExecuteNextStep(AgentState agentState, IWorldAdapter world)
    {
        if (_currentScenario == null || _currentStepIndex < 0 || _currentStepIndex >= _currentScenario.Steps.Count)
        {
            return false;
        }
        
        var step = _currentScenario.Steps[_currentStepIndex];
        
        // Проверяем условие перехода
        if (!step.CanProceed(agentState))
        {
            return false;
        }
        
        // Получаем цель для действия
        var targetId = step.GetTargetId(world, agentState);
        if (targetId == null)
        {
            // Цель недоступна, прерываем сценарий
            InterruptCurrentScenario(agentState);
            return false;
        }
        
        // Устанавливаем цель для агента
        agentState.SetTarget(targetId);
        
        // Получаем действие
        var action = agentState.Actions.GetAction(step.ActionName);
        if (action == null)
        {
            // Действие не найдено, прерываем сценарий
            InterruptCurrentScenario(agentState);
            return false;
        }
        
        // Получаем цель из мира
        var target = world.GetEntityById(targetId);
        if (target == null)
        {
            // Цель исчезла, прерываем сценарий
            InterruptCurrentScenario(agentState);
            return false;
        }
        
        // Проверяем, можно ли выполнить действие
        if (!action.CanExecute(agentState, world, target))
        {
            // Действие недоступно, прерываем сценарий
            InterruptCurrentScenario(agentState);
            return false;
        }
        
        // Выполняем действие
        var success = action.Execute(agentState, world, target);
        
        if (success)
        {
            // Переходим к следующему шагу
            _currentStepIndex++;
            
            // Проверяем, завершён ли сценарий
            if (_currentStepIndex >= _currentScenario.Steps.Count)
            {
                CompleteCurrentScenario(agentState);
            }
            
            return true;
        }
        else
        {
            // Действие не выполнилось, прерываем сценарий
            InterruptCurrentScenario(agentState);
            return false;
        }
    }
    
    /// <summary>
    /// Прерывает текущий сценарий
    /// </summary>
    public void InterruptCurrentScenario(AgentState agentState)
    {
        if (_currentScenario != null)
        {
            _currentScenario.OnInterrupt?.Invoke(agentState);
            OnScenarioInterrupted?.Invoke(_currentScenario);
            _currentScenario = null;
            _currentStepIndex = -1;
        }
    }
    
    /// <summary>
    /// Завершает текущий сценарий
    /// </summary>
    private void CompleteCurrentScenario(AgentState agentState)
    {
        if (_currentScenario != null)
        {
            _currentScenario.OnComplete?.Invoke(agentState);
            OnScenarioCompleted?.Invoke(_currentScenario);
            
            if (_currentScenario.IsLooping)
            {
                // Запускаем сценарий заново
                _currentStepIndex = 0;
            }
            else
            {
                // Завершаем сценарий
                _currentScenario = null;
                _currentStepIndex = -1;
            }
        }
    }
    
    /// <summary>
    /// Получает сценарий по имени
    /// </summary>
    public Scenario? GetScenario(string name)
    {
        _scenarios.TryGetValue(name, out var scenario);
        return scenario;
    }
    
    /// <summary>
    /// Получает все зарегистрированные сценарии
    /// </summary>
    public IEnumerable<Scenario> GetAllScenarios() => _scenarios.Values;
    
    /// <summary>
    /// Удаляет сценарий
    /// </summary>
    public void RemoveScenario(string name)
    {
        _scenarios.Remove(name);
    }
    
    /// <summary>
    /// Очищает все сценарии
    /// </summary>
    public void Clear()
    {
        _scenarios.Clear();
        _currentScenario = null;
        _currentStepIndex = -1;
    }
} 