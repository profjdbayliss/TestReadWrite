using Unity.Entities;
using Unity.Mathematics;

namespace CombatBees
{
    public struct BeeMoveComponent : IComponentData
    {
        public float3 velocity;
        public float3 smoothPosition;
        public short team;
        public bool dead;
    }

    public struct AttractRepelComponent : IComponentData
    {
        public float3 attractivePos;
        public float3 repellantPos;
    }

    public struct BeeScaleComponent : IComponentData
    {
        public float size;
    }
    public struct BeeBehaviourComponent : IComponentData
    {
        public float3 chaseVelocity;
        public Entity enemyTarget;
        public Entity resourceTarget;
        public bool isAttacking;
    }

    public struct TeamOneTag : IComponentData {
        
    }

    public struct TeamTwoTag : IComponentData
    {

    }

    public struct DeadTag : IComponentData
    {

    }

    public struct TeamOneTranslationElement : IBufferElementData
    {
        public float3 Value;
        public Entity entity;
    }

    public struct TeamTwoTranslationElement : IBufferElementData
    {
        public float3 Value;
        public Entity entity;
    }


    public struct TeamOneTranslationArray : IComponentData
    {
    }

    public struct TeamTwoTranslationArray : IComponentData
    {
    }
}

