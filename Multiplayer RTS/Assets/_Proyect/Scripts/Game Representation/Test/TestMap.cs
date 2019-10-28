using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using FixMath.NET;

public class TestMap : MonoBehaviour
{
    private FixVector2 hexSize;
    private FixVector2 origin;

    public Vector2Int dimensions;
    public Transform originPoint;
    public GameObject sampleHex;
    public Mesh spriteMesh;
    public Material spriteMaterial;
    public Camera renderCamera;

    private void Start()
    {
        origin = new FixVector2((Fix64)originPoint.position.x, (Fix64)originPoint.position.y);
        hexSize = new FixVector2((Fix64)(sampleHex.transform.localScale.x *22.95), (Fix64)(sampleHex.transform.localScale.y * 22.95));
        CreateVisualMap();
    }

    public void CreateVisualMap()
    {
        var layout = new Layout(Orientation.pointy, hexSize, origin);

        for (int r = 0; r < dimensions.y; r++)
        {
            int r_offset = Mathf.FloorToInt(r / 2); // or r>>1
            for (int q = -r_offset; q < dimensions.x - r_offset; q++)
            {
                Hex hex = new Hex(q, r, -q - r);

                FixVector2 pixel = layout.HexToPixel(hex);
                Vector3 position = renderCamera.ScreenToWorldPoint(new Vector3( (float)pixel.x, (float)pixel.y, renderCamera.nearClipPlane));

                Instantiate(sampleHex, new Vector3(position.x, position.y, 15), Quaternion.identity);
            }
        }


    }
 
}
