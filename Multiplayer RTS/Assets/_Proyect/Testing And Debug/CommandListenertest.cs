using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class CommandListenertest : MonoBehaviour
{
    public Text TextDisplay;
    private static Text textDisplay;

    private void Start()
    {
        if (TextDisplay == null)
            return;

        textDisplay = TextDisplay;
    }

    public static void DisplayText(string text)
    {
        if (textDisplay == null)
            return;
        textDisplay.text = text;
    }

}
