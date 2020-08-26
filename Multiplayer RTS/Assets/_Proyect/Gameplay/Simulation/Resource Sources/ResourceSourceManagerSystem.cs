using Unity.Burst;
using Unity.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
using System.Runtime.InteropServices;
using UnityEngine;

[DisableAutoCreation]

//
public class ResourceSourceManagerSystem : ComponentSystem
{
    public static Dictionary<Hex, ResourceSourceAndEntity> FoodResourceSources { get; private set; } = new Dictionary<Hex, ResourceSourceAndEntity>();
    public static Dictionary<Hex, ResourceSourceAndEntity> WoodResourceSources { get; private set; } = new Dictionary<Hex, ResourceSourceAndEntity>();
    public static Dictionary<Hex, ResourceSourceAndEntity> GoldResourceSources { get; private set; } = new Dictionary<Hex, ResourceSourceAndEntity>();
    public static Dictionary<Hex, ResourceSourceAndEntity> StoneResourceSources { get; private set; } = new Dictionary<Hex, ResourceSourceAndEntity>();


    EntityQuery m_AllResourceSourceQuery;
    

    protected override void OnCreate()
    {
        m_AllResourceSourceQuery = GetEntityQuery(typeof(ResourceSource));
    }

    protected override void OnUpdate()
    {
        FoodResourceSources.Clear();
        WoodResourceSources.Clear();
        GoldResourceSources.Clear();
        StoneResourceSources.Clear();


        //int count = m_AllResourceSourceQuery.CalculateEntityCount();
        //var allResourcesSources = new Dictionary<Hex, ResourceSource>(count);
        Entities.ForEach((Entity entity, ref ResourceSource res) =>
        {
            Hex resHex = res.position;
            var resType = res.resourceType;
            var resAndEntity = new ResourceSourceAndEntity() { entity = entity, resourceSource = res };

            switch (resType)
            {
                case ResourceType.FOOD:
                    FoodResourceSources.Add(resHex, resAndEntity);
                    break;
                case ResourceType.WOOD:
                    WoodResourceSources.Add(resHex, resAndEntity);
                    break;
                case ResourceType.GOLD:
                    GoldResourceSources.Add(resHex, resAndEntity);
                    break;
                case ResourceType.STONE:
                    StoneResourceSources.Add(resHex, resAndEntity);
                    break;
                default:
                    break;
            }
        });
    }

    public static bool TryGetResourceAtHex(Hex pos, out ResourceSourceAndEntity resource)
    {
        if (GoldResourceSources.TryGetValue(pos, out resource))
        {
            return true;
        }
        else if (StoneResourceSources.TryGetValue(pos, out resource))
        {
            return true;
        }
        else if (WoodResourceSources.TryGetValue(pos, out resource))
        {
            return true;
        }
        else if (FoodResourceSources.TryGetValue(pos, out resource))
        {
            return true;
        }
        else
            return false;
    }

    public static bool TryGetResourceAtHex(Hex pos, ResourceType type, out ResourceSourceAndEntity resource)
    {
        switch (type)
        {
            case ResourceType.FOOD:
                if (FoodResourceSources.TryGetValue(pos, out resource))
                {
                    return true;
                }
                else return false;
            case ResourceType.WOOD:
                if (WoodResourceSources.TryGetValue(pos, out resource))
                {
                    return true;
                }
                else return false;
            case ResourceType.GOLD:
                if (GoldResourceSources.TryGetValue(pos, out resource))
                {
                    return true;
                }
                else return false;
            case ResourceType.STONE:
                if (StoneResourceSources.TryGetValue(pos, out resource))
                {
                    return true;
                }
                else return false;
            default:
                resource = new ResourceSourceAndEntity();
                return false;
        }
    }
    public static Dictionary<Hex, ResourceSourceAndEntity> GetAllConectedResourcesOfType(Hex startingPos, ResourceType type)
    {
        var returnDictionary = new Dictionary<Hex, ResourceSourceAndEntity>();

        var AllResOfType = GetAllResourcesOfType(type);
        

        // esta funcion debe:
        Stack<Hex> openHexes  = new Stack<Hex>();
        List<Hex> closedHexes = new List<Hex>();
        openHexes.Push(startingPos);

        while (openHexes.Count > 0)
        {
            Hex current = openHexes.Pop();

            if (AllResOfType.ContainsKey(current))
            {
                var resSource = AllResOfType[current];
                returnDictionary.Add(current, resSource);

                for (int i = 0; i < 6; i++)
                {
                    var neightbor = current.Neightbor(i);
                    if(! (closedHexes.Contains(neightbor) || openHexes.Contains(neightbor)))
                    {
                        openHexes.Push(neightbor);
                    }
                }
            }
            closedHexes.Add(current);
        }

        return returnDictionary;
    }
    public static Dictionary<Hex, ResourceSourceAndEntity> GetAllResourcesOfType(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.FOOD:
                return FoodResourceSources;
            case ResourceType.WOOD:
                return WoodResourceSources;
            case ResourceType.GOLD:
                return GoldResourceSources;
            case ResourceType.STONE:
                return StoneResourceSources;

            default:
                throw new System.ArgumentException("The resource type is a not defined one!");
                
        }
    }
}