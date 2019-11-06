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
