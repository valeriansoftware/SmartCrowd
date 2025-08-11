using SmartCrowd.Core.Entities;

namespace SmartCrowd.Core.Actions;

public class ActionRegistry
{
    private readonly Dictionary<string, GameAction> _actions = new(StringComparer.OrdinalIgnoreCase);
    
    public void Register(GameAction action)
    {
        if (string.IsNullOrWhiteSpace(action.Name))
        {
            throw new ArgumentException("Action name cannot be null or empty", nameof(action));
        }
        
        _actions[action.Name] = action;
    }
    
    public void RegisterRange(IEnumerable<GameAction> actions)
    {
        foreach (var action in actions)
        {
            Register(action);
        }
    }
    
    public GameAction? GetAction(string name)
    {
        _actions.TryGetValue(name, out var action);
        return action;
    }
    
    public IEnumerable<GameAction> GetAllActions() => _actions.Values;
    
    public bool HasAction(string name) => _actions.ContainsKey(name);
    
    public void RemoveAction(string name) => _actions.Remove(name);
    
    public void Clear() => _actions.Clear();
    
    public int Count => _actions.Count;
} 