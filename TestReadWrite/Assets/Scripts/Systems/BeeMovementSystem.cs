using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace CombatBees
{
    [UpdateInGroup(typeof(TransformSystemGroup))]
    public class BeeMovementSystem : SystemBase
    {
        Random m_Random;
        EntityQuery m_randArray;

        protected override void OnCreate()
        {
            m_Random = new Random(314159);
            m_randArray = GetEntityQuery(ComponentType.ReadOnly < RandomElement>());
            base.OnCreate();
        }


        protected override void OnUpdate()
        {
            float deltaTime = Time.DeltaTime;
            var settings = GetSingleton<SettingsSingleton>();
            var settingsEntity = GetSingletonEntity<ModelSpawnerComponent>();
            
            int sizeX = settings.fieldX;
            int sizeZ = settings.fieldZ;
            int sizeY = 30;
            int rotationStiffness = settings.rotationStiffness;
            int flightJitter = settings.jitter;

            float damping = settings.damping;
            float aggression = settings.aggression;
            int teamAttraction = settings.teamAttraction;
            int teamRepulsion = settings.teamRepulsion;

            var randomEntity = GetSingletonEntity<RandomArray>();
            var randArray = EntityManager.GetBuffer<RandomElement>(randomEntity).AsNativeArray().AsReadOnly();
            int maxRandomIndex = EntityManager.GetBuffer<RandomElement>(randomEntity).Length;
            int startingIndex = m_Random.NextInt(0, maxRandomIndex);

            var job = Entities.ForEach((int entityInQueryIndex, ref Translation translation, ref BeeMoveComponent bee, ref Rotation rotation, in AttractRepelComponent attractRepel, in BeeBehaviourComponent beeBehaviour) =>
                {
                    float3 vel = bee.velocity;
                    int nextRandIndex = (startingIndex + entityInQueryIndex) % maxRandomIndex;
                    float randX = randArray[nextRandIndex].Value;
                    nextRandIndex = (nextRandIndex + 1) % maxRandomIndex;
                    float randY = randArray[nextRandIndex].Value;
                    nextRandIndex = (nextRandIndex + 1) % maxRandomIndex;
                    float randZ = randArray[nextRandIndex].Value;
                    float3 direction = new float3(randX, randY, randZ);

                    vel += direction * (flightJitter * deltaTime);
                    vel *= (1f - damping);

                    nextRandIndex = (nextRandIndex + 1) % maxRandomIndex;                  

                    float3 delta = attractRepel.attractivePos - translation.Value;
                    float dist = math.sqrt(delta.x * delta.x + delta.y * delta.y + delta.z * delta.z);
                    if (dist > 0f)
                    {
                        vel += delta * (teamAttraction * deltaTime / dist);
                    }

                    
                    delta = attractRepel.repellantPos - translation.Value;
                    dist = math.sqrt(delta.x * delta.x + delta.y * delta.y + delta.z * delta.z);
                    if (dist > 0f)
                    {
                        vel -= delta * (teamRepulsion * deltaTime / dist);
                    }


                    // set up bee direction and make sure it's not off the playfield
                    float3 movement = deltaTime * vel;
                    translation.Value = movement + translation.Value;

                    if (math.abs(translation.Value.x) > sizeX)
                    {
                        translation.Value.x = sizeX * math.sign(translation.Value.x);
                        vel.x *= -.5f;
                        vel.y *= .8f;
                        vel.z *= .8f;
                    }
                    if (System.Math.Abs(translation.Value.z) > sizeZ)
                    {
                        translation.Value.z = sizeZ * math.sign(translation.Value.z);
                        vel.z *= -.5f;
                        vel.x *= .8f;
                        vel.y *= .8f;
                    }

                        // the following keeps bees carrying resources from slamming them outside the playfield
                        // WORK: need to add this back in
                        //float resourceModifier = 0f;
                        //if (bee.isHoldingResource)
                        //{
                        //	resourceModifier = ResourceManager.instance.resourceSize;
                        //}
                        if (translation.Value.y < 1)
                    {
                        translation.Value.y = 1;
                        vel.y *= -.5f;
                        vel.z *= .8f;
                        vel.x *= .8f;
                    }
                    else if (translation.Value.y > sizeY)
                    {
                        translation.Value.y = sizeY * math.sign(translation.Value.y);
                        vel.y *= -.5f;
                        vel.z *= .8f;
                        vel.x *= .8f;
                    }

                    // WORK: make the following assignment able to be fully parallelized
                    bee.velocity = vel + beeBehaviour.chaseVelocity;

                   // do smoothing for rotation
                   float3 oldSmoothPos = bee.smoothPosition;
                   bee.smoothPosition = math.lerp(bee.smoothPosition, translation.Value, deltaTime * rotationStiffness);

                    float3 smoothDirection = bee.smoothPosition - oldSmoothPos;
                    if (math.all(smoothDirection != float3.zero))
                    {
                        rotation.Value = quaternion.LookRotation(smoothDirection, new float3(0, 1, 0));
                    }


                });
            job.Schedule();

        }
    }

}