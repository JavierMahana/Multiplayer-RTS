using FixMath.NET;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Unity.Mathematics.math;

[DisableAutoCreation]
public class CollisionSystem : ComponentSystem
{
    private const int OTHER_TEAM_COLLISION_WEIGHT = 5;

    //when this change modify "GetCollidersIndicesThatCanCollide" function
    private static readonly Dictionary<ColliderLayer, ColliderFlags> layerCollisionMatrix = new Dictionary<ColliderLayer, ColliderFlags>()
    {
        {ColliderLayer.UNIT, ColliderFlags.UNITS },
        {ColliderLayer.GROUP, ColliderFlags.NONE }
    };


    private static EntityQuery m_ColliderEntityQuery;
    protected override void OnCreate()
    {
        m_ColliderEntityQuery = GetEntityQuery(typeof(HexPosition), typeof(Collider), typeof(Team));
    }

    
    protected override void OnUpdate()
    {
        var activeMap = MapManager.ActiveMap;
        if (activeMap == null)
        { Debug.LogError("Active map needed for the collision system"); return; }

        int colliderCount = m_ColliderEntityQuery.CalculateEntityCount();

        var positions        = new FractionalHex[colliderCount];
        var collidersRadious = new Fix64[colliderCount];        
        var collidersLayers  = new ColliderLayer[colliderCount];
        var teams            = new int[colliderCount];

        //init the collections
        int collectionIndex = 0;
        Entities.ForEach((ref HexPosition hexPosition, ref Collider collider, ref Team team) =>
        {
            var position = hexPosition.HexCoordinates;

            positions[collectionIndex] = position;
            collidersRadious[collectionIndex] = collider.Radious;           
            collidersLayers[collectionIndex] = collider.Layer;
            teams[collectionIndex] = team.Number;

            collectionIndex++;
        });

        var pointsSorted = SpartialSortUtils.GetPointsSpartiallySorted(positions, 1);


        //collide
        Entities.ForEach((ref HexPosition hexPosition, ref Collider collider, ref Team team) => 
        {
            FractionalHex position = hexPosition.HexCoordinates; 


            if (CollisionTestAgainstMap(collider.Radious, position, activeMap, out List<int> collidingDirections))
            {
                bool fullCollision = collidingDirections.Count == 6;
                FractionalHex collisionResolutionDir;

                if (fullCollision)
                {
                    FractionalHex closestOpenHex = (FractionalHex)RuntimeMap.FindClosestOpenHex(position, activeMap);
                    collisionResolutionDir = (closestOpenHex - position).NormalizedManhathan();                    
                }
                else
                {
                    var directionSum = Hex.Zero; 
                    foreach (int directionIndex in collidingDirections)
                    {
                        directionSum += -(Hex.directions[directionIndex]);
                    }
                    collisionResolutionDir = directionSum.NormalizedManhathan();
                }
                hexPosition.HexCoordinates = position + (collisionResolutionDir * collider.Radious * collider.CollisionPushIntensity);
            }            

            else //collide with other colliders
            {
                int thisIndex;
                var pointsToCheck = SpartialSortUtils.GetFilteredPoints(position, pointsSorted, out thisIndex);
                var collidableIndices = GetCollidableCollidersIndices(collider, pointsToCheck, collidersLayers);

                //SimpleCollisionCheckAndResponse(ref hexPosition, collider.Radious,  positions, collidersRadious, teams, thisIndex, collidableIndices);
                //vamos a intentar una collision de dos pasos.
                DoubleStepCollision(ref hexPosition, collider.Radious, team, positions, collidersRadious, teams, thisIndex, collidableIndices);
            }
        });
    }
    private static void ComplexCollisionCheckAndResponse(ref HexPosition hexPosition, Fix64 radius, FractionalHex[] positions, Fix64[] collidersRadious, int[] teams, int thisIndex, List<int> collidableIndices, int maxIterations)
    {
        //demaciado propensa a generar loops infinitos

        bool moreChecksAreNeeded = true;
        int interations = 0;
        while (moreChecksAreNeeded)
        {
            moreChecksAreNeeded = false;

            foreach (var collidableIndex in collidableIndices)
            {
                var colliderPosition = positions[collidableIndex];
                var colliderRadius = collidersRadious[collidableIndex];
                var colliderTeam = teams[collidableIndex];

                var minimumCollisionFreeDistance = colliderRadius + radius;
                var distance = hexPosition.HexCoordinates.Distance(colliderPosition);
                if (distance < minimumCollisionFreeDistance)
                {
                    if (CrossoverCollision(hexPosition, colliderPosition))
                    {
                        var movementMagnitudeRequired = minimumCollisionFreeDistance - distance;
                        var direction = (hexPosition.HexCoordinates - colliderPosition).NormalizedManhathan();
                        hexPosition.HexCoordinates = hexPosition.HexCoordinates + ((direction * movementMagnitudeRequired));
                    }
                    else 
                    {
                        //moverse a la direccion de la posicion anterior. La cantidad que se atraviesa
                        var movementMagnitudeRequired = minimumCollisionFreeDistance - distance;
                        var direction = (hexPosition.PrevPosition - hexPosition.HexCoordinates).NormalizedManhathan();
                        hexPosition.HexCoordinates = hexPosition.HexCoordinates + ((direction * movementMagnitudeRequired));
                    }

                    positions[thisIndex] = hexPosition.HexCoordinates;
                    moreChecksAreNeeded = true;
                    break;
                }
            }
            interations++;
            if (interations >= maxIterations)
            {
                //Debug.LogWarning("infinite loop");
                break;
            }
        }
    }
    private static void SimpleCollisionCheckAndResponse(ref HexPosition hexPosition, Fix64 radius, FractionalHex[] positions, Fix64[] collidersRadious, int[] teams, int thisIndex, List<int> collidableIndices, bool mutualRepulsion = false)
    {
        Fix64 mutiplier = mutualRepulsion ? (Fix64)0.7 : Fix64.One;
        var position = hexPosition.HexCoordinates;

        FractionalHex collisionMovementRequired = FractionalHex.Zero;
        int amountOfCollisons = 0;
        foreach (var collidableIndex in collidableIndices)
        {
            var colliderPosition = positions[collidableIndex];
            var colliderRadius = collidersRadious[collidableIndex];
            var colliderTeam = teams[collidableIndex];

            var minimumCollisionFreeDistance = colliderRadius + radius;
            var distance = position.Distance(colliderPosition);
            if (distance < minimumCollisionFreeDistance)
            {
                var movementMagnitudeRequired = minimumCollisionFreeDistance - distance;
                var direction = (position - colliderPosition).NormalizedManhathan();

                collisionMovementRequired += ((direction * movementMagnitudeRequired * mutiplier));//* (Fix64)0.7
                amountOfCollisons++;

            }
        }
        hexPosition.HexCoordinates = position + collisionMovementRequired / Fix64.Max((Fix64)amountOfCollisons, Fix64.One);

        positions[thisIndex] = hexPosition.HexCoordinates;
    }

