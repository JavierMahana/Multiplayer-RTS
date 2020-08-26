using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "Map Editor/Misc Hex data")]
///the misc 
public class EditorMiscHexData : ScriptableObject
{
    public bool isEmpty = false;
    [HideIf("isEmpty")]
    public bool isResource = false;
    [HideIf("isEmpty")]
    [ShowIf("isResource")]
    public ResourceSpotData resourceData;

    [Tooltip("The material that is vinculated with this data")]
    public Material hexMaterial;



}

