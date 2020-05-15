using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;
using FixMath.NET;

public class TestMap : MonoBehaviour
{
    private FixVector2 hexSize;
    private FixVector2 origin;
    private Vector2 extents;

    public Vector2Int dimensions;
    public Transform originPoint;
    public GameObject sampleHex;
    public Mesh hexMesh;
    public Material hexMaterial;
    public Camera renderCamera;


    private void Start()
    {
        if (hexMesh == null) hexMesh = sampleHex.GetComponent<MeshFilter>().mesh;
        var bounds = hexMesh.bounds;
        extents = bounds.extents;

        if (hexMaterial == null) hexMaterial = sampleHex.GetComponent<MeshRenderer>().material;

        origin = new FixVector2((Fix64)originPoint.position.x, (Fix64)originPoint.position.y);
        hexSize = new FixVector2(((Fix64)sampleHex.transform.localScale.x * (Fix64)extents.x ), ((Fix64)sampleHex.transform.localScale.y * (Fix64)extents.y));

        CreateVisualMapEntities();
        //CreateVisualMap();
    }

    public void CreateVisualMapEntities()
    {
        var entityManager = World.Active.EntityManager;
        var tileArchetype = entityManager.CreateArchetype(typeof(LocalToWorld), typeof(NonUniformScale), typeof(RenderMesh), typeof(Translation), typeof(HexTile));
        Entity tileEntity = entityManager.CreateEntity(tileArchetype);
        entityManager.SetSharedComponentData(tileEntity, new RenderMesh() { mesh = hexMesh, material = hexMaterial });
        entityManager.SetComponentData(tileEntity, new NonUniformScale()
        {
            Value = new float3(sampleHex.transform.localScale.x, sampleHex.transform.localScale.y, 1)
        });
        bool firstEntityUsed = false;



        var layout = new Layout(Orientation.pointy, hexSize, origin);
        


        for (int r = 0; r < dimensions.y; r++)
        {
            int r_offset = Mathf.FloorToInt(r / 2); // or r>>1
            for (int q = -r_offset; q < dimensions.x - r_offset; q++)
            {
                Hex hex = new Hex(q, r);

                FixVector2 position = layout.HexToWorld(hex);

                Entity entityToUse;
                if (!firstEntityUsed)
                {
                    firstEntityUsed = true;
                    entityToUse = tileEntity;
                }
                else entityToUse = entityManager.Instantiate(tileEntity);

                entityManager.SetComponentData(entityToUse, new Translation() { Value = new float3((float)position.x, (float)position.y, 15) });
            }
        }
    }
    public void CreateVisualMap()
    {
        var layout = new Layout(Orientation.pointy, hexSize, origin);

        for (int r = 0; r < dimensions.y; r++)
        {
            int r_offset = Mathf.FloorToInt(r / 2); // or r>>1
            for (int q = -r_offset; q < dimensions.x - r_offset; q++)
            {
                Hex hex = new Hex(q, r);

                FixVector2 position = layout.HexToWorld(hex);
                Instantiate(sampleHex, new Vector3((float)position.x, (float)position.y + 10, 15), Quaternion.identity);
            }
        }


    }
 
}
