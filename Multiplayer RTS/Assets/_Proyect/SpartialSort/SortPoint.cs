using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SortPoint 
{
    public int index;
    public FractionalHex position;

    public SortPoint(int index, FractionalHex position)
    {
        this.index = index;
        this.position = position;
    }
}
