using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName ="Map")]
public class Map : SerializedScriptableObject
{

    public Vector2 mapScale = Vector2.one;
    //[HideInInspector]
    public Dictionary<Hex, Material> HexMaterials;
    //[HideInInspector]
    public Dictionary<Hex, bool> HexWalkableFlags;
    public void InitMap(Dictionary<Hex, Material> hexMaterials, Dictionary<Hex, bool> hexWalkableFlags)
    {
        HexMaterials =  new Dictionary<Hex, Material>(hexMaterials);
        HexWalkableFlags = new Dictionary<Hex, bool>(hexWalkableFlags);
    }

    //crea un mapa de las dimenciones deceadas y lo llena con una tipo de hex predeterminado.
    //public void ChangeHexValues(Hex hex, TileValues tileValues)
    //{
    //    if (HexMaterials.ContainsKey(hex) && HexWalkableFlags.ContainsKey(hex))
    //    {
    //        HexMaterials[hex] = tileValues.material;
    //        HexWalkableFlags[hex] = tileValues.walkable;
    //    }
    //    else 
    //    {
    //        Debug.LogError("you are trying to chang the values of a hex tile that doesn't exist");
    //    }
    //}
}

//public struct TileValues 
//{
//    public TileValues(bool walkable, Material material)
//    {
//        this.walkable = walkable;
//        this.material = material;
//    }
//    public bool walkable;
//    public Material material;
//}