    private static void DoubleStepCollision(ref HexPosition hexPosition, Fix64 radius, Team team, FractionalHex[] positions, Fix64[] collidersRadious, int[] teams, int thisIndex, List<int> collidableIndices)
    {
        var sameTeamIndices = new List<int>();
        var otherTeamIndices = new List<int>();
        foreach (int index in collidableIndices)
        {            
            var currTeam = teams[index];
            if (currTeam == team.Number)
            {
                sameTeamIndices.Add(index);
            }
            else
            {
                otherTeamIndices.Add(index);
            }
        }

        SimpleCollisionCheckAndResponse(ref hexPosition, radius, positions, collidersRadious, teams, thisIndex, sameTeamIndices, true);
        ComplexCollisionCheckAndResponse(ref hexPosition, radius, positions, collidersRadious, teams, thisIndex, otherTeamIndices, 6);
        //SimpleCollisionCheckAndResponse(ref hexPosition, radius, positions, collidersRadious, teams, thisIndex, otherTeamIndices, false);
    }

    private static bool CrossoverCollision(HexPosition hexPosition, FractionalHex collisionPoint)
    {
        var prevPos = hexPosition.PrevPosition;
        var pos = hexPosition.HexCoordinates;

        var distMovement = prevPos.Distance(pos);
        var distCollider = prevPos.Distance(collisionPoint);

        return distMovement > distCollider;
    }

