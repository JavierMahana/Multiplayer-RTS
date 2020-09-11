using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Lean.Pool;

/// <summary>
/// esta clase esta de oyente para el map manager.
/// </summary>
public class FogOfWarManager : MonoBehaviour
{
    const int FOW_LAYER = 8;
    const int TEXTURE_SIZE = 256;
    const float FOG_Z_OFFSET = 1;

    [Required]
    public Shader gausianBlurShader;
    [Range(0,10)]
    public int filterIterations = 1;

    public bool showFOW = true;
    public MapManager mapManager;
    public Material fogOfWarMat;
    [Range(0,1)]
    public float exploredFogAlpha = 0.5f;
    //public SpriteRenderer fowVisionPrefab;
    public LeanGameObjectPool objectPool;

    public Vector2 borderOffset = Vector2.one;
    public Vector2Int mapAspectRatio = new Vector2Int(2,1);
    

    [HideInInspector]
    public Camera fogCamera;

    [HideInInspector]
    public Camera previousFogCamera;
    [HideInInspector]
    public MeshRenderer fogRenderer;

    

    private void Start()
    {
        if (mapManager == null)
            mapManager = FindObjectOfType<MapManager>();
        if (mapManager == null)
            throw new System.Exception("To use this component there needs to be a Map manager on the scene.");

        var activeMap = MapManager.ActiveMap;
        Debug.Assert(activeMap != null);


        var layout = activeMap.layout;

        int loadedMapIndex = mapManager.mapToLoad;
        Map loadedMap = mapManager.maps[loadedMapIndex];        
        var mapProportions = loadedMap.mapProportions;
        

        var mainCamera = Camera.main;
        var zFogCoord = mainCamera.transform.position.z + mainCamera.nearClipPlane + FOG_Z_OFFSET;

        var fogRect = layout.GetWorldCoordinatesRect(mapProportions, borderOffset, mapAspectRatio);



        //RENDER TEXTURE
        var fogRenderTexture = new RenderTexture(TEXTURE_SIZE * mapAspectRatio.x, TEXTURE_SIZE * mapAspectRatio.y, 0);
        var prevFogRenderTexture = new RenderTexture(TEXTURE_SIZE * mapAspectRatio.x, TEXTURE_SIZE * mapAspectRatio.y, 0);

        //PREVIOUS CAMERA
        var prevFogCameraGO = new GameObject("Previous Fog camera");
        prevFogCameraGO.transform.position = new Vector3(fogRect.center.x, fogRect.center.y, mainCamera.transform.position.z);
        var prevCameraFilter = prevFogCameraGO.AddComponent<CameraBlurFilter>();
        prevCameraFilter.blurShader = gausianBlurShader;
        prevCameraFilter.iterations = filterIterations;

        previousFogCamera = prevFogCameraGO.AddComponent<Camera>();
        previousFogCamera.forceIntoRenderTexture = true;
        previousFogCamera.targetTexture = prevFogRenderTexture;
        previousFogCamera.cullingMask = 1 << FOW_LAYER;
        previousFogCamera.orthographic = true;
        previousFogCamera.orthographicSize = fogRect.size.x * 0.5f / mapAspectRatio.x;
        previousFogCamera.clearFlags = CameraClearFlags.Nothing;
        previousFogCamera.backgroundColor = Color.black;
        //previousFogCamera.depth = -1;




        //CAMERA
        var fogCameraGO = new GameObject("Fog camera");
        fogCameraGO.transform.position = new Vector3(fogRect.center.x, fogRect.center.y, mainCamera.transform.position.z);
        var cameraFilter = fogCameraGO.AddComponent<CameraBlurFilter>();
        cameraFilter.blurShader = gausianBlurShader;
        cameraFilter.iterations = filterIterations;

        fogCamera = fogCameraGO.AddComponent<Camera>();
        fogCamera.forceIntoRenderTexture = true;
        fogCamera.targetTexture = fogRenderTexture;
        fogCamera.cullingMask = 1 << FOW_LAYER;
        fogCamera.orthographic = true;
        fogCamera.orthographicSize = fogRect.size.x * 0.5f / mapAspectRatio.x;
        fogCamera.clearFlags = CameraClearFlags.SolidColor;
        fogCamera.backgroundColor = Color.black;
        


        //MESH RENDERER AND FILTER
        var fogRendererGO = GameObject.CreatePrimitive(PrimitiveType.Quad);//new GameObject("Fog renderer");
        Destroy(fogRendererGO.GetComponent<MeshCollider>());

        //fogRendererGO.layer = FOW_LAYER;
        fogRendererGO.transform.position = new Vector3(fogRect.center.x, fogRect.center.y, zFogCoord);
        fogRendererGO.transform.localScale = fogRect.size;

        fogRenderer = fogRendererGO.GetComponent<MeshRenderer>();
        fogOfWarMat.SetTexture(Shader.PropertyToID("_FirstTex"), fogRenderTexture);
        fogOfWarMat.SetTexture(Shader.PropertyToID("_SecondTex"), prevFogRenderTexture);
        fogOfWarMat.SetFloat(Shader.PropertyToID("_ExploredFogAlpha"), exploredFogAlpha);
        fogRenderer.sharedMaterial = fogOfWarMat;
        
        //var mesh = MeshUtils.CreateFourSidedMesh(fogRect.min, new Vector2(fogRect.xMax, fogRect.yMin), new Vector2(fogRect.xMin, fogRect.yMax), fogRect.max);
        //var meshFilter = fogRendererGO.AddComponent<MeshFilter>();
        //meshFilter.sharedMesh = mesh;
        //fogRenderer = fogRendererGO.AddComponent<MeshRenderer>();

        //now beacuse we need to mantain the 2:1 proportion, the x size of the fog is the double of the hieght.
        //the size of the camera is relationated with the aspect ratio, that means that every 1 in the Y axis is +1 radius and 1 in the X axis is +2 radius.

    }

    private void Update()
    {
        
        objectPool.DespawnAll();
        if (showFOW)
        {
            fogRenderer.enabled = true;
        }
        else
        {
            fogRenderer.enabled = false;
            return;
        }



        var activeMap = MapManager.ActiveMap;
        if (activeMap == null) 
        {
            Debug.LogWarning("You need a active map to use the FOW system");
            return;
        }
        SpawnFOWTeamViewObjects(GameManager.PlayerTeams.ToArray(), objectPool, activeMap);

        //inverseColorToAlphaMat.SetTexture(Shader.PropertyToID("_MainTex"), fogCamera.targetTexture);

    }
    private static void SpawnFOWTeamViewObjects(int[] teams, LeanGameObjectPool pool, ActiveMap map)
    {
        //aca debo entregar una lista con todas las entidades que generan vision de un team y cual es su rango.
        var visionPointOfTeam = SightSystem.GetVisionPointsForEachTeam(teams);
        foreach (var point in visionPointOfTeam)
        {
            //debo agragarle la altura
            var elevation = MapUtilities.GetElevationOfPosition(point.center);
            var fixPositionNoElevation = map.layout.HexToWorld(point.center);
            var fixScale = map.layout.size * point.radius;

            var position = new Vector3((float)fixPositionNoElevation.x, (float)fixPositionNoElevation.y + elevation);
            var scale = new Vector3((float)fixScale.x, (float)fixScale.y)  * 2;
            var spawnedObject = pool.Spawn(position, Quaternion.identity);
            spawnedObject.transform.localScale = scale;
        }
    }
}
