using Unity.Entities;

namespace CombatBees
{
    [GenerateAuthoringComponent]
    public struct ModelComponent : IBufferElementData
    {
        public Entity Model;
    }
}
