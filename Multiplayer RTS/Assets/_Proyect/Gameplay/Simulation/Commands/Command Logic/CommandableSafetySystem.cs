using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

[DisableAutoCreation]
//esto no corre despues de todos los sitemas de lockstep o al inicio.
//ya que esto;
//ve toda entidad con solo dethflag y sin commandable y elimina la death flag
//eso asegura que si una entidad commandable muera su ID no sea utilizado por otra entidad
//(ya que los component systems state no se eliminan con la entidad) y los commandos pierdan validad por eso
public class CommandableSafetySystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.WithAll<CommandableDeathFlag>().WithNone<Commandable>().ForEach((Entity entity) => 
        {
            PostUpdateCommands.RemoveComponent<CommandableDeathFlag>(entity);
        });
    }
}