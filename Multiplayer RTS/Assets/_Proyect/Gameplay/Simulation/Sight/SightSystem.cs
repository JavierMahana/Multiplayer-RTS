using FixMath.NET;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEditor;
using UnityEngine;
using static Unity.Mathematics.math;

//Este sistema se encarga de llenar un diccionario de visibilidad, el cual es usado por varios sistemas de simulación.
//FindActionTargetSystem.
//FinsPosibleTargetSystem.
//este sistema, además, se complementa con el de mustra de visivilidad(sistema que funciona fuera de la simulacion). ya que ese otro sistema usa los datos conseguidos
//acá para esconder o mostrar agentes, dependiendo de la visivilidad. Tambien crea las entidades visuales para las estructuras fuera de visión, pero vistas. 

public class VisionPoint
{
    public VisionPoint(FractionalHex center, Fix64 radius)
    {
        this.center = center;
        this.radius = radius;
    }
    public FractionalHex center;
    public Fix64 radius;
}
[DisableAutoCreation]
public class SightSystem : ComponentSystem
{

    //this dictionary contains the entities that each team can see.
    private static Dictionary<int, List<EntityOnVision>> EntitiesOnVisionRangeOfTeams { get; set; } = new Dictionary<int, List<EntityOnVision>>();
    private static Dictionary<int, List<VisionPoint>> VisionPointsForEachTeam { get; set; } = new Dictionary<int, List<VisionPoint>>();

    /// <summary>
    /// Currently this only returns the units of the team that have a sight vision component.
    /// It must then returns also the units of other teams that act on the team.
    /// And you must update the Update function of this class to do that
    /// </summary>
    public static List<VisionPoint> GetVisionPointsForEachTeam(int team)
    {
        if (team < 0 || team > 7)
            throw new System.NotImplementedException("Currently only supporting 8 teams. 0-7 are the supported teams");


        if (VisionPointsForEachTeam.ContainsKey(team))
            return VisionPointsForEachTeam[team];
        else
            return new List<VisionPoint>(); //throw new System.Exception($"the team selected: {team} doesn't have any entityon sight. Is there any entity of the team with SightRange component?");
    }
    public static List<VisionPoint> GetVisionPointsForEachTeam(int[] teams)
    {
        List<int> alreadyCheckedTeams = new List<int>(teams.Length);

        List<VisionPoint> visionPointsForSelectedTeams = new List<VisionPoint>();
        for (int i = 0; i < teams.Length; i++)
        {
            int team = teams[i];
            if (alreadyCheckedTeams.Contains(team))
                throw new System.ArgumentException($"You are giving a list that contains duplicates of the team: {team}");


            if (team < 0 || team > 7)
                throw new System.ArgumentException("Currently only supporting 8 teams. 0-7 are the supported teams");


            if (VisionPointsForEachTeam.ContainsKey(team))
            {
                visionPointsForSelectedTeams.AddRange(VisionPointsForEachTeam[team]);
            }
            //else
            //throw new System.Exception($"the team selected: {team} doesn't have any entityon sight. Is there any entity of the team with SightRange component?");

            alreadyCheckedTeams.Add(team);

        }

        return visionPointsForSelectedTeams;
    }

    public static List<EntityOnVision> GetEntitiesOnVisionOfTeam(int team)
    {
        if(team < 0 || team > 7)
            throw new System.NotImplementedException("Currently only supporting 8 teams. 0-7 are the supported teams");


        if (EntitiesOnVisionRangeOfTeams.ContainsKey(team))
            return EntitiesOnVisionRangeOfTeams[team];
        else
            return new List<EntityOnVision>(); //throw new System.Exception($"the team selected: {team} doesn't have any entityon sight. Is there any entity of the team with SightRange component?");

    }
    public static List<EntityOnVision> GetEntitiesOnVisionOfTeam(int[] teams)
    {

        List<int> alreadyCheckedTeams = new List<int>(teams.Length);

        List<EntityOnVision> entitiesOnVisionOfSelectedTeam = new List<EntityOnVision>();
        for (int i = 0; i < teams.Length; i++)
        {
            int team = teams[i];
            if(alreadyCheckedTeams.Contains(team))
                throw new System.ArgumentException($"You are giving a list that contains duplicates of the team: {team}");


            if (team < 0 || team > 7)
                throw new System.ArgumentException("Currently only supporting 8 teams. 0-7 are the supported teams");


            if (EntitiesOnVisionRangeOfTeams.ContainsKey(team))
                entitiesOnVisionOfSelectedTeam.AddRange( EntitiesOnVisionRangeOfTeams[team]);
            //else
            //throw new System.Exception($"the team selected: {team} doesn't have any entityon sight. Is there any entity of the team with SightRange component?");

            alreadyCheckedTeams.Add(team);
        }

        return entitiesOnVisionOfSelectedTeam;
    }


