using FixMath.NET;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class SlotAsignTest : MonoBehaviour
{
    
    private Entity entity;
    private Hex startingHex;
    public void GetEntity(Entity entity, Hex startingHex) { this.entity = entity; this.startingHex = startingHex; }
    // Start is called before the first frame update
    void Start()
    {
        if (MapManager.ActiveMap == null)
        {
            var mapManager = FindObjectOfType<MapManager>();
            Debug.Assert(mapManager != null, "You must have an Map Manager object in the scene in order to be able to author units from the editor!", this);
            mapManager.LoadMap(mapManager.mapToLoad);
        }
        var manager = World.Active.EntityManager;

        manager.SetComponentData(entity, new HexPosition()
        {
            HexCoordinates = (FractionalHex)startingHex
        });

        var newEntity = manager.Instantiate(entity);
        manager.SetComponentData(newEntity, new HexPosition()
        {
            HexCoordinates = (FractionalHex)startingHex + new FractionalHex((Fix64)(0), (Fix64)(-0.5))
        });

        newEntity = manager.Instantiate(entity);
        manager.SetComponentData(newEntity, new HexPosition()
        {
            HexCoordinates = (FractionalHex)startingHex + new FractionalHex((Fix64)(0.25), (Fix64)(-0.5))
        });
        newEntity = manager.Instantiate(entity);
        manager.SetComponentData(newEntity, new HexPosition()
        {
            HexCoordinates = (FractionalHex)startingHex + new FractionalHex((Fix64)(0.5), (Fix64)(-0.5))
        });




        //for (int i = 0; i < 8; i++)
        //{
        //    var newEntity = manager.Instantiate(entity);

        //    switch (i)
        //    {
        //        case 0:
        //            manager.SetComponentData(newEntity, new HexPosition()
        //            {
        //                HexCoordinates = (FractionalHex)startingHex + new FractionalHex((Fix64)(-0.09375*2), (Fix64)(0.09375 * 2))
        //            });
        //            break;
        //        case 1:
        //            manager.SetComponentData(newEntity, new HexPosition()
        //            {
        //                HexCoordinates = (FractionalHex)startingHex + new FractionalHex((Fix64)(0.125 * 2), (Fix64)(-0.25 * 2))
        //            });
        //            break;
        //        case 2:
        //            manager.SetComponentData(newEntity, new HexPosition()
        //            {
        //                HexCoordinates = (FractionalHex)startingHex + new FractionalHex((Fix64)(-0.140625 * 2), (Fix64)(0.140625 * 2))
        //            });
        //            break;
        //        case 3:
        //            manager.SetComponentData(newEntity, new HexPosition()
        //            {
        //                HexCoordinates = (FractionalHex)startingHex
        //            });
        //            break;
        //        case 4:
        //            manager.SetComponentData(newEntity, new HexPosition()
        //            {
        //                HexCoordinates = (FractionalHex)startingHex + new FractionalHex((Fix64)(0.140625 * 2), (Fix64)(-0.140625 * 2))
        //            });
        //            break;
        //        case 5:
        //            manager.SetComponentData(newEntity, new HexPosition()
        //            {
        //                HexCoordinates = (FractionalHex)startingHex + new FractionalHex((Fix64)(0.09375 * 2), (Fix64)(0.09375 * 2))
        //            });
        //            break;
        //        case 6:
        //            manager.SetComponentData(newEntity, new HexPosition()
        //            {
        //                HexCoordinates = (FractionalHex)startingHex + new FractionalHex((Fix64)(-0.25 * 2), (Fix64)(0.125 * 2))
        //            });
        //            break;
        //        case 7:
        //            manager.SetComponentData(newEntity, new HexPosition()
        //            {
        //                HexCoordinates = (FractionalHex)startingHex + new FractionalHex((Fix64)(0.25f * 2), (Fix64)(-0.125 * 2))
        //            });
        //            break;


        //    }

    }


}

