using System.Text.Json.Serialization;
using SmartCrowd.Core.Actions;
using SmartCrowd.Core.Planner;
using SmartCrowd.Core.Scheduler;

namespace SmartCrowd.Core.Entities;

public class AgentState
{
    public string Id { get; set; } = string.Empty; // Should match Entity.Id

    public Dictionary<string, int> Stats { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, int> Inventory { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public HashSet<string> Skills { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    
    // Current target for actions
    public string? CurrentTargetId { get; set; }
    
    // Action registry for this agent
    [JsonIgnore]
    public ActionRegistry Actions { get; } = new();
    
    // GOAP planner for this agent
    [JsonIgnore]
    public AgentPlanner? Planner { get; set; }
    
    // Integrated scheduler for this agent
    [JsonIgnore]
    public IntegratedScheduler? Scheduler { get; set; }

    public AgentState() {}

    public AgentState(string id)
    {
        Id = id;
    }

    public void AddSkill(string skill) => Skills.Add(skill);

    public bool RemoveSkill(string skill) => Skills.Remove(skill);

    public int GetItemCount(string itemId) => Inventory.TryGetValue(itemId, out var qty) ? qty : 0;

    public void AddItem(string itemId, int quantity = 1)
    {
        if (quantity <= 0) return;
        Inventory[itemId] = GetItemCount(itemId) + quantity;
    }

    public bool RemoveItem(string itemId, int quantity = 1)
    {
        if (quantity <= 0) return false;
        var current = GetItemCount(itemId);
        if (current < quantity) return false;
        var remaining = current - quantity;
        if (remaining == 0)
        {
            Inventory.Remove(itemId);
        }
        else
        {
            Inventory[itemId] = remaining;
        }
        return true;
    }

    public int GetStat(string statName) => Stats.TryGetValue(statName, out var value) ? value : 0;

    public void SetStat(string statName, int value) => Stats[statName] = value;

    public int ModifyStat(string statName, int delta)
    {
        var newValue = GetStat(statName) + delta;
        Stats[statName] = newValue;
        return newValue;
    }
    
    public void SetTarget(string? targetId)
    {
        CurrentTargetId = targetId;
    }
    
    public void ClearTarget()
    {
        CurrentTargetId = null;
    }
}

