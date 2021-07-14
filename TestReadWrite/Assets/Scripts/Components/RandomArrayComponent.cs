using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace CombatBees
{
    public struct RandomElement : IBufferElementData
    {
        public float Value;
    }

    public struct RandomArray : IComponentData
    {
    }
}