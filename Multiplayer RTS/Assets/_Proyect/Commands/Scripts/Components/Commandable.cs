﻿using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;


[Serializable]
public struct Commandable : IComponentData 
{
    public CommandType DeafaultCommand;
}
