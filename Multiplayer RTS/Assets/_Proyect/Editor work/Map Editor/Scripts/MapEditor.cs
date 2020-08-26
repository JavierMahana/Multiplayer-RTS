 #if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;
using FixMath.NET;

/// <summary>
/// This editor have two phases to create a map.
/// First the geograsphic phase: hieght, slope and if walkable data.
/// Then the Misc phase: resources and (starting places of teams) todo
/// </summary>
public class MapEditor : OdinEditorWindow
{
    public string pathOfHexTileDatas = "Hex Tile Data";
    public string pathOfMiscDatas = "Misc Tile Data";

    [Required(ErrorMessage = "The map editor requires a map to function!", MessageType = InfoMessageType.Warning)]
    public Map editingMap;


    [ShowIf("mapSelected")]
    [HideIf("miscEditorActivated")]
    public EditorHexData defaultTileData;

    [ShowIf("miscEditorActivated")]
    public EditorMiscHexData defaultMiscData;

    [ShowIf("editingMapHaveValues")]
    [HideIf("editorMapIsCreatedAndExist")]
    public bool toggleMiscMapEditor = false;



    private Vector2Int mapProportions;
    private Dictionary<Hex, EditorHexTile> activeEditorHexTiles = null;
    private Dictionary<Hex, EditorMiscHexTile> activeEditorMiscTiles = null;

    ///This lookup table links a material with it's related EditorHexData. It gets initialized by searching in the resources folder for the EditorHexData. And it is used in the
    ///editor monobehaviours.
    [HideInInspector]
    public Dictionary<Material, EditorHexData> DataLookUpTable = null;
    [HideInInspector]
    public Dictionary<Material, EditorMiscHexData> MiscDataLookUpTable = null;

    /// <summary>
    /// background objects that make of the misc map.
    /// </summary>
    private List<GameObject> spriteObjForMiscMap = new List<GameObject>();


    //1°"Initiailize": get all the "EditorHexTile" from the resourses forlder with the "pathOfHexTileDatas" path. Then it populates the "DataLookUpTable".Same with the misc ones.
    protected override void Initialize()
    {
        base.Initialize();
        ClearEditorMap();

        toggleMiscMapEditor = false;

        EditorHexData[] allHexDatas = Resources.LoadAll<EditorHexData>(pathOfHexTileDatas);
        Debug.Assert(allHexDatas != null || allHexDatas.Length != 0, $"There are no Editor Hex Datas in the path: Resources/{pathOfHexTileDatas}");

        DataLookUpTable = new Dictionary<Material, EditorHexData>();
        activeEditorHexTiles = new Dictionary<Hex, EditorHexTile>();

        foreach (var hexData in allHexDatas)
        {
            DataLookUpTable.Add(hexData.hexMaterial, hexData);
        }

        EditorMiscHexData[] allMiscDatas = Resources.LoadAll<EditorMiscHexData>(pathOfMiscDatas);
        Debug.Assert(allMiscDatas != null || allMiscDatas.Length != 0, $"There are no Editor misc Datas in the path: Resources/{pathOfMiscDatas}");

        MiscDataLookUpTable = new Dictionary<Material, EditorMiscHexData>();
        activeEditorMiscTiles = new Dictionary<Hex, EditorMiscHexTile>();

        foreach (var misc in allMiscDatas)
        {
            MiscDataLookUpTable.Add(misc.hexMaterial, misc);
        }
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        ClearEditorMap();
        ClearMiscMap();
    }






    [Button]
    [ShowIf("editorMapIsCreatedAndExist")]
    public void ClearEditorMap()
    {
        if (activeEditorHexTiles == null) return;
        foreach (var entry in activeEditorHexTiles)
        {
            GameObject.DestroyImmediate(entry.Value.gameObject);
        }
        activeEditorHexTiles.Clear();
    }

