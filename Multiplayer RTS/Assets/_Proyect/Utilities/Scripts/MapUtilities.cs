using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixMath.NET;

public static class MapUtilities 
{
    /// <summary>
    /// used to create the offset of the double slopes.
    /// </summary>
    private const float DOUBLE_SLOPE_OFFSET = 0.2886835f;
    /// <summary>
    /// DO NOT USE FOR SIMULATION!!
    /// It's intended use is to render things, like units, above the map, respecting the map height visual.
    /// </summary>
    public static float GetElevationOfPosition(FractionalHex position, ActiveMap customActiveMap = null)
    {
        ActiveMap activeMap;
        if (customActiveMap == null)
        {
            activeMap = MapManager.ActiveMap;
            Debug.Assert(activeMap != null, "The Active Map is null!!!");
        }
        else
        {
            activeMap = customActiveMap;
        }


        Hex standingHex = position.Round();
        GeographicTile geographicTile;
        if (!activeMap.map.GeographicMapValues.TryGetValue(standingHex, out geographicTile))
        {
            Debug.LogWarning($"There is no map hex on the {position} of the map! returning 0 extra height");
            return 0;
        }
        if (!geographicTile.IsSlope)
        {
            int level = GeographicTile.GetValueOfHeight(geographicTile.heightLevel);
            return level * activeMap.heightPerElevationUnit;
        }
        var slopeData = geographicTile.slopeData;

        bool isSimpleSlope;
        SimpleSlopeDirection simpleSlopeDirection;
        DoubleSlopeDirection doubleSlopeDirection;
        GetTheDirectionAndTypeOfSlope(slopeData, out isSimpleSlope, out simpleSlopeDirection, out doubleSlopeDirection);


        var positionRelativeToHex = position - (FractionalHex)standingHex;
        if (isSimpleSlope)
        {
            int bottomLevel;
            int upperLevel;
            GetHeightLevels(simpleSlopeDirection, slopeData, out bottomLevel, out upperLevel);
            float lerpFactor = GetInterpolationValue(simpleSlopeDirection, positionRelativeToHex);

            return Mathf.Lerp((float)bottomLevel, (float)upperLevel, lerpFactor) * activeMap.heightPerElevationUnit;
        }
        else
        {
            int bottomLevel;
            int upperLevel;
            GetHeightLevels(doubleSlopeDirection, slopeData, out bottomLevel, out upperLevel);
            float lerpFactor = GetInterpolationValue(doubleSlopeDirection, positionRelativeToHex);


            return Mathf.Lerp((float)bottomLevel, (float)upperLevel, lerpFactor) * activeMap.heightPerElevationUnit;
        }
    }

