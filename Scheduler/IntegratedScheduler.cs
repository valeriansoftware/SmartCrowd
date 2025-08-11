using SmartCrowd.Core.Entities;
using SmartCrowd.Core.Actions;
using SmartCrowd.Core.Goals;
using SmartCrowd.Core.Planner;
using SmartCrowd.Core.WorldAdapter;

namespace SmartCrowd.Core.Scheduler;

public class SchedulerStatus
{
    public string Mode { get; set; } = string.Empty;
    public bool ScheduleActive { get; set; }
    public int ScheduleEntries { get; set; }
    public bool ScenarioActive { get; set; }
    public string? CurrentScenario { get; set; }
    public int CurrentStep { get; set; }
    public bool GoapActive { get; set; }
    public int GoapGoals { get; set; }
}

public class IntegratedScheduler
{
    private readonly ScheduleManager _scheduleManager;
    private readonly ScenarioManager _scenarioManager;
    private readonly AgentPlanner _goapPlanner;
    private readonly IWorldAdapter _world;
    
    private bool _isInGoapMode = false;
    private TimeSpan _lastScheduleTime = TimeSpan.Zero;
    
    public event Action<string>? OnModeChanged;
    public event Action<ScheduleEntry>? OnScheduleActionExecuted;
    public event Action<Scenario>? OnScenarioActionExecuted;
    
    public IntegratedScheduler(IWorldAdapter world)
    {
        _world = world;
        _scheduleManager = new ScheduleManager();
        _scenarioManager = new ScenarioManager();
        _goapPlanner = new AgentPlanner(world);
        
        // Подписываемся на события
        _scheduleManager.OnActionCompleted += (entry) => OnScheduleActionExecuted?.Invoke(entry);
        _scenarioManager.OnScenarioCompleted += (scenario) => SwitchToScheduleMode();
        _scenarioManager.OnScenarioInterrupted += (scenario) => SwitchToGoapMode();
    }
    
    /// <summary>
    /// Устанавливает расписание для агента
    /// </summary>
    public void SetSchedule(IEnumerable<ScheduleEntry> entries)
    {
        _scheduleManager.SetSchedule(entries);
    }
    
    /// <summary>
    /// Регистрирует сценарий для агента
    /// </summary>
    public void RegisterScenario(Scenario scenario)
    {
        _scenarioManager.RegisterScenario(scenario);
    }
    
    /// <summary>
    /// Добавляет цель для GOAP-планировщика
    /// </summary>
    public void AddGoal(Goal goal)
    {
        _goapPlanner.AddGoal(goal);
    }
    
    /// <summary>
    /// Обновляет время и выполняет соответствующие действия
    /// </summary>
    public bool Update(AgentState agentState, TimeSpan currentTime)
    {
        // Проверяем, нужно ли переключиться в GOAP-режим
        if (ShouldSwitchToGoap(agentState))
        {
            SwitchToGoapMode();
        }
        
        // Выполняем действия в зависимости от текущего режима
        if (_isInGoapMode)
        {
            return ExecuteGoapStep(agentState);
        }
        else if (_scenarioManager.IsActive)
        {
            return ExecuteScenarioStep(agentState);
        }
        else
        {
            return ExecuteScheduleStep(agentState, currentTime);
        }
    }
    
