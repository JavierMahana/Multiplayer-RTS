using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "Map Editor/Hex data")]
public class EditorHexData : ScriptableObject
{
    public bool walkable = true;
    public Material hexMaterial;
    [Tooltip("The sprite that will be used in game.")]
    public Sprite sprite;
    public MapHeight heightLevel = MapHeight.l0;

    public bool isSlope = false;
    [ShowIf("isSlope")]
    public MapHeight heightSide_TopRight = MapHeight.l0;
    [ShowIf("isSlope")]
    public MapHeight heightSide_Right = MapHeight.l0;
    [ShowIf("isSlope")]
    public MapHeight heightSide_DownRight = MapHeight.l0;
    [ShowIf("isSlope")]
    public MapHeight heightSide_DownLeft = MapHeight.l0;
    [ShowIf("isSlope")]
    public MapHeight heightSide_Left = MapHeight.l0;
    [ShowIf("isSlope")]
    public MapHeight heightSide_TopLeft = MapHeight.l0;
}