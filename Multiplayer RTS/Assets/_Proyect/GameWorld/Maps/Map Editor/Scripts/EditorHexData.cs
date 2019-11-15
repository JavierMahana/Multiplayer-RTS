using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Map Editor/Hex data")]
public class EditorHexData : ScriptableObject
{
    public bool walkable;
    public Material hexMaterial;
}