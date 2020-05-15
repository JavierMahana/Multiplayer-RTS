﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//source: https://answers.unity.com/questions/761009/unity2d-sprite-editor-multiple-pivot-point-edit.html

public static class SpriteUtils 
{
	[MenuItem("Sprite/Set Pivot(s)")]
	static void SetPivots()
	{

		Object[] textures = GetSelectedTextures();

		Selection.objects = new Object[0];
		foreach (Texture2D texture in textures)
		{
			string path = AssetDatabase.GetAssetPath(texture);
			TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;

			ti.isReadable = false; // this 2 lines you need to add
			AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
			ti.isReadable = true;
			List<SpriteMetaData> newData = new List<SpriteMetaData>();
			for (int i = 0; i < ti.spritesheet.Length; i++)
			{
				SpriteMetaData d = ti.spritesheet[i];
				d.alignment = 9;
				d.pivot = ti.spritesheet[0].pivot;
				newData.Add(d);
			}
			ti.spritesheet = newData.ToArray();
			AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
		}
	}

	static Object[] GetSelectedTextures()
	{
		return Selection.GetFiltered(typeof(Texture2D), SelectionMode.DeepAssets);
	}
}
