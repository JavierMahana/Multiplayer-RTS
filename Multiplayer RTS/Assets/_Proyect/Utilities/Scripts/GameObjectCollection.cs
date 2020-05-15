using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//the intended usage of this object is to destroy objects that are supposed to be "editor only"
[CreateAssetMenu(menuName = "Utilities/GameObjectCollection")]
public class GameObjectCollection : ScriptableObject
{
    public List<GameObject> Objects = new List<GameObject>();
}
