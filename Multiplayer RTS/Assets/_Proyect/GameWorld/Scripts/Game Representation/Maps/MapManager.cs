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


//el mapa va a ser creado con hex positions. así las pocisiones son coherentes.
public class MapManager : SerializedMonoBehaviour
{

    [SerializeField]
    private bool debug = false;
    public Transform originPoint;
    public Map[] maps;
    public int mapToLoad;
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
        HideTempObjectsInPlayMode();
    }

    #region Editor maps
    //falta poner como dirty el so, para poder borrar las weas visuales dsps.

    //la idea es implementar esto para hacer mas facil la pega de ver el mapa y poder poner unidades creadas con GO
    [Button]
    [HideIf("onPlayMode")]
    [ShowIf("isScriptalbeObjectAssigned")]
    private void LoadTempMapForEditor(int index)
    {
        if (areLoadedTempMapObjects)
        {
            DestroyTempMapObjects();
        }
        Debug.Assert(index < maps.Length, "Invalid index! the maps array doesn't have any value in that spot", this);
        Map map = maps[index];
        Debug.Assert(map != null, $"Trying to acces to a map spot that doesn't contain any map. Check if the index that you used: {index} have any value in the maps array", this);


        Vector2 tileSize = map.mapScale;

        GameObject templateObject = new GameObject("template", typeof(MeshFilter), typeof(MeshRenderer));
        templateObject.transform.localScale = new Vector3(tileSize.x, tileSize.y);
        var mesh = templateObject.GetComponent<MeshFilter>().sharedMesh = MeshUtils.QuadMesh;

        Layout hexLayout = MapUtilities.GetHexLayout(tileSize, mesh, new Vector2(originPoint.position.x, originPoint.position.y));

        foreach (var pair in map.HexWalkableFlags)
        {
            Hex hex = pair.Key;
            bool walkableFlag = pair.Value;
            Material material = map.HexMaterials[hex];

            var current = GameObject.Instantiate(templateObject);
            var pos2D = hexLayout.HexToWorld(hex);
            current.transform.position = new Vector3((float)pos2D.x, (float)pos2D.y, HexTranslationSystem.MAP_Z_VALUE);
            current.transform.SetParent(originPoint, true);
            current.GetComponent<MeshRenderer>().sharedMaterial = material;

            loadedTempMapObjects.Objects.Add(current);
        }
        GameObject.DestroyImmediate(templateObject.gameObject);

        EditorUtility.SetDirty(loadedTempMapObjects);
        
        //create gameobjects at the places where 
    }
    [Button]
    [ShowIf("areLoadedTempMapObjects")]
    [HideIf("onPlayMode")]
    private void DestroyTempMapObjects()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning("you can't destroy the temporal map objects in play mode. do it on edit mode instead!");
            return;
        }
        else if (!areLoadedTempMapObjects)
        {
            Debug.LogWarning("There are not any loaded temp map objects to destroy!",this);
            return;
        }
        foreach (var tempObj in loadedTempMapObjects.Objects)
        {
            GameObject.DestroyImmediate(tempObj);
        }
        loadedTempMapObjects.Objects.Clear();
        EditorUtility.SetDirty(loadedTempMapObjects);
    }
    private void HideTempObjectsInPlayMode()
    {
        if (areLoadedTempMapObjects)
        {
            foreach (var tempObj in loadedTempMapObjects.Objects)
            {
                tempObj.SetActive(false);
            }
        }
    }

    private bool onPlayMode { get => Application.isPlaying; }
    private bool isScriptalbeObjectAssigned { get => loadedTempMapObjects != null; }
    private bool areLoadedTempMapObjects { get => loadedTempMapObjects.Objects != null && loadedTempMapObjects.Objects.Count > 0 && loadedTempMapObjects != null; }
    public GameObjectCollection loadedTempMapObjects;
    //private List<GameObject> loadedTempMapObjects = new List<GameObject>();
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
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Can't load a map on the editor", this);
            return;
        }
        var entityManager = World.Active.EntityManager;
        ClearMapEntities(entityManager);

        Debug.Assert(index < maps.Length, "Invalid index! the maps array doesn't have any value in that spot", this);
        Map map = maps[index];
        Debug.Assert(map != null, $"Trying to acces to a map spot that doesn't contain any map. Check if the index that you used: {index} have any value in the maps array", this);

        Vector3 quadExtents = MeshUtils.QuadMesh.bounds.extents;
        FixVector2 origin = new FixVector2((Fix64)originPoint.position.x, (Fix64)originPoint.position.y);
        FixVector2 hexSize = new FixVector2(((Fix64)map.mapScale.x * (Fix64)quadExtents.x), ((Fix64)map.mapScale.y * (Fix64)quadExtents.y));

        var layout = new Layout(Orientation.pointy, hexSize, origin);


        CreateMapVisuals(entityManager, map, layout);

        var runtimeMap = new RuntimeMap(map);
        ActiveMap = new ActiveMap(runtimeMap, layout);
        
    }
    private void ClearMapEntities(EntityManager entityManager)
    {
        if (debug) Debug.Log("destroying the existing hex-tile entities.");
        EntityQuery hexTileQuerry = entityManager.CreateEntityQuery(
            typeof(LocalToWorld), typeof(NonUniformScale), typeof(RenderMesh), typeof(Translation), typeof(HexTile));
        entityManager.DestroyEntity(hexTileQuerry);
    }
    /// <summary>
    /// creates all the map terrain visuals for the given map 
    /// </summary>
    private void CreateMapVisuals(EntityManager entityManager, Map map, Layout layout)
    {
        var tileArchetype = entityManager.CreateArchetype(typeof(LocalToWorld), typeof(NonUniformScale), typeof(RenderMesh), typeof(Translation), typeof(HexTile), typeof(HexPosition));


        foreach (var HexMaterialPairs in map.HexMaterials)
        {
            Hex currHex = HexMaterialPairs.Key;
            Material currMaterial = HexMaterialPairs.Value;

            CreateHexVisualEntity(entityManager, tileArchetype, currHex, currMaterial, layout, map.mapScale);
        }
        if (debug) Debug.Log($"the map visuals for the map: {map.name} have been created with {map.HexMaterials.Count} tiles in total");
    }
    /// <summary>
    /// creates the render entity of one hex with the given material
    /// </summary>
    private void CreateHexVisualEntity(EntityManager entityManager, EntityArchetype tileArchetype, Hex hex, Material hexMaterial, Layout layout, Vector2 mapScale)
    {
        Entity tileEntity = entityManager.CreateEntity(tileArchetype);
        entityManager.SetSharedComponentData(tileEntity, new RenderMesh() { mesh = MeshUtils.QuadMesh, material = hexMaterial });
        entityManager.SetComponentData(tileEntity, new NonUniformScale()
        {
            Value = new float3(mapScale.x, mapScale.y, 1)
        });

        FixVector2 position = layout.HexToWorld(hex);
        entityManager.SetComponentData(tileEntity, new HexPosition() { HexCoordinates = (FractionalHex)hex });
        //entityManager.SetComponentData(tileEntity, new Translation() { Value = new float3((float)position.x, (float)position.y, 15) });
    }
    #endregion

}
