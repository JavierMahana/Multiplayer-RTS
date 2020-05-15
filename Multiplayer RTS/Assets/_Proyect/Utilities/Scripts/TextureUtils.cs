using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureUtils 
{

    public static string GetUniquePathWhenCreatingPNG(string name)
    {
        throw new System.NotImplementedException(); 
    }
    public static Texture2D GetTexture2DFromMaterial(Material material, TextureFormat textureFormat = TextureFormat.RGBA32, bool mipMapsEnabled = false)
    {
        Texture mainTexture = material.mainTexture;
        Texture2D texture2D = new Texture2D(mainTexture.width, mainTexture.height, textureFormat, mipMapsEnabled);

        RenderTexture currentRT = RenderTexture.active;

        RenderTexture renderTexture = new RenderTexture(mainTexture.width, mainTexture.height, 32);
        //copia la textura en la render texture.
        Graphics.Blit(mainTexture, renderTexture);

        RenderTexture.active = renderTexture;
        texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture2D.Apply();
        
        //Color[] pixels = texture2D.GetPixels();

        RenderTexture.active = currentRT;

        renderTexture.Release();

        return texture2D;
    }
}
