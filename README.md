# TestReadWrite

1. The example was originally built from the TinyKitchen example as a starting point.
2. A dynamic array of random numbers is created in SpawnAll.cs OnCreate()

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
			
 3. The dynamic array is obtained as a dynamic array in OnUpdate (in BeeBehaviour.cs, BeeAttractRepel.cs, BeeMovement.cs)
 
            var randArray = EntityManager.GetBuffer<RandomElement>(randomEntity).AsNativeArray().AsReadOnly();
            int maxRandomIndex = EntityManager.GetBuffer<RandomElement>(randomEntity).Length;

4. The array is used in Entities.ForEach, but only as ReadOnly. For two of the classes this is ok, but in adding the third one it causes the following error:

Unhandled Exception: System.InvalidOperationException: The previously scheduled job reads from the container or resource. You must call JobHandle.Complete() on the job, before you can write to the container or resource safely.
   at Unity.Entities.ComponentSafetyHandles.CompleteReadAndWriteDependency(Int32 type)
   at Unity.Entities.EntityDataAccess.GetBuffer[T](Entity entity, AtomicSafetyHandle safety, AtomicSafetyHandle arrayInvalidationSafety)
   at Unity.Entities.EntityManager.GetBuffer[T](Entity entity)
   at CombatBees.BeeAttractRepel.OnUpdate()
   at Unity.Entities.SystemBase.Update()
   at Unity.Entities.ComponentSystemGroup.UpdateAllSystems()
   at Unity.Entities.ComponentSystem.Update()
   at Unity.Entities.ComponentSystemGroup.UpdateAllSystems()
   at Unity.Entities.ComponentSystem.Update()
   at Unity.Entities.World.Update()
   at Unity.Tiny.UnityInstance.Update(Double timestampInSeconds)
   at Unity.Tiny.EntryPoint.Program.<>c__DisplayClass0_0.<Main>b__0(Double timestampInSeconds)
   at Unity.Platforms.RunLoopImpl.EnterMainLoop(RunLoopDelegate runLoopDelegate)
   at Unity.Tiny.EntryPoint.Program.Main()
   
5. In order to make the program run just comment out lines 57-59 in BeeBehaviour.cs

                   int nextRandIndex = (startingIndex + entityInQueryIndex) % maxRandomIndex;
                   var nextRand = randArray[nextRandIndex].Value;
                   nextRandIndex = (startingIndex + entityInQueryIndex) % maxRandomIndex;
