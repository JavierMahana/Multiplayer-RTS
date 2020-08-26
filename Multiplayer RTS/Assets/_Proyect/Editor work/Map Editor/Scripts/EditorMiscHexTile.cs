﻿#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[PropertyTooltip("This class is created by the map editor and it stores a EditorHexData that is updated by the material that you put on the mesh renderer.")]
[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
[ExecuteInEditMode]
public class EditorMiscHexTile : MonoBehaviour
{
    [ReadOnly]
    [ShowInInspector]
    private Hex hex;

    public EditorMiscHexData data { get; private set; }

    private MapEditor editor;

    private MeshRenderer meshRenderer;
    private Material prevRendererMaterial;

    public void Init(MapEditor editor, EditorMiscHexData data, Hex hex)
    {
        this.editor = editor;

        this.hex = hex;

        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = data.hexMaterial;
    }



    private void OnValidate()
    {
        if (meshRenderer == null)
        {
            meshRenderer = GetComponent<MeshRenderer>();
            prevRendererMaterial = meshRenderer.sharedMaterial;
        }
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            if (meshRenderer.sharedMaterial != prevRendererMaterial)
            {
                Debug.Assert(editor != null, "You must call the Init function when creating this object in order to make it work as intended!");

                if (editor.MiscDataLookUpTable.TryGetValue(meshRenderer.sharedMaterial, out EditorMiscHexData newData))
                {
                    data = newData;
                    prevRendererMaterial = meshRenderer.sharedMaterial;
                }
                else
                {
                    Debug.LogWarning("The material you put on the tile is invalid! Returning the mesh renderer to his previous material");
                    meshRenderer.sharedMaterial = prevRendererMaterial;
                }
            }
        }
    }
    void Start()
    {
        if (Application.isPlaying) Debug.LogWarning("You must destroy the EditorHexTile objects before entering play mode!");
    }

}
#endif