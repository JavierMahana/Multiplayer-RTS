using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

public class TestWindow : OdinEditorWindow
{
    [MenuItem("My game/My Editor")]
    private static void OpenWindow()
    {
        var tree = new OdinMenuTree();
        GetWindow<TestWindow>().Show();
        
    }


    [OnInspectorGUI("GUIBefore", "GUIAfter")]
    public int MyField;

    private void GUIBefore()
    {
        GUILayout.Label("Label before My Field property");
    }

    private void GUIAfter()
    {
        GUILayout.Label("Label after My Field property");
    }

    public string Hello;
}
