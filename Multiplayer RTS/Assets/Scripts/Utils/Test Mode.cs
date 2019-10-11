using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMode : Singleton<TestMode>
{
    public bool OffLineModeTest { get => offLineModeTest; }
    [SerializeField]
    private bool offLineModeTest = false;

}
