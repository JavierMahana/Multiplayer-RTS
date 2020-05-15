using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Rendering;
using FixMath.NET;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine.Rendering;


//el mapa va a ser creado con hex positions. así las pocisiones son coherentes.
public class MapManager : SerializedMonoBehaviour
{
    [SerializeField]
    private bool debug = false;
    public Transform originPoint;
    public Map[] maps;
    public int mapToLoad;
    [Tooltip("If you want to scale all the sprites that make the map, change this value")]
    public Vector2 mapScale =Vector2.one;

    public static ActiveMap ActiveMap { get; private set; }

    private void Awake()
    {
        if (ActiveMap == null)
        {
            LoadMap(mapToLoad);
        }
    }
    private void Start()
    {
#if UNITY_EDITOR
        //HideTempObjectsInPlayMode();
#endif
    }

    #region Editor maps LO UNICO QUE HACE ES LLAMAR LOADMAP!!!
#if UNITY_EDITOR

    [Button]
    [HideIf("onPlayMode")]
    private void LoadMapInEditor(int index)
    {
        LoadMap(index);
    }
    ////falta poner como dirty el so, para poder borrar las weas visuales dsps.

    ////la idea es implementar esto para hacer mas facil la pega de ver el mapa y poder poner unidades creadas con GO
    //[Button]
    //[HideIf("onPlayMode")]
    //[ShowIf("isScriptalbeObjectAssigned")]
    //private void LoadTempMapForEditor(int index)
    //{
    //    if (areLoadedTempMapObjects)
    //    {
    //        DestroyTempMapObjects();
    //    }
    //    Debug.Assert(index < maps.Length, "Invalid index! the maps array doesn't have any value in that spot", this);
    //    Map map = maps[index];
    //    Debug.Assert(map != null, $"Trying to acces to a map spot that doesn't contain any map. Check if the index that you used: {index} have any value in the maps array", this);


    //    Vector2 tileSize = map.mapScale;

    //    GameObject templateObject = new GameObject("template", typeof(MeshFilter), typeof(MeshRenderer));
    //    templateObject.transform.localScale = new Vector3(tileSize.x, tileSize.y);
    //    var mesh = templateObject.GetComponent<MeshFilter>().sharedMesh = MeshUtils.QuadMesh;

    //    Layout hexLayout = MapUtilities.GetHexLayout(tileSize, mesh, new Vector2(originPoint.position.x, originPoint.position.y), map.hexSizeOffSet);

    //    foreach (var pair in map.HexWalkableFlags)
    //    {
    //        Hex hex = pair.Key;
    //        bool walkableFlag = pair.Value;
    //        Material material = map.HexMaterials[hex];

    //        var current = GameObject.Instantiate(templateObject);
    //        var pos2D = hexLayout.HexToWorld(hex);
    //        current.transform.position = new Vector3((float)pos2D.x, (float)pos2D.y, hex.r + 10);
    //        current.transform.SetParent(originPoint, true);
    //        current.GetComponent<MeshRenderer>().sharedMaterial = material;

    //        loadedTempMapObjects.Objects.Add(current);
    //    }
    //    GameObject.DestroyImmediate(templateObject.gameObject);

    //    EditorUtility.SetDirty(loadedTempMapObjects);

    //    //create gameobjects at the places where 
    //}
    //[Button]
    //[ShowIf("areLoadedTempMapObjects")]
    //[HideIf("onPlayMode")]
    //private void DestroyTempMapObjects()
    //{
    //    if (Application.isPlaying)
    //    {
    //        Debug.LogWarning("you can't destroy the temporal map objects in play mode. do it on edit mode instead!");
    //        return;
    //    }
    //    else if (!areLoadedTempMapObjects)
    //    {
    //        Debug.LogWarning("There are not any loaded temp map objects to destroy!",this);
    //        return;
    //    }
    //    foreach (var tempObj in loadedTempMapObjects.Objects)
    //    {
    //        GameObject.DestroyImmediate(tempObj);
    //    }
    //    loadedTempMapObjects.Objects.Clear();
    //    EditorUtility.SetDirty(loadedTempMapObjects);
    //}
    //private void HideTempObjectsInPlayMode()
    //{
    //    if (areLoadedTempMapObjects)
    //    {
    //        foreach (var tempObj in loadedTempMapObjects.Objects)
    //        {
    //            tempObj.SetActive(false);
    //        }
    //    }
    //}

    private bool onPlayMode { get => Application.isPlaying; }
    //private bool isScriptalbeObjectAssigned { get => loadedTempMapObjects != null; }
    //private bool areLoadedTempMapObjects { get => loadedTempMapObjects.Objects != null && loadedTempMapObjects.Objects.Count > 0 && loadedTempMapObjects != null; }
    //public GameObjectCollection loadedTempMapObjects;
    ////private List<GameObject> loadedTempMapObjects = new List<GameObject>();
#endif
    #endregion

