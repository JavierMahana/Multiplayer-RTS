using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using FixMath.NET;

public static class SpartialSortUtils
{    
    public static Dictionary<Hex, List<SortPoint>> GetPointsSpartiallySorted(FractionalHex[] allPositions, int distance)
    {
        int pointCount = allPositions.Length;

        //1- We initialize all the point on the cells that they belong
        var pointsForechHex = GetHexToPointsDictionary(allPositions);

        //2- we fill the spartial distribution points dictionary with the previous dictionary
        var spartiallySortedPoints = new Dictionary<Hex, List<SortPoint>>(pointCount);
        foreach (var cellKeyValue in pointsForechHex)
        {
            Hex cell = cellKeyValue.Key;
            var allPointAtRangeOfHex = GetAllPointsAtRange(cell, distance, pointsForechHex);

            spartiallySortedPoints.Add(cell, allPointAtRangeOfHex);
        }

        //3- return
        return spartiallySortedPoints;
    }
    public static Dictionary<Hex, List<SortPoint>> GetHexToPointsDictionary(FractionalHex[] allPositions)
    {
        int pointCount = allPositions.Length;

        var cellPoints = new Dictionary<Hex, List<SortPoint>>(pointCount);
        for (int i = 0; i < pointCount; i++)
        {
            var currPos = allPositions[i];
            var currSortPoint = new SortPoint(i, currPos);
            var currHex = currPos.Round();

            List<SortPoint> pointsOfHex;
            if (cellPoints.TryGetValue(currHex, out pointsOfHex))
            {
                pointsOfHex.Add(currSortPoint);
                cellPoints[currHex] = pointsOfHex;
            }
            else
            {
                pointsOfHex = new List<SortPoint>() { currSortPoint };
                cellPoints.Add(currHex, pointsOfHex);
            }

        }
        return cellPoints;
    }
    public static List<SortPoint> GetAllPointsAtRange(Hex hex, int range, Dictionary<Hex, List<SortPoint>>pointForeachHex)
    {
        var pointsAtRange = new List<SortPoint>();

        var cellsToCheck = Hex.AllHexesInRange(hex, range, true);

        foreach (Hex neightbor in cellsToCheck)
        {
            if (pointForeachHex.TryGetValue(neightbor, out List<SortPoint> neightborPoints))
            {
                pointsAtRange.AddRange(neightborPoints);
            }
        }

        return pointsAtRange;
    }


    /// <summary>
    /// gets the list of points that a position can access based on the dictionary provided 
    /// </summary>
    public static List<SortPoint> GetFilteredPoints(FractionalHex position, Dictionary<Hex, List<SortPoint>> pointsSpartiallySorted, out int selfIndex, bool excludeItself = true)
    {
        selfIndex = int.MaxValue;
        List<SortPoint> points;
        if (pointsSpartiallySorted.TryGetValue(position.Round(), out points))
        {
            var returnPoints = new List<SortPoint>();
            if (excludeItself)
            {

                foreach (var point in points)
                {
                    if (point.position == position)
                    {
                        selfIndex = point.index;
                        continue;
                    }
                    returnPoints.Add(point);
                }
            }
            return returnPoints;
        }
        else
        {
            points = new List<SortPoint>();
            return points;
        }
    }
    /// <summary>
    /// points filtered by distance
    /// </summary>
    public static List<SortPoint> GetFilteredPoints(FractionalHex position, Fix64 maxDistance, Dictionary<Hex, List<SortPoint>> pointsSpartiallySorted, bool excludeItself = true)
    {
        List<SortPoint> points;
        if (pointsSpartiallySorted.TryGetValue(position.Round(), out points))
        {
            var filteredPoints = new List<SortPoint>();
            foreach (var point in points)
            {
                if (excludeItself) 
                {
                    if (point.position == position)
                    { continue; }
                } 

                var distance = position.Distance(point.position);
                if (distance <= maxDistance)
                {
                    filteredPoints.Add(point);
                }
            }

            return filteredPoints;
        }
        else
        {
            points = new List<SortPoint>();
            return points;
        }
    }
    public static List<SortPoint> GetFilteredPoints(FractionalHex position, Fix64 maxDistance, List<SortPoint> initialListOfPoints, bool excludeItself = true)
    {
        var filteredPoints = new List<SortPoint>();
        foreach (var point in initialListOfPoints)
        {
            if (excludeItself)
            {
                if (point.position == position)
                { continue; }
            }

            var distance = position.Distance(point.position);
            if (distance <= maxDistance)
            {
                filteredPoints.Add(point);
            }
        }

        return filteredPoints;
    }
}
