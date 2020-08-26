using Boo.Lang;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Unity.Mathematics.math;

[DisableAutoCreation]
public class ResourceSystem : ComponentSystem
{
    private const int NUM_TEAMS = 8;
    /// <summary>
    /// This array contains the resources of all 8 teams.
    /// </summary>
    private static List<ResourcesBank> ResourcesPerTeam = new List<ResourcesBank>(NUM_TEAMS);
    /// <summary>
    /// This is useful because it can be updated at any time not only at the simultaion turns.
    /// This makes this array the best for player input validation.
    /// </summary>
    private static List<ResourcesBank> LocalResourcesPerTeam = new List<ResourcesBank>(NUM_TEAMS);

    public static void InitTeamsResourcesBank(ResourcesBank.StartingResources startingResources)
    {
        for (int i = 0; i < ResourcesPerTeam.Count; i++)
        {
            ResourcesPerTeam[i] = new ResourcesBank(startingResources);
            LocalResourcesPerTeam[i] = new ResourcesBank(startingResources);
        }
    }

    //FUNCIONES QUE ACTUALIZAN Y VALIDAN LA ADICIÓN DE RECURSOS LOCALES. NO LOS VEO NECESARIOS.
    //public static bool UpdateLocalTeamResourcesIfValid(AddResources add)
    //{
    //    if (ValidateResourceAddition(add))
    //    {
    //        var teamBank = LocalResourcesPerTeam[add.team];
    //        teamBank.foodCount += add.food;
    //        teamBank.woodCount += add.wood;
    //        teamBank.goldCount += add.gold;
    //        teamBank.stoneCount += add.stone;
    //        return true;
    //    }
    //    else
    //    {
    //        return false;
    //    }
    //}
    //private static bool ValidateResourceAddition(AddResources add)
    //{
    //    if (add.team >= NUM_TEAMS || add.team < 0)
    //    {
    //        Debug.LogWarning($"You are trying to remove resources for a team that doesn't exist! team: {add.team}");
    //        return false;
    //    }

    //    var resourceBank = LocalResourcesPerTeam[add.team];
    //    return resourceBank.CanAddResources(add);
    //}
    public static bool UpdateLocalTeamResourcesIfValid(RemoveResources remove)
    {
        if(ValidateResourceRemoval(remove))
        {
            var teamBank = LocalResourcesPerTeam[remove.team];
            teamBank.foodCount -= remove.food;
            teamBank.woodCount -= remove.wood;
            teamBank.goldCount -= remove.gold;
            teamBank.stoneCount -= remove.stone;
            return true;
        }
        else 
        {
            return false;
        }     
    }

    private static bool ValidateResourceRemoval(RemoveResources remove)
    {
        if (remove.team >= NUM_TEAMS || remove.team < 0)
        {
            Debug.LogWarning($"You are trying to remove resources for a team that doesn't exist! team: {remove.team}");
            return false;
        }

        var resourceBank = LocalResourcesPerTeam[remove.team];
        return resourceBank.CanRemoveResources(remove);
    }

    protected override void OnUpdate()
    {
        
        //esta info es muy importante para el network.
        //pero no importa mucho para la perspectiva del jugador.
        Entities.ForEach((Entity entity, ref AddResources add) => 
        {
            if (add.team >= NUM_TEAMS || add.team < 0)
            {
                Debug.LogWarning($"You are trying to remove resources for a team that doesn't exist! team: {add.team}");
                return;
            }
            var teamBank = ResourcesPerTeam[add.team];
            teamBank.foodCount += add.food;
            teamBank.woodCount += add.wood;
            teamBank.goldCount += add.gold;
            teamBank.stoneCount += add.stone;
        });

        Entities.ForEach((Entity entity, ref RemoveResources remove) =>
        {
            if (remove.team >= NUM_TEAMS || remove.team < 0)
            {
                Debug.LogWarning($"You are trying to remove resources for a team that doesn't exist! team: {remove.team}");
                return;
            }
            var teamBank = ResourcesPerTeam[remove.team];
            teamBank.foodCount -= remove.food;
            teamBank.woodCount -= remove.wood;
            teamBank.goldCount -= remove.gold;
            teamBank.stoneCount -= remove.stone;
        });

        LocalResourcesPerTeam = new List<ResourcesBank>(ResourcesPerTeam);
    }
}