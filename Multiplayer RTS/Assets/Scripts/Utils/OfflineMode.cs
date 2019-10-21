using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class OfflineMode
{
    public static bool OffLineMode { get; private set; }
    public static void SetOffLineMode(bool value)
    {
        OffLineMode = value;
    }
}