    /// <summary>
    /// Проверяет, нужно ли переключиться в GOAP-режим
    /// </summary>
    private bool ShouldSwitchToGoap(AgentState agentState)
    {
        // Проверяем критические цели
        var goals = _goapPlanner.GetAllGoals();
        foreach (var goal in goals)
        {
            if (goal.IsCritical(agentState))
            {
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Выполняет шаг расписания
    /// </summary>
    private bool ExecuteScheduleStep(AgentState agentState, TimeSpan currentTime)
    {
        // Обновляем время в расписании
        var actionsToExecute = _scheduleManager.UpdateTime(currentTime);
        
        foreach (var entry in actionsToExecute)
        {
            if (!_scheduleManager.IsActive)
            {
                break;
            }
            
            // Получаем действие
            var action = agentState.Actions.GetAction(entry.ActionName);
            if (action == null)
            {
                _scheduleManager.MarkActionSkipped(entry.ActionName);
                continue;
            }
            
            // Проверяем цель
            if (!string.IsNullOrEmpty(entry.TargetId))
            {
                var target = _world.GetEntityById(entry.TargetId);
                if (target == null)
                {
                    _scheduleManager.MarkActionSkipped(entry.ActionName);
                    continue;
                }
                
                // Проверяем занятость
                if (target.IsBusy && !string.Equals(target.BusyByAgentId, agentState.Id, StringComparison.Ordinal))
                {
                    if (entry.RetryIfBusy)
                    {
                        _scheduleManager.MarkActionSkipped(entry.ActionName, wasBusy: true);
                    }
                    else
                    {
                        _scheduleManager.MarkActionSkipped(entry.ActionName);
                    }
                    continue;
                }
                
                // Устанавливаем цель
                agentState.SetTarget(entry.TargetId);
                
                // Выполняем действие
                var success = action.Execute(agentState, _world, target);
                if (success)
                {
                    _scheduleManager.MarkActionCompleted(entry.ActionName);
                }
                else
                {
                    _scheduleManager.MarkActionSkipped(entry.ActionName);
                }
            }
            else
            {
                // Действие без цели
                var success = action.Execute(agentState, _world, null!);
                if (success)
                {
                    _scheduleManager.MarkActionCompleted(entry.ActionName);
                }
                else
                {
                    _scheduleManager.MarkActionSkipped(entry.ActionName);
                }
            }
        }
        
        return actionsToExecute.Any();
    }
    
    /// <summary>
    /// Выполняет шаг сценария
    /// </summary>
    private bool ExecuteScenarioStep(AgentState agentState)
    {
        return _scenarioManager.ExecuteNextStep(agentState, _world);
    }
    
    /// <summary>
    /// Выполняет шаг GOAP-планировщика
    /// </summary>
    private bool ExecuteGoapStep(AgentState agentState)
    {
        return _goapPlanner.ExecuteStep(agentState, _world);
    }
    
    /// <summary>
    /// Переключается в GOAP-режим
    /// </summary>
    private void SwitchToGoapMode()
    {
        if (!_isInGoapMode)
        {
            _isInGoapMode = true;
            OnModeChanged?.Invoke("GOAP");
        }
    }
    
    /// <summary>
    /// Переключается в режим расписания
    /// </summary>
    private void SwitchToScheduleMode()
    {
        if (_isInGoapMode)
        {
            _isInGoapMode = false;
            OnModeChanged?.Invoke("Schedule");
        }
    }
    
    /// <summary>
    /// Запускает сценарий
    /// </summary>
    public bool StartScenario(string scenarioName, AgentState agentState)
    {
        var success = _scenarioManager.StartScenario(scenarioName, agentState);
        if (success)
        {
            _isInGoapMode = false;
            OnModeChanged?.Invoke("Scenario");
        }
        return success;
    }
    
    /// <summary>
    /// Приостанавливает расписание
    /// </summary>
    public void PauseSchedule()
    {
        _scheduleManager.Pause();
    }
    
    /// <summary>
    /// Возобновляет расписание
    /// </summary>
    public void ResumeSchedule()
    {
        _scheduleManager.Resume();
    }
    
    /// <summary>
    /// Получает текущий режим работы
    /// </summary>
    public string GetCurrentMode()
    {
        if (_isInGoapMode)
        {
            return "GOAP";
        }
        else if (_scenarioManager.IsActive)
        {
            return "Scenario";
        }
        else
        {
            return "Schedule";
        }
    }
    
    /// <summary>
    /// Получает информацию о текущем состоянии
    /// </summary>
    public SchedulerStatus GetStatus()
    {
        return new SchedulerStatus
        {
            Mode = GetCurrentMode(),
            ScheduleActive = _scheduleManager.IsActive,
            ScheduleEntries = _scheduleManager.Count,
            ScenarioActive = _scenarioManager.IsActive,
            CurrentScenario = _scenarioManager.CurrentScenario?.Name,
            CurrentStep = _scenarioManager.CurrentStepIndex,
            GoapActive = _isInGoapMode,
            GoapGoals = _goapPlanner.GetAllGoals().Count()
        };
    }
} 