using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;


//descomentar metodos en "updateReachableHexListSystem"
public class UpdateIslandTest 
{
    //[Test]
    //public void UpdateListWorksFine()
    //{
    //    var geographicMap = new Dictionary<Hex, bool>() 
    //    {
    //        {new Hex(0,0), true },
    //        {new Hex(1,0), true },
    //        {new Hex(1,1), true },
    //        {new Hex(0,1), false},
    //        {new Hex(0,2), true },
    //        {new Hex(0,3), false},
    //        {new Hex(0,4), true },
    //        {new Hex(0,5), true },
    //        //{new Hex(0,6), true },
    //        {new Hex(0,7), true },
    //        {new Hex(0,8), true},
    //        {new Hex(0,9), true },
    //    };

    //    RuntimeMap map = new RuntimeMap(geographicMap);


    //    UpdateReachableHexListSystem.UpdateIslandDictionaries(map, out Dictionary<Hex, int> islandNumberDictionary, out Dictionary<int, List<Hex>> islandHexesDictionary);
    //    int islandNumber = islandNumberDictionary[new Hex(0, 4)];
    //    var islandHexes = islandHexesDictionary[islandNumber];

    //    Assert.AreEqual(3, islandHexesDictionary.Count);
    //    Assert.AreEqual(2, islandHexes.Count);


    //    Assert.IsTrue(UpdateReachableHexListSystem.IsReachable(new Hex(0, 7), new Hex(0, 9), islandNumberDictionary, islandHexesDictionary));
    //    Assert.IsTrue(UpdateReachableHexListSystem.IsReachable(new Hex(0, 0), new Hex(0, 2), islandNumberDictionary, islandHexesDictionary));
    //    Assert.IsFalse(UpdateReachableHexListSystem.IsReachable(new Hex(0, 0), new Hex(0, 9), islandNumberDictionary, islandHexesDictionary));
    //}
}