    //esta querry debe ser actualizada si es que se quiere dar otro requisito para ser parte de los objetos que pueden ser vistos
    private EntityQuery m_ObservableEntities;
    private EntityQuery m_VisionPointEntities;
    protected override void OnCreate()
    {
        var queryDesc = new EntityQueryDesc()
        {
            None = new ComponentType[] { typeof(Group), typeof(HexTile) },
            All = new ComponentType[] { typeof(HexPosition), typeof(Team), typeof(Collider) }
        };
        m_ObservableEntities = GetEntityQuery(queryDesc);


        var queryDesc2 = new EntityQueryDesc()
        {
            None = new ComponentType[] { typeof(Group), typeof(HexTile) },
            All = new ComponentType[] { typeof(HexPosition), typeof(SightRange) }
        };
        m_VisionPointEntities = GetEntityQuery(queryDesc2);
    }

    protected override void OnUpdate()
    {
        var visionPointCount = m_VisionPointEntities.CalculateEntityCount();
        VisionPointsForEachTeam = new Dictionary<int, List<VisionPoint>>(visionPointCount);
        VisionPointsForEachTeam.Clear();




        var observableCount = m_ObservableEntities.CalculateEntityCount();
        EntitiesOnVisionRangeOfTeams = new Dictionary<int, List<EntityOnVision>>(observableCount);
        EntitiesOnVisionRangeOfTeams.Clear();


        var positionsOfObservableObjects = new FractionalHex[observableCount];
        var teamsOfObservableObjects     = new int[observableCount];
        var collidersOfObservableObjects = new Collider[observableCount];
        var entitiesOfObservableObjects  = new Entity[observableCount]; 

        int iteration = 0;
        Entities.WithNone<Group, HexTile>().ForEach((Entity entity, ref HexPosition position, ref Collider collider, ref Team team) =>
        {
            positionsOfObservableObjects[iteration] = position.HexCoordinates;
            teamsOfObservableObjects[iteration]     = team.Number;
            collidersOfObservableObjects[iteration] = collider;
            entitiesOfObservableObjects[iteration]  = entity;
            iteration++;
        });


        var observablePointsSortedByHex = SpartialSortUtils.GetHexToPointsDictionary(positionsOfObservableObjects);

        Entities.WithNone<Group>().ForEach((ref HexPosition hexPosition, ref SightRange sightRange, ref Team team, ref Collider collider) => 
        {
            var position = hexPosition.HexCoordinates;


            if (VisionPointsForEachTeam.TryGetValue(team.Number, out List<VisionPoint> listOfVisionPointsOfTeam))
            {
                listOfVisionPointsOfTeam.Add(new VisionPoint(position, sightRange.Value));
                VisionPointsForEachTeam[team.Number] = listOfVisionPointsOfTeam;
            }
            else 
            {
                var newListOfVisionPointsOfTeam = new List<VisionPoint>() { new VisionPoint(position, sightRange.Value) };
                VisionPointsForEachTeam.Add(team.Number, newListOfVisionPointsOfTeam);
            }


            

            int sightDistanceInHex = (int)Fix64.Ceiling(sightRange.Value);
            var observablePointsOnRange = SpartialSortUtils.GetAllPointsAtRange(position.Round(), sightDistanceInHex, observablePointsSortedByHex);
            foreach (var point in observablePointsOnRange)
            {
                var positionOfPoint = point.position;
                int teamOfPoint = teamsOfObservableObjects[point.index];
                Collider colliderOfPoint = collidersOfObservableObjects[point.index];
                Fix64 radiusOfPoint = colliderOfPoint.Radius;
                Entity entityOfPoint = entitiesOfObservableObjects[point.index];


                var distance = position.Distance(positionOfPoint);
                bool onSight = distance <= sightRange.Value + collider.Radius + radiusOfPoint;
                if (onSight)
                {
                    var pointToAdd = new EntityOnVision(entityOfPoint, colliderOfPoint, teamOfPoint, positionOfPoint);

                    //add the entity on the dictionary if it is not there already.
                    List<EntityOnVision> entitiesOnSightOfTeam;
                    if (EntitiesOnVisionRangeOfTeams.TryGetValue(team.Number, out entitiesOnSightOfTeam))
                    {
                        if (! entitiesOnSightOfTeam.Contains(pointToAdd))
                        {
                            entitiesOnSightOfTeam.Add(pointToAdd);
                            EntitiesOnVisionRangeOfTeams[team.Number] = entitiesOnSightOfTeam;
                        }
                    }
                    else
                    {
                        entitiesOnSightOfTeam = new List<EntityOnVision>() { pointToAdd };
                        EntitiesOnVisionRangeOfTeams.Add(team.Number, entitiesOnSightOfTeam);
                    }
                }
            }
        });

    }
}
