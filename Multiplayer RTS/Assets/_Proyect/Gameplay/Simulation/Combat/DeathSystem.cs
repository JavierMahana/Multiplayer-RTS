using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
using System.Collections.Generic;

[DisableAutoCreation]
public class DeathSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity entity, ref Health health) => 
        {
            if (health.CurrentHealth <= 0)
            {
                PostUpdateCommands.DestroyEntity(entity);
            }
        });



        #region group death
        var groupsWithChild = new List<Entity>();
        Entities.ForEach((Parent parent) => 
        {
            if (!groupsWithChild.Contains(parent.ParentEntity))
            {
                groupsWithChild.Add(parent.ParentEntity);
            }
        });
        Entities.WithAll<Group>().ForEach((Entity entity) => 
        {
            if (!groupsWithChild.Contains(entity))
            {
                PostUpdateCommands.DestroyEntity(entity);
            }
        });
        #endregion
    }
}