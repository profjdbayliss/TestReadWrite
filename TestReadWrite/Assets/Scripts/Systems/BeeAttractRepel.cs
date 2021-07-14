using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace CombatBees
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public class BeeAttractRepel : SystemBase
    {
        Random m_Random;
        EntityQuery m_TeamOneBees;
        EntityQuery m_TeamTwoBees;


        protected override void OnCreate()
        {
            m_Random = new Random(314159);
            m_TeamOneBees = GetEntityQuery(ComponentType.ReadOnly<TeamOneTag>(), ComponentType.ReadOnly<Translation>());
            m_TeamTwoBees = GetEntityQuery(ComponentType.ReadOnly<TeamTwoTag>(), ComponentType.ReadOnly<Translation>());
            base.OnCreate();
        }

        protected override void OnDestroy()
        {           
            base.OnDestroy();
        }

        protected override void OnUpdate()
        {

            var teamOneEntity = GetSingletonEntity<TeamOneTranslationArray>();
            //[NativeDisableContainerSafetyRestriction]
            NativeArray<TeamOneTranslationElement>.ReadOnly teamOne = EntityManager.GetBuffer<TeamOneTranslationElement>(teamOneEntity).AsNativeArray().AsReadOnly();
            int teamOneLength = SpawnAll.teamOneLength;

            var teamTwoEntity = GetSingletonEntity<TeamTwoTranslationArray>();
            var teamTwo = EntityManager.GetBuffer<TeamTwoTranslationElement>(teamTwoEntity).AsNativeArray().AsReadOnly();
            int teamTwoLength = SpawnAll.teamTwoLength;

            var randomEntity = GetSingletonEntity<RandomArray>();
            var randArray = EntityManager.GetBuffer<RandomElement>(randomEntity).AsNativeArray().AsReadOnly();
            int maxRandomIndex = EntityManager.GetBuffer<RandomElement>(randomEntity).Length;
            int startingIndex = m_Random.NextInt(0, maxRandomIndex);

            var settings = GetSingleton<SettingsSingleton>();
            var settingsEntity = GetSingletonEntity<ModelSpawnerComponent>();

            var job = Entities.ForEach((int entityInQueryIndex, ref AttractRepelComponent bee, in BeeMoveComponent beeMove) =>
            {
                int nextRandIndex = (startingIndex + entityInQueryIndex) % maxRandomIndex;
                if (beeMove.team == 1)
                {
                    bee.attractivePos = teamOne[(int)math.abs(math.abs(randArray[nextRandIndex].Value) * teamOneLength)].Value;
                    nextRandIndex = (nextRandIndex + 1) % maxRandomIndex;
                    bee.repellantPos = teamOne[(int)math.abs(math.abs(randArray[nextRandIndex].Value) * teamOneLength)].Value;
                }
                else if (beeMove.team == 2)
                {
                    bee.attractivePos = teamTwo[(int)math.abs(math.abs(randArray[nextRandIndex].Value) * teamTwoLength)].Value;
                    nextRandIndex = (nextRandIndex + 1) % maxRandomIndex;
                    bee.repellantPos = teamTwo[(int)math.abs(math.abs(randArray[nextRandIndex].Value) * teamTwoLength)].Value;
                }

            });
            

            job.Schedule();

            

            }
    }
}