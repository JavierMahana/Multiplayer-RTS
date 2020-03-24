using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName ="Map")]
public class Map : SerializedScriptableObject
{
    public float elevationPerHeightLevel;
    public Vector2 mapScale = Vector2.one;
    public Vector2 hexSizeOffSet = Vector2.zero;
    [HideInInspector]
    public Dictionary<Hex, Material> HexMaterials;
    [HideInInspector]
    public Dictionary<Hex, bool> HexWalkableFlags;
    [HideInInspector]
    public Dictionary<Hex, MapHeight> HexHeights;
    [HideInInspector]
    public Dictionary<Hex, SlopeData> HexSlopeDatas;
    public void InitMap(Dictionary<Hex, Material> hexMaterials, Dictionary<Hex, bool> hexWalkableFlags,
                        Dictionary<Hex, MapHeight> hexHeights, Dictionary<Hex,SlopeData> hexSlopeDatas)
    {
        HexMaterials =  new Dictionary<Hex, Material>(hexMaterials);
        HexWalkableFlags = new Dictionary<Hex, bool>(hexWalkableFlags);
        HexHeights = new Dictionary<Hex, MapHeight>(hexHeights);
        HexSlopeDatas = new Dictionary<Hex, SlopeData>(hexSlopeDatas);
    }

}
