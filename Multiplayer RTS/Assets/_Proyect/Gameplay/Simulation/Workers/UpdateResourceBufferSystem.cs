using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;


/// <summary>
/// This system ckecks for all the entities with a BEResourceSource and group on gather and updates its buffer.
/// If there is no valid resource it removes theGroupOnGather Comp and the GatheringBehaviour comp of the gatherers.
/// </summary>
[DisableAutoCreation]
public class UpdateResourceBufferSystem : ComponentSystem
{
    protected override void OnUpdate()
    {

        //actualiza el buffer y ve si aun hay recursos disponibles. y luego elimina el componente que update buffer.
        /*
         para lograrlo lo que hace es que actualiza todas las frames el buffer.
         */
        Entities.WithAll<BEResourceSource, UpdateResourceBuffer>().ForEach((Entity entity, ref GroupOnGather onGather) => 
        {
            var buffer = EntityManager.GetBuffer<BEResourceSource>(entity);

            bool haveValidSource = false;
            Hex startinghex = new Hex(0,0);

            for (int i = 0; i < buffer.Length; i++)
            {
                var resSourceData = buffer[i];
                if (ResourceSourceManagerSystem.TryGetResourceAtHex(resSourceData.position, onGather.GatheringResourceType, out ResourceSourceAndEntity res)) 
                {
                    haveValidSource = true;
                    startinghex = resSourceData.position;
                    break;
                }
            }

            buffer.Clear();

            if (haveValidSource)
            {
                var allConectedResources = ResourceSourceManagerSystem.GetAllConectedResourcesOfType(startinghex, onGather.GatheringResourceType);
                foreach (var keyValue in allConectedResources)
                {
                    buffer.Add((BEResourceSource)keyValue.Value);
                }
            }
            else 
            {
                PostUpdateCommands.RemoveComponent<GroupOnGather>(entity);


            }



            PostUpdateCommands.RemoveComponent<UpdateResourceBuffer>(entity);
        });
    }
}