    private static void GetTheDirectionAndTypeOfSlope(SlopeData slopeData, out bool isSimpleSlope, out SimpleSlopeDirection simpleSlopeDirection, out DoubleSlopeDirection doubleSlopeDirection)
    {
        doubleSlopeDirection = DoubleSlopeDirection.UDEFINDED;
        simpleSlopeDirection = SimpleSlopeDirection.UNDEFINED;

        var topRight_h = slopeData.heightSide_0tr;
        var right_h = slopeData.heightSide_1r;
        var downRight_h = slopeData.heightSide_2dr;
        var downLeft_h = slopeData.heightSide_3dl;
        var left_h = slopeData.heightSide_4l;
        var topLeft_h = slopeData.heightSide_5tl;


        bool simple_TopRight = topRight_h != right_h && topRight_h != left_h && left_h == downRight_h && left_h == downLeft_h && right_h == topLeft_h;
        bool simple_Right = right_h != topRight_h && right_h != left_h && left_h == downLeft_h && left_h == topLeft_h && topRight_h == downRight_h;
        bool simple_DownRight = downRight_h != right_h && downRight_h != left_h && left_h == topRight_h && left_h == topLeft_h && right_h == downLeft_h;
        bool simple_DownLeft = downLeft_h != left_h && downLeft_h != right_h && right_h == topRight_h && right_h == topLeft_h && left_h == downRight_h;
        bool simple_Left = left_h != downLeft_h && left_h != right_h && right_h == topRight_h && right_h == downRight_h && downLeft_h == topLeft_h;
        bool simple_TopLeft = topLeft_h != left_h && topLeft_h != right_h && right_h == downLeft_h && right_h == downRight_h && left_h == topRight_h;


        bool double_TopRight = topRight_h == right_h && right_h != downRight_h && right_h != left_h && left_h == downLeft_h && downRight_h == topLeft_h;
        bool double_DownRight = right_h == downRight_h && right_h != downLeft_h && right_h != left_h && left_h == topLeft_h && downLeft_h == topRight_h;
        bool double_Down = downRight_h == downLeft_h && downRight_h != right_h && downRight_h != topLeft_h && right_h == left_h && topLeft_h == topRight_h;
        bool double_DownLeft = downLeft_h == left_h && downLeft_h != downRight_h && downLeft_h != right_h && right_h == topRight_h && downRight_h == topLeft_h;
        bool double_TopLeft = left_h == topLeft_h && left_h != downLeft_h && left_h != right_h && right_h == downRight_h && downLeft_h == topRight_h;
        bool double_Top = topLeft_h == topRight_h && topLeft_h != right_h && topLeft_h != downLeft_h && downLeft_h == downRight_h && right_h == left_h;


        if (simple_TopRight)
        {
            simpleSlopeDirection = SimpleSlopeDirection.TOP_RIGHT;
            isSimpleSlope = true;
            return;
        }
        else if (simple_Right)
        {
            simpleSlopeDirection = SimpleSlopeDirection.RIGHT;
            isSimpleSlope = true;
            return;
        }
        else if (simple_DownRight)
        {
            simpleSlopeDirection = SimpleSlopeDirection.DOWN_RIGHT;
            isSimpleSlope = true;
            return;
        }
        else if (simple_DownLeft)
        {
            simpleSlopeDirection = SimpleSlopeDirection.DOWN_LEFT;
            isSimpleSlope = true;
            return;
        }
        else if (simple_Left)
        {
            simpleSlopeDirection = SimpleSlopeDirection.LEFT;
            isSimpleSlope = true;
            return;
        }
        else if (simple_TopLeft)
        {
            simpleSlopeDirection = SimpleSlopeDirection.TOP_LEFT;
            isSimpleSlope = true;
            return;
        }


        else if (double_TopRight)
        {
            doubleSlopeDirection = DoubleSlopeDirection.TOP_RIGHT;
            isSimpleSlope = false;
            return;
        }
        else if (double_DownRight)
        {
            doubleSlopeDirection = DoubleSlopeDirection.DOWN_RIGHT;
            isSimpleSlope = false;
            return;
        }
        else if (double_Down)
        {
            doubleSlopeDirection = DoubleSlopeDirection.DOWN;
            isSimpleSlope = false;
            return;
        }
        else if (double_DownLeft)
        {
            doubleSlopeDirection = DoubleSlopeDirection.DOWN_LEFT;
            isSimpleSlope = false;
            return;
        }
        else if (double_TopLeft)
        {
            doubleSlopeDirection = DoubleSlopeDirection.TOP_LEFT;
            isSimpleSlope = false;
            return;
        }
        else if (double_Top)
        {
            doubleSlopeDirection = DoubleSlopeDirection.TOP;
            isSimpleSlope = false;
            return;
        }

        else
        {
            throw new System.Exception($"The slope tile doest't have a valid format! it's values are; \n" +
                $"Top right:{ topRight_h}\nRight:{right_h}\nDown right:{downRight_h}\nDown left:{downLeft_h}\nLeft:{left_h}\nTop left:{topLeft_h}");
        }
    }
    private static float GetInterpolationValue(SimpleSlopeDirection simpleSlopeDirection, FractionalHex positionRelativeToHex)
    {
        float lerpValue;
        switch (simpleSlopeDirection)
        {
            case SimpleSlopeDirection.UNDEFINED:
                lerpValue = 0;
                Debug.LogError("The simple slope direction is undefined. You may want to use the double slope directions instead");
                break;
            case SimpleSlopeDirection.LEFT:
                lerpValue = (float)(-positionRelativeToHex.q + positionRelativeToHex.s);
                break;
            case SimpleSlopeDirection.TOP_LEFT:
                lerpValue = (float)(-positionRelativeToHex.q + positionRelativeToHex.r);
                break;
            case SimpleSlopeDirection.TOP_RIGHT:
                lerpValue = (float)(positionRelativeToHex.r - positionRelativeToHex.s);
                break;
            case SimpleSlopeDirection.RIGHT:
                lerpValue = (float)(positionRelativeToHex.q - positionRelativeToHex.s);
                break;
            case SimpleSlopeDirection.DOWN_RIGHT:
                lerpValue = (float)(positionRelativeToHex.q - positionRelativeToHex.r);
                break;
            case SimpleSlopeDirection.DOWN_LEFT:
                lerpValue = (float)(-positionRelativeToHex.r + positionRelativeToHex.s);
                break;
            default:

                lerpValue = 0;
                Debug.LogError("The simple slope direction is undefined. You may want to use the double slope directions instead");
                break;
        }
        lerpValue = Mathf.Clamp01(lerpValue);
        return lerpValue;
    }
    private static float GetInterpolationValue(DoubleSlopeDirection doubleSlopeDirection, FractionalHex positionRelativeToHex)
    {
        float lerpValue;
        switch (doubleSlopeDirection)
        {
            case DoubleSlopeDirection.UDEFINDED:
                lerpValue = 0;
                Debug.LogError("The double slope direction is undefined. You may want to use the simple slope directions instead");
                break;
            case DoubleSlopeDirection.TOP_LEFT:
                lerpValue = Mathf.InverseLerp(-DOUBLE_SLOPE_OFFSET, DOUBLE_SLOPE_OFFSET, -(float)positionRelativeToHex.q);
                break;
            case DoubleSlopeDirection.TOP:
                lerpValue = Mathf.InverseLerp(-DOUBLE_SLOPE_OFFSET, DOUBLE_SLOPE_OFFSET, (float)positionRelativeToHex.r);
                break;
            case DoubleSlopeDirection.TOP_RIGHT:
                lerpValue = Mathf.InverseLerp(-DOUBLE_SLOPE_OFFSET, DOUBLE_SLOPE_OFFSET, -(float)positionRelativeToHex.s);
                break;
            case DoubleSlopeDirection.DOWN_RIGHT:
                lerpValue = Mathf.InverseLerp(-DOUBLE_SLOPE_OFFSET, DOUBLE_SLOPE_OFFSET, (float)positionRelativeToHex.q);
                break;
            case DoubleSlopeDirection.DOWN:
                lerpValue = Mathf.InverseLerp(-DOUBLE_SLOPE_OFFSET, DOUBLE_SLOPE_OFFSET, -(float)positionRelativeToHex.r);
                break;
            case DoubleSlopeDirection.DOWN_LEFT:
                lerpValue = Mathf.InverseLerp(-DOUBLE_SLOPE_OFFSET, DOUBLE_SLOPE_OFFSET, (float)positionRelativeToHex.s);
                break;
            default:
                lerpValue = 0;
                Debug.LogError("The double slope direction is undefined. You may want to use the simple slope directions instead");
                break;
        }

        lerpValue = Mathf.Clamp01(lerpValue);
        return lerpValue;
    }
    private static void GetHeightLevels(SimpleSlopeDirection simpleSlopeDirection, SlopeData slopeData, out int bottomLevel, out int upperLevel)
    {
        switch (simpleSlopeDirection)
        {
            case SimpleSlopeDirection.UNDEFINED:
                bottomLevel = 0;
                upperLevel = 0;
                Debug.LogError("The simple slope direction is undefined. You may want to use the double slope directions instead");
                break;
            case SimpleSlopeDirection.LEFT:
                bottomLevel = GeographicTile.GetValueOfHeight(slopeData.heightSide_1r);
                upperLevel = GeographicTile.GetValueOfHeight(slopeData.heightSide_4l);
                break;
            case SimpleSlopeDirection.TOP_LEFT:
                bottomLevel = GeographicTile.GetValueOfHeight(slopeData.heightSide_2dr);
                upperLevel = GeographicTile.GetValueOfHeight(slopeData.heightSide_5tl);
                break;
            case SimpleSlopeDirection.TOP_RIGHT:
                bottomLevel = GeographicTile.GetValueOfHeight(slopeData.heightSide_3dl);
                upperLevel = GeographicTile.GetValueOfHeight(slopeData.heightSide_0tr);
                break;
            case SimpleSlopeDirection.RIGHT:
                bottomLevel = GeographicTile.GetValueOfHeight(slopeData.heightSide_4l);
                upperLevel = GeographicTile.GetValueOfHeight(slopeData.heightSide_1r);
                break;
            case SimpleSlopeDirection.DOWN_RIGHT:
                bottomLevel = GeographicTile.GetValueOfHeight(slopeData.heightSide_5tl);
                upperLevel = GeographicTile.GetValueOfHeight(slopeData.heightSide_2dr);
                break;
            case SimpleSlopeDirection.DOWN_LEFT:
                bottomLevel = GeographicTile.GetValueOfHeight(slopeData.heightSide_0tr);
                upperLevel = GeographicTile.GetValueOfHeight(slopeData.heightSide_3dl);
                break;
            default:
                bottomLevel = 0;
                upperLevel = 0;
                Debug.LogError("The simple slope direction is undefined. You may want to use the double slope directions instead");
                break;
        }
    }
    private static void GetHeightLevels(DoubleSlopeDirection doubleSlopeDirection, SlopeData slopeData, out int bottomLevel, out int upperLevel)
    {
        switch (doubleSlopeDirection)
        {
            case DoubleSlopeDirection.UDEFINDED:
                bottomLevel = 0;
                upperLevel = 0;
                Debug.LogError("The double slope direction is undefined. You may want to use the simple slope directions instead");
                break;
            case DoubleSlopeDirection.TOP_LEFT:
                bottomLevel = GeographicTile.GetValueOfHeight(slopeData.heightSide_1r);
                upperLevel = GeographicTile.GetValueOfHeight(slopeData.heightSide_4l);
                break;
            case DoubleSlopeDirection.TOP:
                bottomLevel = GeographicTile.GetValueOfHeight(slopeData.heightSide_2dr);
                upperLevel = GeographicTile.GetValueOfHeight(slopeData.heightSide_5tl);
                break;
            case DoubleSlopeDirection.TOP_RIGHT:
                bottomLevel = GeographicTile.GetValueOfHeight(slopeData.heightSide_3dl);
                upperLevel = GeographicTile.GetValueOfHeight(slopeData.heightSide_0tr);
                break;
            case DoubleSlopeDirection.DOWN_RIGHT:
                bottomLevel = GeographicTile.GetValueOfHeight(slopeData.heightSide_4l);
                upperLevel = GeographicTile.GetValueOfHeight(slopeData.heightSide_1r);
                break;
            case DoubleSlopeDirection.DOWN:
                bottomLevel = GeographicTile.GetValueOfHeight(slopeData.heightSide_5tl);
                upperLevel = GeographicTile.GetValueOfHeight(slopeData.heightSide_2dr);
                break;
            case DoubleSlopeDirection.DOWN_LEFT:
                bottomLevel = GeographicTile.GetValueOfHeight(slopeData.heightSide_0tr);
                upperLevel = GeographicTile.GetValueOfHeight(slopeData.heightSide_3dl);
                break;
            default:
                bottomLevel = 0;
                upperLevel = 0;
                Debug.LogError("The double slope direction is undefined. You may want to use the simple slope directions instead");
                break;
        }
    }

