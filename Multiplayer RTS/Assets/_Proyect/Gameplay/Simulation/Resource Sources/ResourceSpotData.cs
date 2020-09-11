using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ResourceSpotData 
{
    public Sprite sprite;
    public int ammount;
    public int maxGatherers;
    public ResourceType resourceType;
    public int ticksForExtraction = 30;

}
