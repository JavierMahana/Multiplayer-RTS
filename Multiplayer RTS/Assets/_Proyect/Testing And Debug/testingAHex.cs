using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testingAHex : MonoBehaviour
{
    public Hex e= new Hex(1,2);
    public Hex i = new Hex(1,1);
    void Start()
    {
        Debug.Log($"{MapManager.ActiveMap.map.GeographicMapValues[e].walkable} {MapManager.ActiveMap.map.MovementMapValues[e]} {MapManager.ActiveMap.map.UnitsMapValues[e]}");
        Debug.Log($"{MapManager.ActiveMap.map.GeographicMapValues[i].walkable} {MapManager.ActiveMap.map.MovementMapValues[i]} {MapManager.ActiveMap.map.UnitsMapValues[i]}");
        Debug.Log($"{MapUtilities.IsTraversable(e,i)}");
    }
    private void Update()
    {
        //Debug.Log($"{MapManager.ActiveMap.map.GeographicMapValues[e].walkable} {MapManager.ActiveMap.map.MovementMapValues[e]} {MapManager.ActiveMap.map.UnitsMapValues[e]}");
    }

}
