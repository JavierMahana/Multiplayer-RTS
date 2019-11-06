using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;
using FixMath.NET;

public class MapEditor : OdinEditorWindow
{
    public string pathOfHexTileDatas = "Hex Tile Data";
    [Required(ErrorMessage = "The map editor requires a map to function!", MessageType = InfoMessageType.Warning)]
    public Map editingMap;
    [ShowIf("mapSelected")]
    public EditorHexData defaultTileData;

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

        Vector2 tileSize = editingMap.mapScale;

        GameObject templateObject = new GameObject("template", typeof(EditorHexTile));
        templateObject.transform.localScale = new Vector3(tileSize.x, tileSize.y);
        var mesh = templateObject.GetComponent<MeshFilter>().sharedMesh = MeshUtils.QuadMesh;

        Layout hexLayout = MapUtilities.GetHexLayout(tileSize, mesh, new Vector2(0, 0));

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


        Dictionary<Hex, Material> hexMaterials = new Dictionary<Hex, Material>();
        Dictionary<Hex, bool> hexWalkableFlags = new Dictionary<Hex, bool>();

        foreach (var tile in activeEditorHexTiles)
        {
            Hex tileHex = tile.Key;
            Material tileMaterial = tile.Value.data.hexMaterial;
            bool tileWalkableFlag = tile.Value.data.walkable;

            hexMaterials.Add(tileHex, tileMaterial);
            hexWalkableFlags.Add(tileHex, tileWalkableFlag);
        }

        editingMap.InitMap(hexMaterials, hexWalkableFlags);
        EditorUtility.SetDirty(editingMap);
    }


    [ShowIf("mapSelected")]
    [ShowIf("defaultTileDataAssigned")]
    [Button]
    public void CreateNewMap(int width, int height)
    {
        Vector2 tileSize = editingMap.mapScale;
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

        ClearEditorMap();

        GameObject templateObject = new GameObject("template", typeof(EditorHexTile));
        templateObject.transform.localScale = new Vector3(tileSize.x, tileSize.y);
        var mesh = templateObject.GetComponent<MeshFilter>().sharedMesh = MeshUtils.QuadMesh;
        
        Layout hexLayout = MapUtilities.GetHexLayout(tileSize, mesh, new Vector2(0, 0));


        for (int r = 0; r < height; r++)
        {
            int r_offset = Mathf.FloorToInt(r / 2); // or r>>1
            for (int q = -r_offset; q < width - r_offset; q++)
            {
                Hex hex = new Hex(q, r, -q - r);
                CreateAndStoreNewEditorHexTile(hex, templateObject, hexLayout, defaultTileData);
            }
        }
        GameObject.DestroyImmediate(templateObject);
    }


    private void CreateAndStoreNewEditorHexTile(Hex hex, GameObject template, Layout hexLayout, EditorHexData data)
    {
        FixVector2 pos = hexLayout.HexToWorld(hex);

        GameObject newHex = Instantiate(template);
        newHex.transform.position = new Vector3((float)pos.x, (float)pos.y);

        var currentTile = newHex.GetComponent<EditorHexTile>();
        currentTile.Init(this, data);

        activeEditorHexTiles.Add(hex, currentTile);
    }


    [MenuItem("Editor tools/Map Editor")]
    private static void OpenWindow()
    {
        var window = GetWindow<MapEditor>();
        window.position = GUIHelper.GetEditorWindowRect().AlignCenter(300, 400);
    }
}
