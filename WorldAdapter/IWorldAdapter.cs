using SmartCrowd.Core.Entities;

namespace SmartCrowd.Core.WorldAdapter;

public interface IWorldAdapter
{
    IEnumerable<Entity> GetAllEntities();

    Entity? GetEntityById(string id);

    void UpdateEntity(Entity entity);

    bool TryReserveEntity(string entityId, string agentId);

    void ReleaseEntity(string entityId, string agentId);

    // Optional: batch bootstrap to optimize startup data sync
    IEnumerable<Entity> LoadInitialEntitiesBatch(int? maxCount = null);
}

