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

    //this matrix allows to have complex layer collision bewtween colliders.(collision bewtween walls and clifs is not here.)
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
            collidersRadious[collectionIndex] = collider.Radius;           
            collidersLayers[collectionIndex] = collider.Layer;
            teams[collectionIndex] = team.Number;

            collectionIndex++;
        });

        var pointsSorted = SpartialSortUtils.GetPointsSpartiallySorted(positions, 1);


        //collide
        Entities.ForEach((ref HexPosition hexPosition, ref Collider collider, ref Team team) => 
        {
            if (!layerCollisionMatrix.TryGetValue(collider.Layer, out ColliderFlags colliderFlag))
            {
                throw new System.Exception("Assign the collider layer type on the collision matrix!");
            }
            else if (colliderFlag == ColliderFlags.NONE)
            {
                return;
            }

            FractionalHex position = hexPosition.HexCoordinates;
            //FractionalHex prevPos = hexPosition.PrevPosition;
            bool areHexOnCenter = activeMap.map.MovementMapValues.TryGetValue(position.Round(), out bool centerIsWalkable);
            bool centerOnWalkableHex = areHexOnCenter && centerIsWalkable;


            if (! centerOnWalkableHex)
            {
                FractionalHex closestOpenHex = (FractionalHex)MapUtilities.FindClosestOpenHex(position, activeMap.map, true);
                FractionalHex collisionResolutionDir = (closestOpenHex - position).NormalizedManhathan();
                hexPosition.HexCoordinates = position + (collisionResolutionDir * collider.Radius);
            }
            else if (CollisionTestAgainstMap(collider.Radius, position, activeMap, out List<FractionalHex> collidingResponseVectors))
            {
                var colisionIntensity = Fix64.Zero; ;
                var colisionVectorSum = (FractionalHex)Hex.Zero;
                foreach (var vector in collidingResponseVectors)
                {
                    if (vector.Lenght() > colisionIntensity)
                        colisionIntensity = vector.Lenght();
                    colisionVectorSum += vector;
                }
                var direction = colisionVectorSum.NormalizedManhathan();

                hexPosition.HexCoordinates = position + colisionVectorSum;//direction * colisionIntensity;
            }

            else //collide with other colliders
            {
                int thisIndex;
                var pointsToCheck = SpartialSortUtils.GetFilteredPoints(position, pointsSorted, out thisIndex);
                var collidableIndices = GetCollidableCollidersIndices(collider, pointsToCheck, collidersLayers);

                //SimpleCollisionCheckAndResponse(ref hexPosition, collider.Radious,  positions, collidersRadious, teams, thisIndex, collidableIndices);
                //vamos a intentar una collision de dos pasos.
                DoubleStepCollision(ref hexPosition, collider.Radius, team, positions, collidersRadious, teams, thisIndex, collidableIndices);
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
    /// It returns true if there is a collision with the map, false otherwise. The 'out' list comntains the most important corners that are colliding and the distance of the overlap.On priority order:
    /// 1-collision with a hex outside the map
    /// 2-collision with an unwalkable hex(mapa geografico)
    /// 3-collision with an unwalkable hex(construcciones/mapa de movimiento)
    /// </summary>
    private static bool CollisionTestAgainstMap(Fix64 colliderArea, FractionalHex position, ActiveMap activeMap, out List<FractionalHex> collisionResponseVectors)
    {
        collisionResponseVectors = new List<FractionalHex>();

        bool outsideMapCollision = false;
        bool geographycHexCollision = false;
        bool movementMapCollision = false;
        var collisionResponseVectorsToOutsideMapCollitions = new List<FractionalHex>();
        var collisionResponseVectorsThatCauseGeographycCollisions = new List<FractionalHex>();
        var collisionResponseVectorsThatCauseMovementMapCollisions = new List<FractionalHex>();


        Hex centerHex = position.Round();
        //hace un check en todas las esquinas del area de colision.
        for (int i = 0; i < 6; i++)
        {
            var direction = Hex.directions[i];
            var corner = position + ((FractionalHex)direction * colliderArea);

            Hex hexThatCanHaveACollision = centerHex + direction;
            if (corner.Round() == centerHex)
                continue;
            else if (hexThatCanHaveACollision != corner.Round())
                continue;

            if (!activeMap.map.MovementMapValues.TryGetValue(corner.Round(), out bool walkable))
            {
                outsideMapCollision = true;
                var collisionResponseVector = -(FractionalHex)direction * GetCollisionDistance(centerHex, corner);
                collisionResponseVectorsToOutsideMapCollitions.Add(collisionResponseVector);
            }
            else
            {
                if (! MapUtilities.IsTraversable(corner.Round(), centerHex, MapUtilities.MapType.GEOGRAPHYC))
                {
                    geographycHexCollision = true;
                    var collisionResponseVector = -(FractionalHex)direction * GetCollisionDistance(centerHex, corner);
                    collisionResponseVectorsThatCauseGeographycCollisions.Add(collisionResponseVector);
                }
                else if (!walkable)
                {
                    movementMapCollision = true;
                    var collisionResponseVector = -(FractionalHex)direction * GetCollisionDistance(centerHex, corner);
                    collisionResponseVectorsThatCauseMovementMapCollisions.Add(collisionResponseVector);
                }
            }


        }
        if (outsideMapCollision)
        {
            collisionResponseVectors = collisionResponseVectorsToOutsideMapCollitions;
            return true;
        }
        else if (geographycHexCollision)
        {
            collisionResponseVectors = collisionResponseVectorsThatCauseGeographycCollisions;
            return true;
        }
        else if (movementMapCollision)
        {
            collisionResponseVectors = collisionResponseVectorsThatCauseMovementMapCollisions;
            return true;
        }
        else
        {
            return false;
        }
    }
    /// <summary>
    /// deprecated!
    ///// </summary>
    //private static bool CollisionTestAgainstMap(Fix64 colliderArea, FractionalHex position, ActiveMap activeMap, out List<int> collidingDirections)
    //{
    //    collidingDirections = new List<int>();

    //    bool outsideMapCollision = false;
    //    bool geographycHexCollision = false;
    //    bool movementMapCollision = false;
    //    var directionsToOutsideMapCollitions = new List<int>(); 
    //    var directionsThatCauseGeographycCollisions = new List<int>();
    //    var directionsThatCauseMovementMapCollisions = new List<int>();

    //    Hex centerHex = position.Round();
    //    //hace un check en todas las esquinas del area de colision.
    //    for (int i = 0; i < 6; i++)
    //    {
    //        var direction = Hex.directions[i];
    //        var corner = position + ((FractionalHex)direction * colliderArea);
    //        if (activeMap.map.MovementMapValues.TryGetValue(corner.Round(), out bool walkable))
    //        {
    //            if (! MapUtilities.IsTraversable(centerHex, corner.Round(), MapUtilities.MapType.GEOGRAPHYC))
    //            {
    //                directionsThatCauseGeographycCollisions.Add(i);
    //                geographycHexCollision = true;
    //            }
    //            else if (! walkable)
    //            {
    //                directionsThatCauseMovementMapCollisions.Add(i);
    //                movementMapCollision = true;
    //            }
    //        }
    //        else 
    //        {
    //            directionsToOutsideMapCollitions.Add(i);
    //            outsideMapCollision = true;
    //        }
    //    }

    //    if (outsideMapCollision)
    //    {
    //        collidingDirections = directionsToOutsideMapCollitions;
    //        return true;
    //    }
    //    else if (geographycHexCollision)
    //    {
    //        collidingDirections = directionsThatCauseGeographycCollisions;
    //        return true;
    //    }
    //    else if (movementMapCollision)
    //    {
    //        collidingDirections = directionsThatCauseMovementMapCollisions;
    //        return true;
    //    }
    //    else
    //    {
    //        return false;
    //    }
    //}
    public static Fix64 GetCollisionDistance(Hex centerHex, FractionalHex collidingCorner)
    {
        //es importante conseguir el true mid point.
        //ese punto hace verdadera la igualdad ->  collidingDistance = collidingCorner.Distance(centerHex) - 0.5(mid point distance);
        //ya que alinea los 3 puntos importantes; los dos centros y el borde.


        //hay 6 casos uwu.para sacar la distancia de colición
        var collidingHex = collidingCorner.Round();
        var direction = MapUtilities.GetDirectionToAdjacentHex(centerHex, collidingHex);
        Fix64 q = collidingCorner.q - (Fix64)centerHex.q;
        Fix64 r = collidingCorner.r - (Fix64)centerHex.r;
        Fix64 s = collidingCorner.s - (Fix64)centerHex.s;
        FractionalHex trueMidPoint = new FractionalHex();
        switch (direction)
        {
            case MapUtilities.HexDirection.TOP_RIGHT://Q
                trueMidPoint = new FractionalHex
                    (
                        (Fix64)centerHex.q,
                        collidingCorner.r + (q * (Fix64)0.5)
                    );
                break;
            case MapUtilities.HexDirection.RIGHT://R
                trueMidPoint = new FractionalHex
                (
                    collidingCorner.q + (r * (Fix64)0.5),
                    (Fix64)centerHex.r
                );
                break;
            case MapUtilities.HexDirection.DOWN_RIGHT://S
                trueMidPoint = new FractionalHex
                (
                     collidingCorner.q + (s * (Fix64)0.5),
                     collidingCorner.r + (s * (Fix64)0.5)
                );
                break;
            case MapUtilities.HexDirection.DOWN_LEFT://Q
                trueMidPoint = new FractionalHex
                (
                    (Fix64)centerHex.q,
                    collidingCorner.r + (q * (Fix64)0.5)
                );
                break;
            case MapUtilities.HexDirection.LEFT://R
                trueMidPoint = new FractionalHex
                (
                    collidingCorner.q + r * (Fix64)0.5,
                    (Fix64)centerHex.r
                );
                break;
            case MapUtilities.HexDirection.TOP_LEFT://S
                trueMidPoint = new FractionalHex
                (
                    collidingCorner.q + (s * (Fix64)0.5),
                    collidingCorner.r + (s * (Fix64)0.5)
                );
                break;
            default:
                throw new System.Exception();
        }

        var collidingDistance = trueMidPoint.Distance((FractionalHex)centerHex) - (Fix64)0.5;
        if (collidingDistance < Fix64.Zero)
        {
            throw new System.Exception();
        }
        return collidingDistance;
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

            if (point.Distance(currPosition.HexCoordinates) < currCollider.Radius)
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