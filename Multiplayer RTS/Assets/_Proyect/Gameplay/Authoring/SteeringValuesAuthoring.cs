using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Authoring/Unit Steering Values")]
[System.Serializable]
public class SteeringValuesAuthoring : ScriptableObject
{
    public int targetWeight;
    public int directionWight;
    public int flockWeight;

    public int alienationWeight;
    public int cohesionWeight;
    public int separationWeight;
    public int groupalSeparationWeigth;

    public float satisfactionArea;
    public float separationDistance;
    public float singleSeparationDistance;

}
