using System.Collections.Concurrent;
using SmartCrowd.Core.Entities;

namespace SmartCrowd.Core.WorldAdapter;

public class InMemoryWorldAdapter : IWorldAdapter
{
    private readonly ConcurrentDictionary<string, Entity> _entities = new(StringComparer.Ordinal);

    public InMemoryWorldAdapter(IEnumerable<Entity>? seed = null)
    {
        if (seed != null)
        {
            foreach (var e in seed)
            {
                _entities[e.Id] = e;
            }
        }
    }

    public IEnumerable<Entity> GetAllEntities() => _entities.Values;

    public Entity? GetEntityById(string id)
    {
        _entities.TryGetValue(id, out var entity);
        return entity;
    }

    public void UpdateEntity(Entity entity)
    {
        _entities[entity.Id] = entity;
    }

    public bool TryReserveEntity(string entityId, string agentId)
    {
        // Atomic reservation using lock per-entity to avoid races
        var entity = GetOrCreateEntity(entityId);
        lock (GetEntityLock(entityId))
        {
            if (entity.IsBusy && !string.Equals(entity.BusyByAgentId, agentId, StringComparison.Ordinal))
            {
                return false;
            }
            entity.TryReserve(agentId);
            return true;
        }
    }

    public void ReleaseEntity(string entityId, string agentId)
    {
        var entity = GetOrCreateEntity(entityId);
        lock (GetEntityLock(entityId))
        {
            entity.TryRelease(agentId);
        }
    }

    public IEnumerable<Entity> LoadInitialEntitiesBatch(int? maxCount = null)
    {
        return maxCount.HasValue ? _entities.Values.Take(maxCount.Value).ToArray() : _entities.Values.ToArray();
    }

    private Entity GetOrCreateEntity(string entityId)
    {
        return _entities.GetOrAdd(entityId, id => new Entity(id));
    }

    private readonly ConcurrentDictionary<string, object> _entityLocks = new(StringComparer.Ordinal);

    private object GetEntityLock(string entityId)
    {
        return _entityLocks.GetOrAdd(entityId, _ => new object());
    }
}

