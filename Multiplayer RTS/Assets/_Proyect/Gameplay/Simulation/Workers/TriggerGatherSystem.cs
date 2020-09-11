using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

/// <summary>
/// este sistema lo que hace es agregar el componente de GroupOnGather si es que el hexagono contienen un recurso.
/// Y a todos sus hijos les agrega el componente GatheringBehaviour
/// </summary>
[DisableAutoCreation]
public class TriggerGatherSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.WithNone<BEResourceSource>().ForEach((Entity entity, ref TriggerGather trigger) =>
        {
            PostUpdateCommands.AddBuffer<BEResourceSource>(entity);
        });

        //agrega elementos al buffer y si no hay elementos. Se quita 
        Entities.WithAll<BEResourceSource>().ForEach((Entity entity, ref TriggerGather trigger) =>
        {
            var buffer = EntityManager.GetBuffer<BEResourceSource>(entity);
            buffer.Clear();
            //necesito conocer donde hay recursos.
            ResourceSourceAndEntity res;

            if (ResourceSourceManagerSystem.TryGetResourceAtHex(trigger.targetResourcePos, out res))
            {
                var type = res.resourceSource.resourceType;
                var allConectedResources = ResourceSourceManagerSystem.GetAllConectedResourcesOfType(trigger.targetResourcePos, type);
                foreach (var keyValue in allConectedResources)
                {
                    buffer.Add((BEResourceSource)keyValue.Value);
                }

                if (EntityManager.HasComponent<GroupOnGather>(entity))
                {
                    PostUpdateCommands.SetComponent<GroupOnGather>(entity, new GroupOnGather() { GatheringResourceType = type });
                }
                else
                {
                    PostUpdateCommands.AddComponent<GroupOnGather>(entity, new GroupOnGather() { GatheringResourceType = type });
                }
            }
            else
            {
                //NO HAY RECURSOS EN EL HEXAGONO.
                //detiene ejecucion de gather.
                //es decir no agrega el componente "GroupOnGather" que trigerrea el resto de componentes del systema de gather.
            }
        });





        //remove the component after use
        Entities.ForEach((Entity entity, ref TriggerGather trigger) =>
        {
            PostUpdateCommands.RemoveComponent<TriggerGather>(entity);
        });
    }
}