    [ShowIf("editingMapHaveValues")]
    [HideIf("editorMapIsCreatedAndExist")]
    [HideIf("miscEditorActivated")]
    [Button]
    public void LoadGeoMap()
    {
        Debug.Assert(editingMapHaveValues, "To load a map, it must have values!");
        ClearEditorMap();

        Vector2 tileSize = editingMap.materialArtMapScale;

        GameObject templateObject = new GameObject("template", typeof(EditorHexTile));
        //templateObject.transform.localScale = new Vector3(tileSize.x, tileSize.y);
        templateObject.GetComponent<MeshFilter>().sharedMesh = MeshUtils.QuadMesh;

        Layout hexLayout = new Layout(Orientation.pointy, new FixVector2((Fix64)tileSize.x, (Fix64)tileSize.y), new FixVector2(0, 0));//MapUtilities.GetHexLayout(tileSize, mesh, new Vector2(0, 0), editingMap.hexSizeOffSet);

        foreach (var pair in editingMap.HexWalkableFlags)
        {
            Hex hex = pair.Key;
            bool walkableFlag = pair.Value;
            Material material = editingMap.HexMaterials[hex];

            EditorHexData data;
            if (DataLookUpTable.TryGetValue(material, out data))
            {
                CreateAndStoreNewEditorHexTile(hex, templateObject, hexLayout, data);
            }
            else { Debug.LogError($"invalid material: {material} when Loading map. In the hex: {hex}"); }
        }
        GameObject.DestroyImmediate(templateObject.gameObject);
    }

    [Button]
    [InfoBox("Saving a new geo map might cause that the misc map becomes obsolete!", "miscMapHaveValues", InfoMessageType = InfoMessageType.Warning)]
    [ShowIf("editorMapIsCreatedAndExist")]
    [HideIf("miscEditorActivated")]
    public void SaveGeoMap()
    {
        if (activeEditorHexTiles == null)
        {
            Debug.LogError("Unable to Save");
            return;
        }
        else if (activeEditorHexTiles.Count == 0)
        {
            Debug.LogError("Unable to Save");
            return;
        }


        Dictionary<Hex, Material> hexMaterials = new Dictionary<Hex, Material>();
        Dictionary<Hex, Sprite> hexSprites = new Dictionary<Hex, Sprite>();
        Dictionary<Hex, bool> hexWalkableFlags = new Dictionary<Hex, bool>();
        Dictionary<Hex, MapHeight> hexHeights = new Dictionary<Hex, MapHeight>();
        Dictionary<Hex, SlopeData> hexSlopeDatas = new Dictionary<Hex, SlopeData>();

        foreach (var tile in activeEditorHexTiles)
        {
            Hex tileHex = tile.Key;
            var tileData = tile.Value.data;
            Material tileMaterial = tileData.hexMaterial;
            Debug.Assert(tileMaterial != null, "Material null on the editor hex data", tileData);
            Sprite tileSprite = tileData.sprite;
            Debug.Assert(tileSprite != null, "Sprite null on the editor hex data", tileData);
            bool tileWalkableFlag = tileData.walkable;
            MapHeight tileHeight = tileData.heightLevel;
            SlopeData slopeData = new SlopeData()
            {
                isSlope = tileData.isSlope,
                heightSide_0tr = tileData.heightSide_TopRight,
                heightSide_1r = tileData.heightSide_Right,
                heightSide_2dr = tileData.heightSide_DownRight,
                heightSide_3dl = tileData.heightSide_DownLeft,
                heightSide_4l = tileData.heightSide_Left,
                heightSide_5tl = tileData.heightSide_TopLeft
            };

            hexMaterials.Add(tileHex, tileMaterial);
            hexSprites.Add(tileHex, tileSprite);
            hexWalkableFlags.Add(tileHex, tileWalkableFlag);
            hexHeights.Add(tileHex, tileHeight);
            hexSlopeDatas.Add(tileHex, slopeData);
        }

        editingMap.InitMap(hexMaterials, hexSprites, hexWalkableFlags, hexHeights, hexSlopeDatas, mapProportions);
        //var serializedObj = new UnityEditor.SerializedObject(editingMap);
        //serializedObj.ApplyModifiedProperties();
        EditorUtility.SetDirty(editingMap);
    }


