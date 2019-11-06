using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnOneCubePerHex : MonoBehaviour
{
    public float z;
    public Vector3 scale;
    void Start()
    {
        Layout layout = MapManager.ActiveMap.layout;
        RuntimeMap runtimeMap = MapManager.ActiveMap.map;

        foreach (var value in runtimeMap.StaticMapValues)
        {
            var world = layout.HexToWorld(value.Key);

            var cube = GameObject.CreatePrimitive(PrimitiveType.Quad);
            cube.transform.position = new Vector3((float)world.x, (float)world.y, z);
            cube.transform.localScale = scale;
        }
    }


}
