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

//1°"Initiailize": get all the "EditorHexTile" from the resourses forlder with the "pathOfHexTileDatas" path. Then it populates the "DataLookUpTable"
public class MapEditor : OdinEditorWindow
{
    public string pathOfHexTileDatas = "Hex Tile Data";
    [Required(ErrorMessage = "The map editor requires a map to function!", MessageType = InfoMessageType.Warning)]
    public Map editingMap;
    [ShowIf("mapSelected")]
    public EditorHexData defaultTileData;
    private Vector2Int mapProportions;


    private bool mapSelected { get => editingMap != null; }
    private bool defaultTileDataAssigned { get => defaultTileData != null; }
    private bool editingMapHaveValues 
    { 
        get
        {
            if (editingMap == null) return false;
            else return (editingMap.HexMaterials != null && editingMap.HexWalkableFlags != null);
        }
    }
    private bool editorMapIsCreatedAndExist { get => activeEditorHexTiles != null && activeEditorHexTiles.Count != 0; }

    private Dictionary<Hex, EditorHexTile> activeEditorHexTiles = null;
    [HideInInspector]
    [Tooltip("This lookup table links a material with it's related EditorHexData. It gets initialized by searching in the resources folder for the EditorHexData.")]
    public Dictionary<Material, EditorHexData> DataLookUpTable = null;


    protected override void Initialize()
    {
        base.Initialize();
        ClearEditorMap();
        
        EditorHexData[] allHexDatas = Resources.LoadAll<EditorHexData>(pathOfHexTileDatas);
        Debug.Assert(allHexDatas != null || allHexDatas.Length != 0, $"There are no Editor Hex Datas in the path: Resources/{pathOfHexTileDatas}");

        DataLookUpTable = new Dictionary<Material, EditorHexData>();
        activeEditorHexTiles = new Dictionary<Hex, EditorHexTile>();

        foreach (var hexData in allHexDatas)
        {
            DataLookUpTable.Add(hexData.hexMaterial, hexData);
        }
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        ClearEditorMap();
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
    [Button]
    public void LoadMap()
    {
        Debug.Assert(editingMapHaveValues, "To load a map, it must have values!");
        ClearEditorMap();

        Vector2 tileSize = editingMap.materialArtMapScale;

        GameObject templateObject = new GameObject("template", typeof(EditorHexTile));
        //templateObject.transform.localScale = new Vector3(tileSize.x, tileSize.y);
        templateObject.GetComponent<MeshFilter>().sharedMesh = MeshUtils.QuadMesh;

        Layout hexLayout = new Layout(Orientation.pointy, new FixVector2((Fix64)tileSize.x, (Fix64)tileSize.y), new FixVector2(0,0));//MapUtilities.GetHexLayout(tileSize, mesh, new Vector2(0, 0), editingMap.hexSizeOffSet);

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
    [ShowIf("editorMapIsCreatedAndExist")]
    public void SaveMap()
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


        Dictionary<Hex, Material> hexMaterials   = new Dictionary<Hex, Material>();
        Dictionary<Hex, Sprite> hexSprites       = new Dictionary<Hex, Sprite>();
        Dictionary<Hex, bool> hexWalkableFlags   = new Dictionary<Hex, bool>();
        Dictionary<Hex, MapHeight> hexHeights    = new Dictionary<Hex, MapHeight>();
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

            hexMaterials.Add    (tileHex, tileMaterial);
            hexSprites.Add      (tileHex, tileSprite);
            hexWalkableFlags.Add(tileHex, tileWalkableFlag);
            hexHeights.Add      (tileHex, tileHeight);
            hexSlopeDatas.Add   (tileHex, slopeData);
        }

        editingMap.InitMap(hexMaterials, hexSprites, hexWalkableFlags, hexHeights, hexSlopeDatas, mapProportions);
        EditorUtility.SetDirty(editingMap);
    }


    [ShowIf("mapSelected")]
    [ShowIf("defaultTileDataAssigned")]
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

        
        Layout hexLayout = new Layout(Orientation.pointy, new FixVector2((Fix64)tileSize.x, (Fix64)tileSize.y), new FixVector2(0,0));//(MapUtilities.GetHexLayout(tileSize, mesh, new Vector2(0, 0), editingMap.hexSizeOffSet);


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
}
#endif