    [ShowIf("mapSelected")]
    [ShowIf("defaultTileDataAssigned")]
    [HideIf("miscEditorActivated")]
    [Button]
    public void CreateNewMap(int width, int height)
    {
        Vector2 tileSize = editingMap.materialArtMapScale;
        if (tileSize.x <= 0 || tileSize.y <= 0)
        {
            Debug.LogError("Invalid tile size! Check the map configs");
            return;
        }
        if (width <= 0 || height <= 0)
        {
            Debug.LogError("Invalid method input values!");
            return;
        }
        mapProportions = new Vector2Int(width, height);

        ClearEditorMap();

        GameObject templateObject = new GameObject("template", typeof(EditorHexTile));
        //templateObject.transform.localScale = new Vector3(tileSize.x, tileSize.y);
        var mesh = templateObject.GetComponent<MeshFilter>().sharedMesh = MeshUtils.QuadMesh;


        Layout hexLayout = new Layout(Orientation.pointy, new FixVector2((Fix64)tileSize.x, (Fix64)tileSize.y), new FixVector2(0, 0));//(MapUtilities.GetHexLayout(tileSize, mesh, new Vector2(0, 0), editingMap.hexSizeOffSet);


        for (int r = 0; r < height; r++)
        {
            int r_offset = Mathf.FloorToInt(r / 2); // or r>>1
            for (int q = -r_offset; q < width - r_offset; q++)
            {
                Hex hex = new Hex(q, r);
                CreateAndStoreNewEditorHexTile(hex, templateObject, hexLayout, defaultTileData);
            }
        }
        GameObject.DestroyImmediate(templateObject);
    }

    [ShowIf("miscEditorActivated")]
    [ShowIf("miscMapIsCreatedAndExist")]
    [Button]
    public void ClearMiscMap()
    {
        if (activeEditorMiscTiles == null) return;
        foreach (var entry in activeEditorMiscTiles)
        {
            GameObject.DestroyImmediate(entry.Value.gameObject);
        }
        activeEditorMiscTiles.Clear();

        if (spriteObjForMiscMap == null) return;
        foreach (var obj in spriteObjForMiscMap)
        {
            DestroyImmediate(obj);
        }
        spriteObjForMiscMap.Clear();

    }


    [ShowIf("miscEditorActivated")]
    [ShowIf("defaultMiscDataIsAssigned")]
    [HideIf("miscMapHaveValues")]
    [HideIf("miscMapIsCreatedAndExist")]
    [Button]
    public void CreateMiscMap() 
    {
        ClearMiscMap();

        var scale = editingMap.spriteArtMapScale;

        Layout layout = new Layout(Orientation.pointy, new FixVector2((Fix64)scale.x, (Fix64)scale.y), new FixVector2(0, 0));
        //llenar de sprites y de objetos con material 
        foreach (var keyValuePar in editingMap.HexSprites)
        {
            Hex hex = keyValuePar.Key;
            var sprite = keyValuePar.Value;

            CreateAndStoreMiscObjectsInHex(hex, layout, defaultMiscData, sprite);
        }
        

        //esta funcion debe:
        //spawnear una capa de visuales opacas atras(hecha de sprites) y luego spawnear una capa de objetos como los editor hex tile pero para los miscelaneos.
    }

    
    [ShowIf("miscEditorActivated")]
    [ShowIf("miscMapIsCreatedAndExist")]
    [Button]
    public void SaveMiscMap()
    {
        if (activeEditorMiscTiles == null)
        {
            Debug.LogError("Unable to Save");
            return;
        }
        else if (activeEditorMiscTiles.Count == 0)
        {
            Debug.LogError("Unable to Save");
            return;
        }

        var resourcesSpotsData = new Dictionary<Hex, ResourceSpotData>();
        var miscMaterials = new Dictionary<Hex, Material>();

        foreach (var keyValue in activeEditorMiscTiles)
        {
            Hex hex = keyValue.Key;
            var miscTile = keyValue.Value;
            var miscData = miscTile.data;

            miscMaterials.Add(hex, miscData.hexMaterial);
            if(!miscData.isEmpty && miscData.isResource)
                resourcesSpotsData.Add(hex, miscTile.data.resourceData);
        }
        editingMap.InitMiscs(miscMaterials, resourcesSpotsData);
        //var serializedObj = new UnityEditor.SerializedObject(editingMap);
        //serializedObj.ApplyModifiedProperties();
        EditorUtility.SetDirty(editingMap);
    }