    /// <summary>
    /// USED IN SIMULATION!!
    /// </summary>
    public static bool IsTraversable(Hex hexA, Hex hexB, MapType mapToUse = MapType.GEOGRAPHYC, ActiveMap customActiveMap = null)
    {
        ActiveMap activeMap;
        if (customActiveMap == null)
        {
            activeMap = MapManager.ActiveMap;
            Debug.Assert(activeMap != null, "The Active Map is null!!!");
        }
        else
        {
            activeMap = customActiveMap;
        }

        if ((hexB - hexA).Lenght() > 1)
        {
            Debug.LogWarning($"you are checking if two hexes that are not adjacent are traversable.({hexA} y {hexB})");
            return false;
        }

        GeographicTile geoA, geoB;

        //this region cheks if the hex exist the map. and if it is desocupied on that map.
        if (!activeMap.map.GeographicMapValues.TryGetValue(hexA, out geoA) || !activeMap.map.GeographicMapValues.TryGetValue(hexB, out geoB))
        {
            return false;
        }
        switch (mapToUse)
        {
            case MapType.GEOGRAPHYC:
                if (!geoA.walkable || !geoB.walkable)
                {
                    return false;
                }
                break;
            case MapType.MOVEMENT:
                if (!activeMap.map.MovementMapValues.TryGetValue(hexA, out bool walkableA) || !activeMap.map.MovementMapValues.TryGetValue(hexB, out bool walkableB))
                {
                    return false;
                }
                else if (!walkableA || !walkableB)
                {
                    return false;
                }
                break;
            case MapType.UNIT:
                if (!activeMap.map.UnitsMapValues.TryGetValue(hexA, out bool freeA) || !activeMap.map.UnitsMapValues.TryGetValue(hexB, out bool freeB))
                {
                    return false;
                }
                else if (!freeA || !freeB)
                {
                    return false;
                }
                break;
            default:
                throw new System.Exception("invalid or not implemented map type");
        }


        //after seeng if the hex is walkable and before the slope check we check if both hexes are the same.
        if (hexA == hexB)
        {
            return true;
        }

        MapHeight heightA;
        MapHeight heightB;
        if (geoA.IsSlope)
        {
            var directionToB = GetDirectionToAdjacentHex(hexA, hexB);
            switch (directionToB)
            {
                case HexDirection.TOP_RIGHT:
                    heightA = geoA.slopeData.heightSide_0tr;
                    break;
                case HexDirection.RIGHT:
                    heightA = geoA.slopeData.heightSide_1r;
                    break;
                case HexDirection.DOWN_RIGHT:
                    heightA = geoA.slopeData.heightSide_2dr;
                    break;
                case HexDirection.DOWN_LEFT:
                    heightA = geoA.slopeData.heightSide_3dl;
                    break;
                case HexDirection.LEFT:
                    heightA = geoA.slopeData.heightSide_4l;
                    break;
                case HexDirection.TOP_LEFT:
                    heightA = geoA.slopeData.heightSide_5tl;
                    break;
                default:
                    Debug.LogError("Error! Check 'GetDirectionToAdjacentHex'.");
                    return false;
            }
        }
        else
            heightA = geoA.heightLevel;

        if (geoB.IsSlope)
        {
            var directionToA = GetDirectionToAdjacentHex(hexB, hexA);
            switch (directionToA)
            {
                case HexDirection.TOP_RIGHT:
                    heightB = geoB.slopeData.heightSide_0tr;
                    break;
                case HexDirection.RIGHT:
                    heightB = geoB.slopeData.heightSide_1r;
                    break;
                case HexDirection.DOWN_RIGHT:
                    heightB = geoB.slopeData.heightSide_2dr;
                    break;
                case HexDirection.DOWN_LEFT:
                    heightB = geoB.slopeData.heightSide_3dl;
                    break;
                case HexDirection.LEFT:
                    heightB = geoB.slopeData.heightSide_4l;
                    break;
                case HexDirection.TOP_LEFT:
                    heightB = geoB.slopeData.heightSide_5tl;
                    break;
                default:
                    Debug.LogError("Error! Check 'GetDirectionToAdjacentHex'.");
                    return false;
            }
        }
        else
            heightB = geoB.heightLevel;


        if (heightA == heightB)
            return true;
        else
            return false;
    }

