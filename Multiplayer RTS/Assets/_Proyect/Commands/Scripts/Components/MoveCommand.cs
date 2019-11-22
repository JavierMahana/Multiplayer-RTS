using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

[Serializable]
public struct MoveCommand 
{        
    public Entity Target;
    public DestinationHex Destination;    
}
