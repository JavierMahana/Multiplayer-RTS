using System.Collections.Generic;
using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

/// <summary>
/// sistema encargado de almacenar todos los silos de almacenamiento de recursos por equipo.
/// por ahora solo son estructuras (esto es objeto de cambio cuando se incorporen las estructuras que no utilicen todo un hexagono).
/// </summary>
[DisableAutoCreation]
public class DropPointSystem : ComponentSystem
{
    //deben ser por equipos
    public static Dictionary<Hex, Entity>[] FoodDropPointsPerTeam { get; private set; } = new Dictionary<Hex, Entity>[8]
    {
        new Dictionary<Hex, Entity>(),
        new Dictionary<Hex, Entity>(),
        new Dictionary<Hex, Entity>(),
        new Dictionary<Hex, Entity>(),
        new Dictionary<Hex, Entity>(),
        new Dictionary<Hex, Entity>(),
        new Dictionary<Hex, Entity>(),
        new Dictionary<Hex, Entity>(),
    };
    public static Dictionary<Hex, Entity>[] WoodDropPointsPerTeam { get; private set; } = new Dictionary<Hex, Entity>[8]
    {
        new Dictionary<Hex, Entity>(),
        new Dictionary<Hex, Entity>(),
        new Dictionary<Hex, Entity>(),
        new Dictionary<Hex, Entity>(),
        new Dictionary<Hex, Entity>(),
        new Dictionary<Hex, Entity>(),
        new Dictionary<Hex, Entity>(),
        new Dictionary<Hex, Entity>(),
    };
    public static Dictionary<Hex, Entity>[] GoldDropPointsPerTeam { get; private set; } = new Dictionary<Hex, Entity>[8]
    {
        new Dictionary<Hex, Entity>(),
        new Dictionary<Hex, Entity>(),
        new Dictionary<Hex, Entity>(),
        new Dictionary<Hex, Entity>(),
        new Dictionary<Hex, Entity>(),
        new Dictionary<Hex, Entity>(),
        new Dictionary<Hex, Entity>(),
        new Dictionary<Hex, Entity>(),
    };
    public static Dictionary<Hex, Entity>[] StoneDropPointsPerTeam { get; private set; } = new Dictionary<Hex, Entity>[8]
    {
        new Dictionary<Hex, Entity>(),
        new Dictionary<Hex, Entity>(),
        new Dictionary<Hex, Entity>(),
        new Dictionary<Hex, Entity>(),
        new Dictionary<Hex, Entity>(),
        new Dictionary<Hex, Entity>(),
        new Dictionary<Hex, Entity>(),
        new Dictionary<Hex, Entity>(),
    };

    public static Dictionary<Hex, Entity> GetAllDropPointsOfTeam(ResourceType type, int team)
    {
        team %= 8;
        switch (type)
        {
            case ResourceType.FOOD:
                return FoodDropPointsPerTeam[team];
            case ResourceType.WOOD:
                return WoodDropPointsPerTeam[team]; 
            case ResourceType.GOLD:
                return GoldDropPointsPerTeam[team];
            case ResourceType.STONE:
                return StoneDropPointsPerTeam[team];
            default:
                UnityEngine.Debug.LogError("Not valid resource type!");
                return null;                
        }
    }

    private static void ClearAllDropPointsColections()
    {
        for (int i = 0; i < 8; i++)
        {
            FoodDropPointsPerTeam[i].Clear();
            WoodDropPointsPerTeam[i].Clear();
            GoldDropPointsPerTeam[i].Clear();
            StoneDropPointsPerTeam[i].Clear();
        }
    }

    protected override void OnUpdate()
    {
        ClearAllDropPointsColections();

        Entities.ForEach((Entity entity, ref Team team, ref ResourceDropPoint dropPoint, ref Building building) => 
        {
            int teamNum = team.Number;
            teamNum %= 8;

            if (dropPoint.CanDropFood)
            {
                FoodDropPointsPerTeam[teamNum].Add(building.position, entity);
            }
            if (dropPoint.CanDropWood)
            {
                WoodDropPointsPerTeam[teamNum].Add(building.position, entity);
            }
            if (dropPoint.CanDropGold)
            {
                GoldDropPointsPerTeam[teamNum].Add(building.position, entity);
            }
            if (dropPoint.CanDropStone)
            {
                StoneDropPointsPerTeam[teamNum].Add(building.position, entity);
            }
        });
    }
}