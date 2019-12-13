using FixMath.NET;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class FormationPositionTest : MonoBehaviour, IConvertGameObjectToEntity
{

    public SlotAsignTest slotAsign;
    // Add fields to your component here. Remember that:
    //
    // * The purpose of this class is to store data for authoring purposes - it is not for use while the game is
    //   running.
    // 
    // * Traditional Unity serialization rules apply: fields must be public or marked with [SerializeField], and
    //   must be one of the supported types.
    //
    // For example,
    //    public float scale;



    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        if (MapManager.ActiveMap == null)
        {
            var mapManager = FindObjectOfType<MapManager>();
            Debug.Assert(mapManager != null, "You must have an Map Manager object in the scene in order to be able to author units from the editor!", this);
            mapManager.LoadMap(mapManager.mapToLoad);
        }
        var fractionalHex = MapManager.ActiveMap.layout.WorldToFractionalHex(new FixVector2((Fix64)transform.position.x, (Fix64)transform.position.y));
        //var pos = layout.HexToWorld(fractionalHex);

        dstManager.AddComponentData<HexPosition>(entity, new HexPosition() { HexCoordinates = fractionalHex });
        slotAsign.GetEntity(entity, fractionalHex.Round());
    }
}
