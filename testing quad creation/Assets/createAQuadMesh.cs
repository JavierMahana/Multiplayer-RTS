using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class createAQuadMesh : MonoBehaviour
{
    public Vector2 e1 = Vector2.zero;
    public Vector2 e2 = Vector2.up;
    public Vector2 e3 = Vector2.one;
    public Vector2 e4 = Vector2.right;
    public float vel = 10;
    public Material mat;

    public Mesh mesh;
    public GameObject obj;
    // Start is called before the first frame update
    void Start()
    {
        Vector3[] vertices = new Vector3[4]

        {
            new Vector3(e1.x, e1.y, transform.position.z),
            new Vector3(e2.x, e2.y, transform.position.z),
            new Vector3(e3.x, e3.y, transform.position.z),
            new Vector3(e4.x, e4.y, transform.position.z),
        };

        int[] triangles = new int[6]
        {
            1,
            2,
            0,

            2,
            3,
            0
        };

        mesh = new Mesh() {vertices = vertices, triangles = triangles };
        obj = new GameObject("mesh", typeof(MeshFilter), typeof(MeshRenderer));
        obj.GetComponent<MeshFilter>().sharedMesh = mesh;
        if(mat!= null)
            obj.GetComponent<MeshRenderer>().sharedMaterial = mat;
    }


    void Update()
    {
        obj.transform.Rotate(Vector3.forward * Time.deltaTime * vel, Space.Self);
    }
}
