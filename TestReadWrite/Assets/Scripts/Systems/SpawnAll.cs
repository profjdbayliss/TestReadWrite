using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Tiny.Rendering;
using Unity.Collections;

namespace CombatBees
{
    ///<summary>
    /// Instantiate all main items
    ///</summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class SpawnAll : SystemBase
    {
        private Random m_Random;
        private static int init = 0;
        private static float oneThirdFieldX;
        private static float oneThirdFieldZ;

        private EntityQuery m_TeamOneBees;
        private EntityQuery m_TeamTwoBees;

        public static NativeArray<Translation>.ReadOnly teamOne;
        public static NativeArray<Translation>.ReadOnly teamTwo;
        public static int teamOneLength;
        public static int teamTwoLength;
        

        protected override void OnCreate()
        {
            RequireSingletonForUpdate<ModelSpawnerComponent>();
            m_Random = new Random(314159);

            // create random array from -1 to 1
            Entity rand = EntityManager.CreateEntity(typeof(RandomArray));
            var randArray = EntityManager.AddBuffer<RandomElement>(rand);
            int numberOfRandoms = 100;
            for (int i = 0; i < numberOfRandoms; i++)
            {
                randArray.Add(new RandomElement
                {
                    Value = m_Random.NextFloat(-1, 1)
                });
            }

            // create dynamic array of translations for teams of bees
            Entity trans = EntityManager.CreateEntity(typeof(TeamOneTranslationArray));
            var transArray = EntityManager.AddBuffer<TeamOneTranslationElement>(trans);
            trans = EntityManager.CreateEntity(typeof(TeamTwoTranslationArray));
            var trans2Array = EntityManager.AddBuffer<TeamTwoTranslationElement>(trans);

            // queries for groups needed in sim updates
            m_TeamOneBees = GetEntityQuery(ComponentType.ReadOnly<TeamOneTag>(), ComponentType.ReadOnly<Translation>());
            m_TeamTwoBees = GetEntityQuery(ComponentType.ReadOnly<TeamTwoTag>(), ComponentType.ReadOnly<Translation>());

            base.OnCreate();
        }

        protected override void OnStartRunning()
        {
            if (init == 0)
            {
                init = 1;
                var settings = GetSingleton<SettingsSingleton>();
                var cmdBuffer = new EntityCommandBuffer(Allocator.TempJob);
                int sizeX = settings.fieldX;
                int sizeZ = settings.fieldZ;
                var settingsEntity = GetSingletonEntity<ModelSpawnerComponent>();
                var allPrefabs = EntityManager.GetBuffer<ModelComponent>(settingsEntity);

                // Check if the list of prefab is empty
                if (allPrefabs.Length == 0)
                    throw new System.Exception("No prefabs to instantiate");

                // create resources
                if (settings.initialResourceCount > 0)
                {

                    Entity prefab;
                    Entity prefabEntityType = allPrefabs[0].Model;
                    oneThirdFieldX = settings.fieldX * 2 / 3.0f;
                    oneThirdFieldZ = settings.fieldZ * 2 / 3.0f;

                    for (int i = 0; i < settings.initialResourceCount; i++)
                    {

                        prefab = cmdBuffer.Instantiate(prefabEntityType);

                        int posX = (int)(m_Random.NextFloat(oneThirdFieldX) - sizeX / 3.0f);
                        int posZ = (int)(m_Random.NextFloat(oneThirdFieldZ) - sizeZ / 3.0f);
                        cmdBuffer.SetComponent(prefab, new Translation()
                        {
                            Value = new float3(posX - 0.5f, 1, posZ - 0.5f),

                        });
                        cmdBuffer.AddComponent<ResourceComponent>(prefab);


                    }
                }

                // create both teams of bees
                if (settings.initialBeeCount > 0)
                {
                    Entity bee;
                    Entity beeEntityType = allPrefabs[1].Model;
                    float spawnSpeed = settings.maxSpawnSpeed;

                    for (int i = 0; i < settings.initialBeeCount; i++)
                    {
                        bee = cmdBuffer.Instantiate(beeEntityType);
                        float size = m_Random.NextFloat(settings.minBeeSize, settings.maxBeeSize);
                        int posX = (int)(m_Random.NextFloat(sizeX * 2) - sizeX);
                        int posY = (int)(m_Random.NextFloat(5) + 1);
                        int posZ = (int)(m_Random.NextFloat(sizeZ * 2) - sizeZ);
                        Translation trans = new Translation
                        {
                            Value = new float3(posX - 0.5f, posY, posZ - 0.5f),

                        };
                        cmdBuffer.SetComponent(bee, trans);
                        float3 vel = m_Random.NextFloat3Direction() * spawnSpeed;
                        float3 smoothPosition = new float3(posX - 0.5f + 1, posY + 0, posZ - 0.5f + 0) * 0.01f;

                        // set attract and repel to self as it will change as soon as the attract/repel system changes
                        cmdBuffer.AddComponent<BeeScaleComponent>(bee, new BeeScaleComponent { size = size });
                        cmdBuffer.AddComponent<NonUniformScale>(bee, new NonUniformScale { Value = size });
                        cmdBuffer.AddComponent<BeeBehaviourComponent>(bee, new BeeBehaviourComponent
                        {
                            chaseVelocity = 0,
                            enemyTarget = Entity.Null,
                            resourceTarget = Entity.Null,
                            isAttacking = false,

                        });
                        cmdBuffer.AddComponent<AttractRepelComponent>(bee, new AttractRepelComponent
                        {
                            attractivePos = trans.Value,
                            repellantPos = trans.Value,
                        });

                        // set up the teams
                        if (m_Random.NextInt(0, 2) >= 1)
                        {
                            cmdBuffer.AddComponent<TeamOneTag>(bee, new TeamOneTag { });
                            cmdBuffer.AddComponent<BeeMoveComponent>(bee, new BeeMoveComponent
                            {
                                velocity = vel,
                                smoothPosition = smoothPosition,
                                team = 1,
                                dead = false,
                            });

                        }
                        else
                        {
                            cmdBuffer.AddComponent<TeamTwoTag>(bee, new TeamTwoTag { });
                            cmdBuffer.AddComponent<BeeMoveComponent>(bee, new BeeMoveComponent
                            {
                                velocity = vel,
                                smoothPosition = smoothPosition,
                                team = 2,
                                dead = false,
                            });
                        }
                    }
                }

                cmdBuffer.Playback(EntityManager);
                cmdBuffer.Dispose();
            }

        }

        protected override void OnDestroy()
        {
            
            base.OnDestroy();
        }


        protected override void OnUpdate()
        {

            // make sure to update all arrays of bee translations every frame

            teamOne = m_TeamOneBees.ToComponentDataArray<Translation>(Allocator.Temp).AsReadOnly();
            var teamOneEntities = m_TeamOneBees.ToEntityArray(Allocator.Temp).AsReadOnly();
            teamTwo = m_TeamTwoBees.ToComponentDataArray<Translation>(Allocator.Temp).AsReadOnly();
            var teamTwoEntities = m_TeamOneBees.ToEntityArray(Allocator.Temp).AsReadOnly();
            teamOneLength = m_TeamOneBees.CalculateEntityCount();
            teamTwoLength = m_TeamTwoBees.CalculateEntityCount();

            var teamOneEntity = GetSingletonEntity<TeamOneTranslationArray>();
            var teamTwoEntity = GetSingletonEntity<TeamTwoTranslationArray>();
            var bufferOne = EntityManager.GetBuffer<TeamOneTranslationElement>(teamOneEntity);
            var bufferTwo = EntityManager.GetBuffer<TeamTwoTranslationElement>(teamTwoEntity);

            // copy team one info
            if (bufferOne.Length < teamOneLength)
            {
                bufferOne.Clear();
                for (int i = 0; i < teamOneLength; i++)
                {
                    bufferOne.Add(new TeamOneTranslationElement { Value = teamOne[i].Value, entity = teamOneEntities[i] });
                }
            } else
            {
                // we can just replace what's already there
                for (int i = 0; i < teamOneLength; i++)
                {
                    bufferOne[i] = new TeamOneTranslationElement { Value = teamOne[i].Value, entity = teamOneEntities[i] };
                }
            }

            // copy team two info
            if (bufferTwo.Length < teamTwoLength)
            {
                bufferTwo.Clear();
                for (int i = 0; i < teamTwoLength; i++)
                {
                    bufferTwo.Add(new TeamTwoTranslationElement { Value = teamTwo[i].Value, entity = teamTwoEntities[i] });
                }
            }
            else
            {
                for (int i = 0; i < teamTwoLength; i++)
                {
                    bufferTwo[i] = new TeamTwoTranslationElement { Value = teamTwo[i].Value, entity = teamTwoEntities[i] };
                }
            }
        }
            
            
        
    }
}