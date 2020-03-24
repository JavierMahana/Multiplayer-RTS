using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MeshCreator : MonoBehaviour
{
    public Vector2 llPoint;
    public Vector2 lrPoint;
    public Vector2 ulPoint;
    public Vector2 urPoint;

    public Material material;
    void Start()
    {
        var filter = GetComponent<MeshFilter>();
        var renderer = GetComponent<MeshRenderer>();


        filter.mesh = MeshUtils.CreateFourSidedMesh(llPoint, lrPoint, ulPoint, urPoint);
        renderer.sharedMaterial = material;
    }    
}
