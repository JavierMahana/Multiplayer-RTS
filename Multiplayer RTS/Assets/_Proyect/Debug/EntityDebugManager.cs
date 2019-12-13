using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Unity.Entities;
using Unity.Collections;

public class EntityDebugManager : MonoBehaviour
{
    

    public bool drawGizmosOnUnits = false;
    [ShowIf("drawGizmosOnUnits")]
    public bool showPhysicalRadius = false;
    [ShowIf("DrawPhysicalRadiusGizmos")]
    public Color radiusColor = Color.blue;

    private bool DrawPhysicalRadiusGizmos => drawGizmosOnUnits && showPhysicalRadius;



    private EntityQuery colliderQuery;
    private void Start()
    {
        var entityManager = World.Active.EntityManager;
        colliderQuery = entityManager.CreateEntityQuery(typeof(HexPosition), typeof(Collider));
    }
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        { return; }
        
        Debug.Assert(MapManager.ActiveMap != null,"the entity debug requires an active map");
        var camera = Camera.main;
        var layout = MapManager.ActiveMap.layout;
        var entityManager = World.Active.EntityManager;
        if (DrawPhysicalRadiusGizmos)
        {
            Gizmos.color = radiusColor;

            var colliderComp = colliderQuery.ToComponentDataArray<Collider>(Allocator.TempJob);
            var positionComp = colliderQuery.ToComponentDataArray<HexPosition>(Allocator.TempJob);

            for (int i = 0; i < colliderComp.Length; i++)
            {
                var collider = colliderComp[i];
                var position = positionComp[i];

                var sides = new FractionalHex[6];
                for (int n = 0; n < 6; n++)
                {
                    var side = position.HexCoordinates + ((FractionalHex)Hex.directions[n] * collider.Radious);
                    sides[n] = side;
                }
                for (int s = 0; s < 6; s++)
                {
                    int q = s - 1;
                    q = q == -1 ? 5 : q;

                    var lineStart = layout.HexToWorld(sides[q]);
                    var lineEnd = layout.HexToWorld(sides[s]);
                    var lineStartVector = new Vector3( (float)lineStart.x,(float)lineStart.y, camera.nearClipPlane);
                    var lineEndVector = new Vector3((float)lineEnd.x, (float)lineEnd.y, camera.nearClipPlane);

                    Gizmos.DrawLine(lineStartVector, lineEndVector);
                }

            }

            positionComp.Dispose();
            colliderComp.Dispose();
        }
    }
}
