using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEditor;
using Sirenix.Utilities.Editor;
using System.IO;




public class MaterialToTextureWindow : OdinEditorWindow
{
    public string creationFolder;
    [Space]
    public string customName = "";
    public Material material;

    private bool notNullMaterial => material != null;



    [ShowIf("notNullMaterial")]
    [Button]
    private void CreateTextureAsset()
    {
        if (material == null) { Debug.LogError("Need a material!"); return; }

        string completePath;
        if (customName != "")
        {
            completePath = creationFolder + customName + ".png";
        }
        else
        {
            completePath = creationFolder + material.name + " texture.png";
        }
        var newTexture = TextureUtils.GetTexture2DFromMaterial(material);

        if (File.Exists(completePath))
        {
            Debug.LogError("can't create new file because there is a file with the same name");
            return;
        }

        var PNGbytes = newTexture.EncodeToPNG();
        File.WriteAllBytes(completePath, PNGbytes);
        AssetDatabase.Refresh();

    }



    [MenuItem("Editor tools/Material To Texture Creator")]
    private static void OpenWindow()
    {
        var window = GetWindow<MaterialToTextureWindow>();
        window.Show();
        window.position = GUIHelper.GetEditorWindowRect().AlignCenter(320, 400);
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        creationFolder = $"{Application.dataPath}/_Proyect/Resources/Textures/";
    }
}
