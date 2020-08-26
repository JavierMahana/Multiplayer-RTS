#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEditor;
using System.IO;

[RequireComponent(typeof(SpriteRenderer))]
public class AssetCreationTest : MonoBehaviour
{
    private SpriteRenderer _sr;
    public Material material;
    public Texture2D texture;
    public Sprite storedSprite;
    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
    }
    private void Start()
    {
        _sr.sprite = storedSprite;
    }
    [Button]
    public void CreateSpriteAndTextureUssingMaterial()
    {
        if (material == null) return;

        var newTexture = TextureUtils.GetTexture2DFromMaterial(material);
        var texturePath = $"{Application.dataPath}/_Proyect/{ material.name } Texture.png";//AssetDatabase.GenerateUniqueAssetPath($"Assets/_Proyect/{ material.name } Texture.png");

        if (File.Exists(texturePath))
        {
            Debug.LogError("can't create new file because there is a file with the same name");
            return;
        }

        var PNGbytes = newTexture.EncodeToPNG();
        File.WriteAllBytes(texturePath, PNGbytes);
        AssetDatabase.Refresh();
        //Debug.Log($"{File.Exists(texturePath)}");
        //Resources
        //AssetDatabase.fin
        //AssetDatabase.CreateAsset(newTexture, texturePath);
        //newTexture.Apply();

        //var sprite = Sprite.Create(newTexture, new Rect(0.0f, 0.0f, newTexture.width, newTexture.height), new Vector2(0.5f, 0.5f));
        //sprite.name = $"{material.name} Sprite";
        //storedSprite = sprite;
        //AssetDatabase.AddObjectToAsset(sprite, newTexture);
        //AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(sprite));

        //storedSprite = sprite;
        //var path = AssetDatabase.GenerateUniqueAssetPath("Assets/_Proyect/MySprite.asset");
        //AssetDatabase.CreateAsset(sprite, path);

    }

    [Button]
    public void CreateAsset()
    {
        if (texture == null) return;

        var sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        storedSprite = sprite;
        //storedSprite = sprite;
        var path = AssetDatabase.GenerateUniqueAssetPath("Assets/_Proyect/MySprite.asset");
        AssetDatabase.CreateAsset(sprite, path);

    }
    [Button]
    public void MoveAsset()
    {
        //var path = AssetDatabase.GetAssetPath(storedSprite);

        //Debug.Log(path);
        AssetDatabase.MoveAsset("Assets/MySprite.asset", "Assets/_Proyect/MySprite.asset");
    }


}
#endif