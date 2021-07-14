using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

namespace CombatBees
{
    public class ModelConversion : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {
        public List<GameObject> allPrefabs;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            
            Unity.Mathematics.Random m_Random = new Unity.Mathematics.Random(314159);

            dstManager.AddComponentData(entity, new ModelSpawnerComponent()
            {
            });

            // create the converted prefabs for ecs
            var entityPrefabs = dstManager.AddBuffer<ModelComponent>(entity);
            foreach (var onePrefab in allPrefabs)
            {
                entityPrefabs.Add(new ModelComponent
                {
                    Model = conversionSystem.GetPrimaryEntity(onePrefab)
                });
            }

        }

        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.AddRange(allPrefabs);
        }
    }
}