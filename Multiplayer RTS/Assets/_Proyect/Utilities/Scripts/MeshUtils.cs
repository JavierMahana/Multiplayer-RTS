using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshUtils
{
    public static Mesh QuadMesh
    {
        get 
        {
            if (quadMesh == null) quadMesh = CreateQuadMesh();
            return quadMesh;
        }
    }
    private static Mesh quadMesh;


    //public static Mesh CreatePrimitiveMesh(PrimitiveType type)
    //{
    //    var gameObject = GameObject.CreatePrimitive(type);
    //    Mesh mesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
    //    GameObject.Destroy(gameObject);
    //    return mesh;        
    //}
    /// <summary>
    /// 
    /// </summary>
    /// <param name="vertex0">Lower left</param>
    /// <param name="vertex1">Lower Right</param>
    /// <param name="vertex2">Upper Left</param>
    /// <param name="vertex3">Upper right</param>
    /// <returns></returns>
    public static Mesh CreateFourSidedMesh(Vector2 vertex0, Vector2 vertex1, Vector2 vertex2, Vector2 vertex3)
    {
        var mesh = new Mesh();

        var vertices = new Vector3[4]
        {
            // 2-----3
            // |     |
            // 0-----1
            new Vector3(vertex0.x, vertex0.y, 0),
            new Vector3(vertex1.x, vertex1.y, 0),
            new Vector3(vertex2.x, vertex2.y, 0),
            new Vector3(vertex3.x, vertex3.y, 0)
        };
        mesh.vertices = vertices;

        var tris = new int[6]
        {
            // lower left triangle
            0, 2, 1,
            // upper right triangle
            2, 3, 1
        };
        mesh.triangles = tris;

        var normals = new Vector3[4]
        {
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward
        };
        mesh.normals = normals;

        var uv = new Vector2[4]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };
        mesh.uv = uv;

        return mesh;
    }
    private static Mesh CreateQuadMesh()
    {
        int width = 1;
        int height = 1;

        float midleWidth = (float)width/2;
        float midleHeigh = (float)height/2;

        var mesh = new Mesh();

        var vertices = new Vector3[4]
        {
            // 2-----3
            // |     |
            // 0-----1
            new Vector3(-midleWidth, -midleHeigh, 0),
            new Vector3(midleWidth, -midleHeigh, 0),
            new Vector3(-midleWidth, midleHeigh, 0),
            new Vector3(midleWidth, midleHeigh, 0)
        };
        mesh.vertices = vertices;

        var tris = new int[6]
        {
            // lower left triangle
            0, 2, 1,
            // upper right triangle
            2, 3, 1
        };
        mesh.triangles = tris;

        var normals = new Vector3[4]
        {
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward
        };
        mesh.normals = normals;

        var uv = new Vector2[4]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };
        mesh.uv = uv;

        
        return mesh;
    }
}
