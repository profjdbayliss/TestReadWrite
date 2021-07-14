using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace CombatBees
{
    public class BeeScaleSystem : SystemBase
    {

        protected override void OnCreate()
        {
            base.OnCreate();
        }

        protected override void OnUpdate()
        {
            var settings = GetSingleton<SettingsSingleton>();
            float speedStretch = settings.speedStretch;

            Entities.ForEach((BeeMoveComponent beeMove, BeeScaleComponent beeScale, ref NonUniformScale scale) =>
            {
                // now change scale based on velocity
                scale.Value = beeScale.size;
                float velMagnitude = math.sqrt(beeMove.velocity.x * beeMove.velocity.x + beeMove.velocity.y * beeMove.velocity.y + beeMove.velocity.z * beeMove.velocity.z);
                float stretch = math.max(1f, velMagnitude * speedStretch);
                scale.Value.z *= stretch;
                scale.Value.x /= (stretch - 1f) / 5f + 1f;
                scale.Value.y /= (stretch - 1f) / 5f + 1f;
            }).Schedule();
        }
    }
}
