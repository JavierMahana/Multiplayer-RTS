using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "Map Editor/Hex data")]
public class EditorHexData : ScriptableObject
{
    public bool walkable;
    public Material hexMaterial;
    [Tooltip("If this is on, the sprite is going to use the texture thats on the material")]
    public bool automaticSpriteCreation;
    [HideIf("automaticSpriteCreation")]
    public Sprite customSprite;
    public MapHeight heightLevel;

    public bool isSlope;
    [ShowIf("isSlope")]
    public MapHeight heightSide_TopRight;
    [ShowIf("isSlope")]
    public MapHeight heightSide_Right;
    [ShowIf("isSlope")]
    public MapHeight heightSide_DownRight;
    [ShowIf("isSlope")]
    public MapHeight heightSide_DownLeft;
    [ShowIf("isSlope")]
    public MapHeight heightSide_Left;
    [ShowIf("isSlope")]
    public MapHeight heightSide_TopLeft;

    //I need to study the render texture
    public Sprite CreateSpriteUsingMaterial()
    {
        Debug.Assert(hexMaterial != null,"in order to create a sprite automatically you need to assing the material");
        if (automaticSpriteCreation != true) { Debug.LogWarning("You are creating a sprite even tho you have automatic sprite creation disbled"); }

        throw new System.NotImplementedException();
        /*
         *              Texture mainTexture = renderer.material.mainTexture;
             Texture2D texture2D = new Texture2D(mainTexture.width, mainTexture.height, TextureFormat.RGBA32, false);
 
             RenderTexture currentRT = RenderTexture.active;
 
             RenderTexture renderTexture = new RenderTexture(mainTexture.width, mainTexture.height, 32);
             Graphics.Blit(mainTexture, renderTexture);
 
             RenderTexture.active = renderTexture;
             texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
             texture2D.Apply();
 
             Color[] pixels = texture2D.GetPixels();
 
             RenderTexture.active = currentRT;

        renderTexture.Release();
         * */
    }
}