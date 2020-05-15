using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName ="Map")]
public class Map : SerializedScriptableObject
{
    public float elevationPerHeightUnit = 0;
    public Vector2 spriteArtMapScale = Vector2.one;
    public Vector2 materialArtMapScale = Vector2.one;
    [HideInInspector]
    public Vector2Int mapProportions;
    [HideInInspector]
    public Dictionary<Hex, Material> HexMaterials;
    [HideInInspector]
    public Dictionary<Hex, Sprite> HexSprites;
    [HideInInspector]
    public Dictionary<Hex, bool> HexWalkableFlags;
    [HideInInspector]
    public Dictionary<Hex, MapHeight> HexHeights;
    [HideInInspector]
    public Dictionary<Hex, SlopeData> HexSlopeDatas;
    public void InitMap(Dictionary<Hex, Material> hexMaterials, Dictionary<Hex, Sprite> hexSprites, Dictionary<Hex, bool> hexWalkableFlags,
                        Dictionary<Hex, MapHeight> hexHeights, Dictionary<Hex,SlopeData> hexSlopeDatas, Vector2Int mapProportions)
    {
        HexMaterials     =  new Dictionary<Hex, Material>(hexMaterials);
        HexSprites       = new Dictionary<Hex, Sprite>(hexSprites);
        HexWalkableFlags = new Dictionary<Hex, bool>(hexWalkableFlags);
        HexHeights       = new Dictionary<Hex, MapHeight>(hexHeights);
        HexSlopeDatas    = new Dictionary<Hex, SlopeData>(hexSlopeDatas);

        this.mapProportions = mapProportions;
    }

}