    public static HexDirection GetDirectionToAdjacentHex(Hex startHex, Hex other)
    {
        Hex diference = other - startHex;
        for (int i = 0; i < 6; i++)
        {
            Hex direction = Hex.directions[i];
            if (direction == diference) 
            {
                return (HexDirection)i;
            }
        }
        throw new System.ArgumentException("The Hex given to this function must be adjacent ones!");
    }
    public static bool CompatibleHeightLevel(MapHeight a, MapHeight b)
    {
        //esta funcion trabaja con los bits-> los niveles son compatibles si al menos uno de las dos alturas comparte un nivel.
        //ej: 0010 &                |  0001 &                |
        //    0001 --> 0000 = false |  0011  --> 0001 = true |
        var similarrities = a & b;
        return similarrities != 0;
    }


    /// <summary>
    /// gets if there is clear direct path bewtween the two points.
    /// </summary>
    public static bool PathToPointIsClear(FractionalHex position, FractionalHex point , ActiveMap customActiveMap = null, bool pathIsClearEvenIfDestPointIsBlocked = false)
    {
        ActiveMap activeMap;
        if (customActiveMap == null)
        {
            activeMap = MapManager.ActiveMap;
            Debug.Assert(activeMap != null, "The Active Map is null!!!");
        }
        else
        {
            activeMap = customActiveMap;
        }

        Hex pointHex = point.Round();
        var hexesInBewtween = Hex.HexesInBetween(position, point);
        for (int i = 0; i < hexesInBewtween.Count - 1; i++)
        {
            Hex hexA = hexesInBewtween[i];
            Hex hexB = hexesInBewtween[i + 1];

            if (pointHex == hexB && pathIsClearEvenIfDestPointIsBlocked)
            {
                if (!MapUtilities.IsTraversable(hexA, hexB, MapUtilities.MapType.GEOGRAPHYC, activeMap))
                {
                    return false;
                }
            }
            else
            {
                if (!MapUtilities.IsTraversable(hexA, hexB, MapUtilities.MapType.MOVEMENT, activeMap))
                {
                    return false;
                }
            }
        }
        return true;
    }
    /// <summary>
    /// mode = true, uses the movementMap. mode = false, uses the unitOcupationMap
    /// </summary>    
    public static Hex FindClosestOpenHex(FractionalHex position, RuntimeMap map, bool mode)
    {
        Dictionary<Hex, bool> mapValuesToUse;
        if (mode)
        {
            mapValuesToUse = new Dictionary<Hex, bool>(map.MovementMapValues);
        }
        else 
        {
            mapValuesToUse = new Dictionary<Hex, bool>(map.UnitsMapValues);
        }

        var closestOpenHex = Hex.Zero;
        Fix64 closestOpenHexDistance = Fix64.MaxValue;
        foreach (var hexValuePair in mapValuesToUse)
        {
            if (!hexValuePair.Value) { continue; }

            var hex = hexValuePair.Key;
            var distance = position.Distance((FractionalHex)hex);

            if (distance <= closestOpenHexDistance)
            {
                closestOpenHex = hex;
                closestOpenHexDistance = distance;
            }
        }

        return closestOpenHex;

    }
    /// <summary>
    /// REVISA EN UNA LINEA DESDE LA POSICION DE REFERENCIA Y EL INICIO Y REVISA TODOS LOS VECINOS DE ESA LINEA.
    /// </summary>
    public static bool TryFindClosestOpenAndReachableHex(out Hex closestOpenHex, FractionalHex referencePos, FractionalHex startingPos, Dictionary<Hex,bool> mapValues)
    {
        
        bool hexFound = false;

        Hex startingHexRound = startingPos.Round();
        closestOpenHex = startingHexRound;

        var hexesInBewtween = Hex.HexesInBetween(referencePos, startingPos);
        if (hexesInBewtween.Count > 1)
        {
            for (int i = 0; i < hexesInBewtween.Count; i++)
            {
                Hex curr = hexesInBewtween[i];
                if (HexIsOpenAndReachable(startingHexRound, curr, mapValues))
                {
                    //si este hexagono esta libre y desocupado usamos este como nuevo blanco.
                    closestOpenHex = curr;
                    hexFound = true;
                    break;
                }
                else
                {
                    var openNeightbors = new List<Hex>();
                    for (int j = 0; j < 6; j++)
                    {
                        Hex neightbor = curr.Neightbor(j);
                        if (HexIsOpenAndReachable(startingHexRound, neightbor, mapValues))
                        {
                            openNeightbors.Add(neightbor);
                        }                        
                    }

                    if (openNeightbors.Count > 0)
                    {
                        Hex closestNeightbor = openNeightbors[0];
                        bool closestNeightborIsTraversable = IsTraversable(curr, closestNeightbor);
                        Fix64 closestDist = startingPos.Distance((FractionalHex)closestNeightbor);
                        
                        for (int j = 1; j < openNeightbors.Count; j++)
                        {
                            Fix64 dist = startingPos.Distance((FractionalHex)openNeightbors[j]);
                            bool canTraverseToNeightbor = IsTraversable(curr, openNeightbors[j]);
                            if (closestNeightborIsTraversable)
                            {
                                if (canTraverseToNeightbor)
                                {
                                    if (dist < closestDist)
                                    {
                                        closestNeightbor = openNeightbors[j];
                                        closestDist = dist;
                                        closestNeightborIsTraversable = true;
                                    }
                                }
                            }
                            else 
                            {
                                if (canTraverseToNeightbor)
                                {
                                    closestNeightbor = openNeightbors[j];
                                    closestDist = dist;
                                    closestNeightborIsTraversable = true;
                                }
                                else 
                                {
                                    if (dist < closestDist)
                                    {
                                        closestNeightbor = openNeightbors[j];
                                        closestDist = dist;
                                        closestNeightborIsTraversable = false;
                                    }
                                } 
                            }                            
                        }

                        closestOpenHex = closestNeightbor;
                        hexFound = true;
                        break;
                    } 
                }
            }
        }
        else 
        {
            closestOpenHex = startingHexRound;
            return HexIsOpen(startingHexRound, mapValues);
        }


        return hexFound;
    }

    private static bool HexIsOpen(Hex hex, Dictionary<Hex, bool> mapValues)
    {
        if (mapValues.TryGetValue(hex, out bool open))
        {
            return open;
        }
        else
        {
            return false;
        }
    }
    public static bool HexIsOpenAndReachable(Hex current, Hex end, Dictionary<Hex, bool> mapValues)
    {
        bool reachable = UpdateReachableHexListSystem.IsReachable(current, end);
        bool free = HexIsOpen(end, mapValues);
        //Debug.Log($"the hex is free:{free} and reachable:{reachable}");
        return reachable && free;
    }



    public enum HexDirection { TOP_RIGHT = 0, RIGHT = 1, DOWN_RIGHT = 2, DOWN_LEFT = 3,LEFT = 4, TOP_LEFT = 5 }
    public enum MapType {GEOGRAPHYC, MOVEMENT, UNIT }
    private enum SimpleSlopeDirection { UNDEFINED = 0, LEFT, TOP_LEFT, TOP_RIGHT, RIGHT, DOWN_RIGHT, DOWN_LEFT }
    private enum DoubleSlopeDirection { UDEFINDED = 0, TOP_LEFT, TOP, TOP_RIGHT, DOWN_RIGHT, DOWN, DOWN_LEFT }

}
