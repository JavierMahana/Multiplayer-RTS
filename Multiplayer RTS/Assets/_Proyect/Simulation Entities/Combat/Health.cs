﻿using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct Health : IComponentData
{
    public int MaxHealth;
    public int CurrentHealth;
    
}
