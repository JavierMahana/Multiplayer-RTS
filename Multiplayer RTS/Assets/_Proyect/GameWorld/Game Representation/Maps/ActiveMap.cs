using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveMap
{
    public RuntimeMap map;
    public Layout layout;
    public ActiveMap(RuntimeMap map, Layout layout)
    {
        this.map = map;
        this.layout = layout;
    }
}
