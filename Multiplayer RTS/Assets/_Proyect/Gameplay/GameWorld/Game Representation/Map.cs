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
    [SerializeField][HideInInspector]
    public Vector2Int mapProportions { get; private set; }
    [SerializeField][HideInInspector]
    public Dictionary<Hex, Material> HexMaterials { get; private set; }

    [SerializeField][HideInInspector]
    public Dictionary<Hex, Sprite> HexSprites { get; private set; }
    [SerializeField][HideInInspector]
    public Dictionary<Hex, bool> HexWalkableFlags { get; private set; }
    [SerializeField][HideInInspector]
    public Dictionary<Hex, MapHeight> HexHeights { get; private set; }
    [SerializeField][HideInInspector]
    public Dictionary<Hex, SlopeData> HexSlopeDatas { get; private set; }



    [SerializeField][HideInInspector]
    public Dictionary<Hex, Material> MiscMaterials { get; private set; }
    [SerializeField][HideInInspector]
    public Dictionary<Hex, ResourceSpotData> ResourceSpots { get; private set; }

    public void InitMap(Dictionary<Hex, Material> hexMaterials, Dictionary<Hex, Sprite> hexSprites, Dictionary<Hex, bool> hexWalkableFlags,
                        Dictionary<Hex, MapHeight> hexHeights, Dictionary<Hex,SlopeData> hexSlopeDatas, Vector2Int mapProportions)
    {
        HexMaterials     = new Dictionary<Hex, Material>(hexMaterials);
        HexSprites       = new Dictionary<Hex, Sprite>(hexSprites);
        HexWalkableFlags = new Dictionary<Hex, bool>(hexWalkableFlags);
        HexHeights       = new Dictionary<Hex, MapHeight>(hexHeights);
        HexSlopeDatas    = new Dictionary<Hex, SlopeData>(hexSlopeDatas);

        this.mapProportions = mapProportions;
    }
    
    public void InitMiscs(Dictionary<Hex, Material> MiscMaterials, Dictionary<Hex, ResourceSpotData> ResourceSpots) 
    {
        this.MiscMaterials = new Dictionary<Hex, Material>(MiscMaterials);
        this.ResourceSpots = new Dictionary<Hex, ResourceSpotData>(ResourceSpots);
    }

    [Button]
    private void DebugDictionaries()
    {
        if(HexMaterials != null)
            Debug.Log($"HexMaterials count {HexMaterials.Count}");
        if(HexSprites != null)
            Debug.Log($"HexSprites count {HexSprites.Count}");
        if(HexWalkableFlags != null)
            Debug.Log($"HexWalkableFlags count {HexWalkableFlags.Count}");
        if(HexHeights != null)
            Debug.Log($"HexHeights count {HexHeights.Count}");
        if(HexSlopeDatas != null)
            Debug.Log($"HexSlopeDatas count {HexSlopeDatas.Count}");
    }
    [Button]
    private void DebugMiscDictionaries()
    {
        Debug.Log($"MiscMaterials count {MiscMaterials.Count}");
        Debug.Log($"ResourceSpots count {ResourceSpots.Count}");
    }
}
