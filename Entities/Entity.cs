using System.Collections.Concurrent;
using System.Text.Json.Serialization;

namespace SmartCrowd.Core.Entities;

public class Entity : IJsonOnDeserialized
{
    private bool _isAgent;

    public string Id { get; set; } = Guid.NewGuid().ToString();

    public HashSet<string> Tags { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    // Props can hold arbitrary values; during JSON deserialization values may materialize as JsonElement
    public Dictionary<string, object?> Props { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public bool IsAgent
    {
        get => _isAgent;
        set
        {
            _isAgent = value;
            if (_isAgent)
            {
                Tags.Add("agent");
            }
            else
            {
                Tags.Remove("agent");
            }
        }
    }

    public bool IsBusy { get; private set; }

    public string? BusyByAgentId { get; private set; }

    public Entity() { }

    public Entity(string id)
    {
        Id = id;
    }

    public bool HasTag(string tag) => Tags.Contains(tag);

    public void AddTag(string tag) => Tags.Add(tag);

    public void RemoveTag(string tag) => Tags.Remove(tag);

    public bool TryReserve(string agentId)
    {
        if (string.IsNullOrWhiteSpace(agentId))
        {
            return false;
        }

        if (IsBusy && !string.Equals(BusyByAgentId, agentId, StringComparison.Ordinal))
        {
            return false;
        }

        IsBusy = true;
        BusyByAgentId = agentId;
        return true;
    }

    public bool TryRelease(string agentId)
    {
        if (!IsBusy)
        {
            return true;
        }

        if (!string.Equals(BusyByAgentId, agentId, StringComparison.Ordinal))
        {
            return false;
        }

        IsBusy = false;
        BusyByAgentId = null;
        return true;
    }

    public void OnDeserialized()
    {
        if (IsAgent)
        {
            Tags.Add("agent");
        }
    }
}