    #region PlayMode Maps
    /// <summary>
    /// sets the Active map and also it creates the entities that render the map. It uses the first map of the array
    /// </summary>
    public void LoadMap()
    {
        LoadMap(0);
    }
    /// <summary>
    /// sets the Active map and also it creates the entities that render the map.
    /// </summary>
    public void LoadMap(int index)
    {

        //if (!Application.isPlaying && editorMap == false)
        //{
        //    Debug.LogWarning("Loading a play mode map on the editor!!!", this);
        //    return;
        //}
        //else if (Application.isPlaying && editorMap == true)
        //{
        //    Debug.LogWarning("Loading a editor map on play mode!!!", this);
        //    return;
        //}

        Debug.Assert(index < maps.Length, "Invalid index! the maps array doesn't have any value in that spot", this);
        Map map = maps[index];
        Debug.Assert(map != null, $"Trying to acces to a map spot that doesn't contain any map. Check if the index that you used: {index} have any value in the maps array", this);

        ClearMapObjects();

        var origin = new FixVector2((Fix64)originPoint.position.x, (Fix64)originPoint.position.y);
        var hexSize = new FixVector2((Fix64)(map.spriteArtMapScale.x * mapScale.x), (Fix64)(map.spriteArtMapScale.y * mapScale.y));//scañed by the value int this object!!


        var layout = new Layout(Orientation.pointy, hexSize, origin);
        var runtimeMap = new RuntimeMap(map);

        ActiveMap = new ActiveMap(runtimeMap, layout);

        CreateMapVisuals(map, layout);

        //throw new System.NotImplementedException(); //ClearMapGO();
        //ClearMapEntities(entityManager);
        

        //Debug.Assert(index < maps.Length, "Invalid index! the maps array doesn't have any value in that spot", this);
        //Map map = maps[index];
        //Debug.Assert(map != null, $"Trying to acces to a map spot that doesn't contain any map. Check if the index that you used: {index} have any value in the maps array", this);

        //Vector3 quadExtents = MeshUtils.QuadMesh.bounds.extents;
        //FixVector2 origin = new FixVector2((Fix64)originPoint.position.x, (Fix64)originPoint.position.y);
        //FixVector2 hexSize = new FixVector2(((Fix64)map.mapScale.x * (Fix64)quadExtents.x) + (Fix64)map.hexSizeOffSet.x, ((Fix64)map.mapScale.y * (Fix64)quadExtents.y) + (Fix64)map.hexSizeOffSet.y);

        //var layout = new Layout(Orientation.pointy, hexSize, origin);
        //var runtimeMap = new RuntimeMap(map);
        //ActiveMap = new ActiveMap(runtimeMap, layout);

        //CreateMapVisuals(entityManager, map, layout);        
    }
    private void ClearMapObjects()
    {
        if (debug) Debug.Log("destroying the existing hex-tile entities.");
        var tileObjects = FindObjectsOfType<HexTileVisual>();
        foreach (var tile in tileObjects)
        {
            Destroy(tile);
        }
    }

    //private void ClearMapEntities(EntityManager entityManager)
    //{
    //    if (debug) Debug.Log("destroying the existing hex-tile entities.");
    //    EntityQuery hexTileQuerry = entityManager.CreateEntityQuery(
    //        typeof(HexTile));
    //    entityManager.DestroyEntity(hexTileQuerry);
    //}
    /// <summary>
    /// creates all the map terrain visuals for the given map 
    /// </summary>
    /// 
    private void CreateMapVisuals(Map map, Layout layout)
    {
        var camera = Camera.main;
        int minZValue = Mathf.CeilToInt(camera.transform.position.z + camera.nearClipPlane + 1);

        var mapRoot = new GameObject("Map Root");
        mapRoot.transform.position = Vector3.zero;

        foreach (var HexSpriteValue in map.HexSprites)
        {


            Hex currHex = HexSpriteValue.Key;
            var currSprite = HexSpriteValue.Value;


            var currentTileObject = new GameObject($"tile {currHex}", typeof(SpriteRenderer), typeof(HexTileVisual));
            var meshRenderer = currentTileObject.GetComponent<SpriteRenderer>();
            var hexTileVisual = currentTileObject.GetComponent<HexTileVisual>();
            var objectTransform = currentTileObject.transform;

            var worldPos = layout.HexToWorld(currHex);
            objectTransform.position = new Vector3((float)worldPos.x, (float)worldPos.y, minZValue + currHex.r);
            objectTransform.localScale = mapScale;
            objectTransform.SetParent(mapRoot.transform, true);

            meshRenderer.sprite = currSprite;
            hexTileVisual.Hex = currHex;
        }
        if (debug) Debug.Log($"the map visuals for the map: {map.name} have been created with {map.HexMaterials.Count} tiles in total");
    }
    //private void CreateMapVisuals(EntityManager entityManager, Map map, Layout layout)
    //{
    //    var tileArchetype = entityManager.CreateArchetype(typeof(HexTile), typeof(HexPosition));


    //    foreach (var HexMaterialPairs in map.HexMaterials)
    //    {
    //        Hex currHex = HexMaterialPairs.Key;
    //        Material currMaterial = HexMaterialPairs.Value;


    //        CreateHexVisualEntity(entityManager, tileArchetype, currHex, currMaterial, layout, map.mapScale);
    //    }
    //    if (debug) Debug.Log($"the map visuals for the map: {map.name} have been created with {map.HexMaterials.Count} tiles in total");
    //}
    ///// <summary>
    /// creates the render entity of one hex with the given material
    /// </summary>
    //private void CreateHexVisualEntity(EntityManager entityManager, EntityArchetype tileArchetype, Hex hex, Material hexMaterial, Layout layout, Vector2 mapScale)
    //{
    //    Entity tileEntity = entityManager.CreateEntity(tileArchetype);
    //    entityManager.SetSharedComponentData(tileEntity, new RenderMesh() { mesh = MeshUtils.QuadMesh, material = hexMaterial });
    //    entityManager.SetComponentData(tileEntity, new NonUniformScale()
    //    {
    //        Value = new float3(mapScale.x, mapScale.y, 1)
    //    });

    //    FixVector2 position = layout.HexToWorld(hex);
    //    entityManager.SetComponentData(tileEntity, new HexPosition() { HexCoordinates = (FractionalHex)hex });
    //}
    #endregion

}