    [ShowIf("miscEditorActivated")]
    [HideIf("miscMapIsCreatedAndExist")]
    [ShowIf("miscMapHaveValues")]
    [Button]
    public void LoadMiscMap()
    {
        ClearMiscMap();

        var scale = editingMap.spriteArtMapScale;
        Layout layout = new Layout(Orientation.pointy, new FixVector2((Fix64)scale.x, (Fix64)scale.y), new FixVector2(0, 0));
        //we use that dictionary only to get all the hexes of the map.
        foreach (var keyValue in editingMap.MiscMaterials)
        {
            

            Hex hex = keyValue.Key;
            Sprite bgSprite;
            if (editingMap.HexSprites.ContainsKey(hex))
            {
                bgSprite = editingMap.HexSprites[hex];
            }
            else 
            {
                bgSprite = null;
                Debug.LogError($"invalid hex: {hex}. It doesn't have the geo map created in there.");
                return;
            }
            var material = keyValue.Value;



            EditorMiscHexData data;
            if(MiscDataLookUpTable.TryGetValue(material, out data))
            {
                CreateAndStoreMiscObjectsInHex(hex, layout, data, bgSprite);
            }
            else
                Debug.LogError($"invalid material: {material} when Loading misc map. In the hex: {hex}");
        }
    }

    private void CreateAndStoreMiscObjectsInHex(Hex hex, Layout hexLayout, EditorMiscHexData data, Sprite backgroudSprite)
    {
        var backgroundGO = new GameObject("spriteBackGround", typeof(SpriteRenderer));

        var fixPos = hexLayout.HexToWorld(hex);
        var position = new Vector2((float)fixPos.x, (float)fixPos.y);
        backgroundGO.transform.position = new Vector3(position.x, position.y, position.y);

        var backgroundRenderer = backgroundGO.GetComponent<SpriteRenderer>();
        backgroundRenderer.sprite = backgroudSprite;

        spriteObjForMiscMap.Add(backgroundGO);



        var miscTileGO = new GameObject("Misc tile", typeof(EditorMiscHexTile));
        var miscTile = miscTileGO.GetComponent<EditorMiscHexTile>();
        miscTile.Init(this, data, hex);

        miscTileGO.GetComponent<MeshFilter>().sharedMesh = MeshUtils.QuadMesh;
        miscTileGO.GetComponent<MeshRenderer>().sharedMaterial = data.hexMaterial;
        //-1 in the Z.
        miscTileGO.transform.position = new Vector3(position.x, position.y, position.y - 1);

        activeEditorMiscTiles.Add(hex, miscTile);
    }

    private void CreateAndStoreNewEditorHexTile(Hex hex, GameObject template, Layout hexLayout, EditorHexData data)
    {
        FixVector2 pos = hexLayout.HexToWorld(hex);

        GameObject newHex = Instantiate(template);
        newHex.transform.position = new Vector3((float)pos.x, (float)pos.y, (float)pos.y);//the z axis is relative to the y axis

        var currentTile = newHex.GetComponent<EditorHexTile>();
        currentTile.Init(this, data, hex);

        activeEditorHexTiles.Add(hex, currentTile);
    }


    [MenuItem("Editor tools/Map Editor")]
    private static void OpenWindow()
    {
        var window = GetWindow<MapEditor>();
        window.position = GUIHelper.GetEditorWindowRect().AlignCenter(300, 400);
    }


    private bool mapSelected { get => editingMap != null; }
    private bool defaultTileDataAssigned { get => defaultTileData != null; }
    private bool editingMapHaveValues => editingMap == null ? false : editingMap.HexMaterials != null && editingMap.HexMaterials.Count > 0;
    private bool editorMapIsCreatedAndExist { get => activeEditorHexTiles != null && activeEditorHexTiles.Count != 0; }

    private bool miscEditorActivated => editingMapHaveValues && toggleMiscMapEditor && !editorMapIsCreatedAndExist;
    private bool miscMapIsCreatedAndExist => activeEditorMiscTiles != null && activeEditorMiscTiles.Count != 0;

    private bool defaultMiscDataIsAssigned { get => defaultMiscData != null; }
    /// <summary>
    /// only checks to see if the map have a resource spot dictionary.
    /// </summary>
    private bool miscMapHaveValues => editingMap == null ? false : editingMap.MiscMaterials != null && editingMap.MiscMaterials.Count > 0;  
}
#endif