using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
using FixMath.NET;

[UpdateInGroup(typeof(PresentationSystemGroup))]
public class HexTranslationSystem : ComponentSystem
{
    public const int AGENTS_Z_VALUE = 9;
    public const int TILE_Z_VALUE = 10;

    protected override void OnUpdate()
    {
        if (MapManager.ActiveMap == null)
            return ;
        else
        {
            Entities.WithNone<HexTile>().ForEach((ref Translation translation, ref HexPosition hexPosition) =>
            {
                var worldPos = MapManager.ActiveMap.layout.HexToWorld(hexPosition.HexCoordinates);
                translation.Value = new float3((float)worldPos.x, (float)worldPos.y, AGENTS_Z_VALUE);
            });
            Entities.WithAll<HexTile>().ForEach((ref Translation translation, ref HexPosition hexPosition) =>
            {
                var worldPos = MapManager.ActiveMap.layout.HexToWorld(hexPosition.HexCoordinates);
                translation.Value = new float3((float)worldPos.x, (float)worldPos.y, TILE_Z_VALUE);
            });
        }
    }
}