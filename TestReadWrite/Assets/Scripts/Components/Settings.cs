using Unity.Entities;
using Unity.Mathematics;

namespace CombatBees
{
    [GenerateAuthoringComponent]
    public struct SettingsSingleton : IComponentData
    {
        // Bee settings
        public float maxSpawnSpeed;
        public float damping;
        public float aggression;
        public float attackDistance;
        public float chaseForce;
        public float attackForce;
        public float hitDistance;
        public int jitter;
        public int teamAttraction;
        public int teamRepulsion;
        public int rotationStiffness;
        public float speedStretch;
        public float minBeeSize;
        public float maxBeeSize;

        // Global settings
        public short initialResourceCount;
        public short initialBeeCount;
        public short fieldX;
        public short fieldZ;
        public short numberOfRandomNumbers;
    }
}