    /// <summary>
    /// Gets the colliders that their layers could collide with the layers of the collider
    /// </summary>   
    private static List<int> GetCollidableCollidersIndices(Collider current, List<SortPoint> pointsToCheck, ColliderLayer[] allColliderLayersArray)
    {
        var result = new List<int>();
        ColliderFlags currentLayerFlags;
        if (!layerCollisionMatrix.TryGetValue(current.Layer, out currentLayerFlags))
        {
            Debug.LogError($"You must update the collision layer matrix to include the key: {current.Layer}");
            return result;
        }

        foreach (var point in pointsToCheck)
        {
            int pointIndex = point.index;
            var colliderLayer = allColliderLayersArray[pointIndex];

            bool colliderCanCauseCollition = false;
            if (currentLayerFlags.HasFlag(ColliderFlags.UNITS))
            {
                if (colliderLayer == ColliderLayer.UNIT) colliderCanCauseCollition = true;
            }
            else if (currentLayerFlags.HasFlag(ColliderFlags.GROUPS))
            {
                if (colliderLayer == ColliderLayer.UNIT) colliderCanCauseCollition = true;
            }

            if (colliderCanCauseCollition)
            {
                result.Add(pointIndex);
            }
        }
        return result;
    }


    /// <summary>
    /// It returns true if there is a collision with the map, false otherwise. The 'out' list comntains the most important corners that are colliding. priority order
    /// </summary>
    private static bool CollisionTestAgainstMap(Fix64 colliderArea, FractionalHex position, ActiveMap activeMap, out List<int> collidingDirections)
    {
        collidingDirections = new List<int>();

        bool outsideMapCollision = false;
        bool unwalkableHexCollision = false;
        var directionsToOutsideMapCollitions = new List<int>(); 
        var directionsToUnwalkableHexCollisions = new List<int>();
        for (int i = 0; i < 6; i++)
        {
            var direction = Hex.directions[i];
            var corner = position + ((FractionalHex)direction * colliderArea);
            if (activeMap.map.DinamicMapValues.TryGetValue(corner.Round(), out bool walkable))
            {
                if (!walkable)
                {
                    directionsToUnwalkableHexCollisions.Add(i);
                    unwalkableHexCollision = true;
                }                
            }
            else 
            {
                directionsToOutsideMapCollitions.Add(i);
                outsideMapCollision = true;
            }
        }

        if (outsideMapCollision)
        {
            collidingDirections = directionsToOutsideMapCollitions;
            return true;
        }
        else if (unwalkableHexCollision)
        {
            collidingDirections = directionsToUnwalkableHexCollisions;
            return true;
        }
        else 
        {
            return false;
        }
    }
    private static FractionalHex TrueCollisionResponse(HexPosition hexPosition, Fix64 radius, FractionalHex collisionBodyPos, Fix64 collisionBodyRadius)
    {
        var prevPos = hexPosition.PrevPosition;
        var direction = (collisionBodyPos - prevPos).NormalizedManhathan();
        var distance = prevPos.Distance(collisionBodyPos);
        return prevPos + direction * (Fix64)0.2 *(distance - (radius + collisionBodyRadius));
    }
    /// <summary>
    /// A point cast that returns true if it hits a collider, false otherwise. Also, it returns the entity with the collider with more priority that hits 
    /// </summary>
    public static bool PointCast(FractionalHex point, out Entity colliderEntity)
    {
        //this need to prioritice friendly entities.
        var entityArray = m_ColliderEntityQuery.ToEntityArray(Allocator.TempJob);
        var positions = m_ColliderEntityQuery.ToComponentDataArray<HexPosition>(Allocator.TempJob);
        var colliders = m_ColliderEntityQuery.ToComponentDataArray<Collider>(Allocator.TempJob);
        int entityCount = m_ColliderEntityQuery.CalculateEntityCount();

        bool collisionFound = false;
        colliderEntity = new Entity();

        for (int i = 0; i < entityCount; i++)
        {
            var currEntity = entityArray[i];
            var currPosition = positions[i];
            var currCollider = colliders[i];

            if (point.Distance(currPosition.HexCoordinates) < currCollider.Radious)
            {
                colliderEntity = currEntity;
                collisionFound = true;
                if (currCollider.Layer == ColliderLayer.UNIT)
                {
                    //early out
                    entityArray.Dispose();
                    positions.Dispose();
                    colliders.Dispose();
                    return true;
                }
            }
            
        }

        entityArray.Dispose();
        positions.Dispose();
        colliders.Dispose();

        return collisionFound;
    }

}