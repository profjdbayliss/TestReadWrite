using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace CombatBees
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public class BeeBehaviour : SystemBase
    {
        Random m_Random;

        protected override void OnCreate()
        {
            m_Random = new Random(314159);
            base.OnCreate();
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        protected override void OnUpdate()
        {
            var deltaTime = Time.DeltaTime;

            var teamOneEntity = GetSingletonEntity<TeamOneTranslationArray>();
            var teamOne = EntityManager.GetBuffer<TeamOneTranslationElement>(teamOneEntity).AsNativeArray().AsReadOnly();
            int teamOneLength = SpawnAll.teamOneLength;

            var teamTwoEntity = GetSingletonEntity<TeamTwoTranslationArray>();
            var teamTwo = EntityManager.GetBuffer<TeamTwoTranslationElement>(teamTwoEntity).AsNativeArray().AsReadOnly();
            int teamTwoLength = SpawnAll.teamTwoLength;

            var randomEntity = GetSingletonEntity<RandomArray>();
            var randArray = EntityManager.GetBuffer<RandomElement>(randomEntity).AsNativeArray().AsReadOnly();
            int maxRandomIndex = EntityManager.GetBuffer<RandomElement>(randomEntity).Length;
            int startingIndex = m_Random.NextInt(0, maxRandomIndex);

            var settings = GetSingleton<SettingsSingleton>();
            var aggression = settings.aggression;
            var attackDistance = settings.attackDistance;
            var chaseForce = settings.chaseForce;
            var attackForce = settings.attackForce;
            var hitDistance = settings.hitDistance;

            var job = Entities.ForEach((int entityInQueryIndex, ref BeeBehaviourComponent beeBehaviour, in BeeMoveComponent beeMove, in Translation trans) =>
            {
                if (beeBehaviour.enemyTarget == Entity.Null && beeBehaviour.resourceTarget == Entity.Null)
                {
                    //
                    // comment the following lines out to get the program to run correctly 
                    // without the read/write error
                    //
                    int nextRandIndex = (startingIndex + entityInQueryIndex) % maxRandomIndex;
                    var nextRand = randArray[nextRandIndex].Value;
                    nextRandIndex = (startingIndex + entityInQueryIndex) % maxRandomIndex;

                }
            });

            job.Schedule();
        }
    }

}