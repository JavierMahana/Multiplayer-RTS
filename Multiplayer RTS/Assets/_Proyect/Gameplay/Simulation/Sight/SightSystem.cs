using FixMath.NET;
using Sirenix.Utilities;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements.Experimental;
using static Unity.Mathematics.math;
/// <summary>
///Este sistema se encarga de llenar un diccionario de visibilidad, el cual es usado por varios sistemas de simulación.
///FindActionTargetSystem.
///FinsPosibleTargetSystem.
///este sistema, además, se complementa con el de mustra de visivilidad(sistema que funciona fuera de la simulacion). ya que ese otro sistema usa los datos conseguidos
///acá para esconder o mostrar agentes, dependiendo de la visivilidad. Tambien crea las entidades visuales para las estructuras fuera de visión, pero vistas. 
/// </summary>
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
    //DONT USE IN SIMULATION!
    //IT CONTAINTS ENTITIES THAT HAVE THE dontrun in simulation component.
    private static Dictionary<int, HashSet<Entity>> EntitiesOnVisionRangeOfTeamsHashset { get; set; } = new Dictionary<int, HashSet<Entity>>();

    //this dictionary contains the entities that each team can see.
    //CAN USE THIS DICTIONARIES FOR SIMULATION


    private static Dictionary<int, List<BuildingOnVision>> BuildingsOnVisionRangeOfTeams { get; set; } = new Dictionary<int, List<BuildingOnVision>>();

    private static Dictionary<int, List<EntityOnVision>> UnitsOnVisionRangeOfTeams { get; set; } = new Dictionary<int, List<EntityOnVision>>();


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



                                          


    public static List<EntityOnVision> GetUnitsOnVisionOfTeam(int team)
    {
        if(team < 0 || team > 7)
            throw new System.NotImplementedException("Currently only supporting 8 teams. 0-7 are the supported teams");


        if (UnitsOnVisionRangeOfTeams.ContainsKey(team))
            return UnitsOnVisionRangeOfTeams[team];
        else
            return new List<EntityOnVision>(); //throw new System.Exception($"the team selected: {team} doesn't have any entityon sight. Is there any entity of the team with SightRange component?");

    }
    public static List<EntityOnVision> GetUnitsOnVisionOfTeam(int[] teams)
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


            if (UnitsOnVisionRangeOfTeams.ContainsKey(team))
                entitiesOnVisionOfSelectedTeam.AddRange( UnitsOnVisionRangeOfTeams[team]);
            //else
            //throw new System.Exception($"the team selected: {team} doesn't have any entityon sight. Is there any entity of the team with SightRange component?");

            alreadyCheckedTeams.Add(team);
        }

        return entitiesOnVisionOfSelectedTeam;
    }




    public static List<BuildingOnVision> GetBuildingsOnVisionOfTeam(int team)
    {
        if (team < 0 || team > 7)
            throw new System.NotImplementedException("Currently only supporting 8 teams. 0-7 are the supported teams");


        if (BuildingsOnVisionRangeOfTeams.ContainsKey(team))
            return BuildingsOnVisionRangeOfTeams[team];
        else
            return new List<BuildingOnVision>(); //throw new System.Exception($"the team selected: {team} doesn't have any entityon sight. Is there any entity of the team with SightRange component?");

    }
    public static List<BuildingOnVision> GetBuildingsOnVisionOfTeam(int[] teams)
    {

        List<int> alreadyCheckedTeams = new List<int>(teams.Length);

        List<BuildingOnVision> entitiesOnVisionOfSelectedTeam = new List<BuildingOnVision>();
        for (int i = 0; i < teams.Length; i++)
        {
            int team = teams[i];
            if (alreadyCheckedTeams.Contains(team))
                throw new System.ArgumentException($"You are giving a list that contains duplicates of the team: {team}");


            if (team < 0 || team > 7)
                throw new System.ArgumentException("Currently only supporting 8 teams. 0-7 are the supported teams");

            if (BuildingsOnVisionRangeOfTeams.ContainsKey(team))
                entitiesOnVisionOfSelectedTeam.AddRange(BuildingsOnVisionRangeOfTeams[team]);  
            //else
            //throw new System.Exception($"the team selected: {team} doesn't have any entityon sight. Is there any entity of the team with SightRange component?");

            alreadyCheckedTeams.Add(team);
        }

        return entitiesOnVisionOfSelectedTeam;
    }





    public static HashSet<Entity> GetEntitiesOnVisionOfTeamHashSet(int team)
    {
        if (team < 0 || team > 7)
            throw new System.NotImplementedException("Currently only supporting 8 teams. 0-7 are the supported teams");


        if (EntitiesOnVisionRangeOfTeamsHashset.ContainsKey(team))
            return EntitiesOnVisionRangeOfTeamsHashset[team];
        else
            return new HashSet<Entity>(); //throw new System.Exception($"the team selected: {team} doesn't have any entityon sight. Is there any entity of the team with SightRange component?");

    }
    public static HashSet<Entity> GetEntitiesOnVisionOfTeamHashSet(int[] teams)
    {

        List<int> alreadyCheckedTeams = new List<int>(teams.Length);

        HashSet<Entity> entitiesOnVisionOfSelectedTeam = new HashSet<Entity>();
        for (int i = 0; i < teams.Length; i++)
        {
            int team = teams[i];
            if (alreadyCheckedTeams.Contains(team))
                throw new System.ArgumentException($"You are giving a list that contains duplicates of the team: {team}");


            if (team < 0 || team > 7)
                throw new System.ArgumentException("Currently only supporting 8 teams. 0-7 are the supported teams");


            if (EntitiesOnVisionRangeOfTeamsHashset.ContainsKey(team))
                entitiesOnVisionOfSelectedTeam.AddRange(EntitiesOnVisionRangeOfTeamsHashset[team]);

            alreadyCheckedTeams.Add(team);
        }

        return entitiesOnVisionOfSelectedTeam;
    }


    //esta querry debe ser actualizada si es que se quiere dar otro requisito para ser parte de los objetos que pueden ser vistos
    private EntityQuery m_ObservableUnits;
    private EntityQuery m_ObservableBuildings;
    private EntityQuery m_VisionPointEntities;
    protected override void OnCreate()
    {
        var queryDesc = new EntityQueryDesc()
        {
            None = new ComponentType[] { typeof(Group), typeof(HexTile), typeof(ExcludeFromSimulation) },
            All = new ComponentType[] { typeof(HexPosition), typeof(Team)}
        };
        m_ObservableUnits = GetEntityQuery(queryDesc);

        var queryDesc1 = new EntityQueryDesc()
        {
            None = new ComponentType[] { typeof(Group), typeof(HexTile), typeof(ExcludeFromSimulation) },
            All = new ComponentType[] { typeof(Building), typeof(Team) },
        };
        m_ObservableBuildings = GetEntityQuery(queryDesc1);

        var queryDesc2 = new EntityQueryDesc()
        {
            None = new ComponentType[] { typeof(Group), typeof(HexTile) },
            All = new ComponentType[] {  typeof(SightRange), typeof(Team) },
            Any = new ComponentType[] { typeof(HexPosition), typeof(Building)}
        };
        m_VisionPointEntities = GetEntityQuery(queryDesc2);
    }

    protected override void OnUpdate()
    {
        int visionPointCount = m_VisionPointEntities.CalculateEntityCount();
        VisionPointsForEachTeam = new Dictionary<int, List<VisionPoint>>(visionPointCount);


        int observableUnitCount = m_ObservableUnits.CalculateEntityCount();
        UnitsOnVisionRangeOfTeams = new Dictionary<int, List<EntityOnVision>>(observableUnitCount);

        var positionsOfObservableUnits = new FractionalHex[observableUnitCount];
        var teamsOfObservableUnits = new int[observableUnitCount];
        var collidersOfObservableUnits = new Collider[observableUnitCount];
        var entitiesOfObservableUnits = new Entity[observableUnitCount];

        int observableBuildingCount = m_ObservableBuildings.CalculateEntityCount();
        BuildingsOnVisionRangeOfTeams = new Dictionary<int, List<BuildingOnVision>>(observableBuildingCount);

        var observableBuildings = new Dictionary<Hex, BuildingOnVision>(observableBuildingCount);
        var obserbableSubstitutes = new Dictionary<Hex, SubstituteOnVision>();

        EntitiesOnVisionRangeOfTeamsHashset = new Dictionary<int, HashSet<Entity>>(observableUnitCount + observableBuildingCount);



        //INIT THE LISTS OF UNIT COMPONENTS
        int iteration = 0;
        Entities.WithNone<Group, HexTile, ExcludeFromSimulation>().ForEach((Entity entity, ref HexPosition position, ref Collider collider, ref Team team) =>
        {
            positionsOfObservableUnits[iteration] = position.HexCoordinates;
            teamsOfObservableUnits[iteration]     = team.Number;
            collidersOfObservableUnits[iteration] = collider;
            entitiesOfObservableUnits[iteration]  = entity;
            iteration++;
        });
        Entities.WithNone<Group, HexTile, ExcludeFromSimulation>().ForEach((Entity entity, ref Building building, ref Team team) => 
        {
            Hex hex = building.position;
            if(observableBuildings.ContainsKey(hex))
            {
                Debug.LogError($"In {hex} are more than 1 building.");
            }
            else 
            {
                observableBuildings.Add(hex, new BuildingOnVision(entity, building, team));
            }            
        });
        Entities.WithAll<Substitute>().ForEach((Entity entity, ref Building building) => 
        {
            Hex hex = building.position;
            if ( obserbableSubstitutes.ContainsKey(hex))
            {
                Debug.LogError($"In {hex} are more than 1 substitute.");
            }
            else
            {
                obserbableSubstitutes.Add(hex, new SubstituteOnVision(entity, building));
            }
        });

        //WE GET THE OBSERVABLE UNITS SORTED BY THE HEX THAT THEY ARE STANDING ON
        var observablePointsSortedByHex = SpartialSortUtils.GetHexToPointsDictionary(positionsOfObservableUnits);



        //1- AGREGA LA ENTIDAD A LOS PUNTOS DE VISION DE SU EQUIPO
        Entities.WithNone<ExcludeFromSimulation>().ForEach((ref Building building, ref SightRange sightRange, ref Team team) => 
        {
            var position = (FractionalHex)building.position;

            //ADD THE UNIT TO THE POINT OF VISION OF HIS TEAM.
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
        });
        Entities.WithNone<Group, ExcludeFromSimulation>().ForEach((ref HexPosition hexPosition, ref SightRange sightRange, ref Team team) =>
        {
            var position = hexPosition.HexCoordinates;

            //ADD THE UNIT TO THE POINT OF VISION OF HIS TEAM.
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
        });

        // loopea por todas las ENTIDADES(unidades y estructuras) con sight range y luego en cada una revisa las ENTIDADES obserbables (dentro de los hexagonos de su vision) 
        // y si es que estan en su rango de vision los agrega a unidades/buildings en vista y a la hashset de entidades.
        // hay que agragar que esto revisa por las estructuras a la vista.
        Entities.WithNone<Group, HexTile, ExcludeFromSimulation>().WithAny<HexPosition, Building>().ForEach((Entity entity, ref SightRange sightRange, ref Team team) => 
        {
            FractionalHex position;
            if (EntityManager.HasComponent<HexPosition>(entity))
            {
                position = EntityManager.GetComponentData<HexPosition>(entity).HexCoordinates;
            }
            else 
            {
                position = (FractionalHex)EntityManager.GetComponentData<Building>(entity).position;
            }
            
            
            //ACÁ SE REVISA 1 A 1 SI LA ENTIDAD ACTUAL VE ALGUNA DE LAS UNIDADES QUE ESTAN DENTRO DE LOS HEXAGONOS QUE VE.

            //BUILDINGS.
            int sightDistanceInHex = (int)Fix64.Ceiling(sightRange.Value);

            var allVisibleHexes = Hex.AllHexesInRange(position.Round(), sightDistanceInHex, true);
            foreach (var visHex in allVisibleHexes)
            {
                if(observableBuildings.ContainsKey(visHex))
                {
                    BuildingOnVision buildingComps = observableBuildings[visHex];
                    
                    List<BuildingOnVision> buildingViewedByTeam;
                    if (BuildingsOnVisionRangeOfTeams.TryGetValue(team.Number, out buildingViewedByTeam))
                    {
                        if(! buildingViewedByTeam.Contains(buildingComps))
                        {
                            buildingViewedByTeam.Add(buildingComps);
                            BuildingsOnVisionRangeOfTeams[team.Number] = buildingViewedByTeam;
                        }                        
                    }
                    else 
                    {
                        var newBuildingViewedByTeam = new List<BuildingOnVision>() { buildingComps } ;
                        BuildingsOnVisionRangeOfTeams.Add(team.Number, newBuildingViewedByTeam);
                    }
                    //add to hash set.
                    HashSet<Entity> entitiesOnSightOfTeamHashSet;
                    if (EntitiesOnVisionRangeOfTeamsHashset.TryGetValue(team.Number, out entitiesOnSightOfTeamHashSet))
                    {
                        if (!entitiesOnSightOfTeamHashSet.Contains(buildingComps.entity))
                        {
                            entitiesOnSightOfTeamHashSet.Add(buildingComps.entity);
                            EntitiesOnVisionRangeOfTeamsHashset[team.Number] = entitiesOnSightOfTeamHashSet;
                        }
                    }
                    else
                    {
                        entitiesOnSightOfTeamHashSet = new HashSet<Entity>() { buildingComps.entity };
                        EntitiesOnVisionRangeOfTeamsHashset.Add(team.Number, entitiesOnSightOfTeamHashSet);
                    }
                }

                //substitutes to hashset.
                if (obserbableSubstitutes.ContainsKey(visHex)) 
                {
                    var subst = obserbableSubstitutes[visHex]; 

                    HashSet<Entity> entitiesOnSightOfTeamHashSet;
                    if (EntitiesOnVisionRangeOfTeamsHashset.TryGetValue(team.Number, out entitiesOnSightOfTeamHashSet))
                    {
                        if (!entitiesOnSightOfTeamHashSet.Contains(subst.entity))
                        {
                            entitiesOnSightOfTeamHashSet.Add(subst.entity);
                            EntitiesOnVisionRangeOfTeamsHashset[team.Number] = entitiesOnSightOfTeamHashSet;
                        }
                    }
                    else
                    {
                        entitiesOnSightOfTeamHashSet = new HashSet<Entity>() { subst.entity };
                        EntitiesOnVisionRangeOfTeamsHashset.Add(team.Number, entitiesOnSightOfTeamHashSet);
                    }
                }
            }

            //UNIDADES.
            var observablePointsOnRange = SpartialSortUtils.GetAllPointsAtRange(position.Round(), sightDistanceInHex, observablePointsSortedByHex);
            foreach (var point in observablePointsOnRange)
            {
                var positionOfPoint = point.position;
                int teamOfPoint = teamsOfObservableUnits[point.index];
                Collider colliderOfPoint = collidersOfObservableUnits[point.index];
                Fix64 radiusOfPoint = colliderOfPoint.Radius;
                Entity entityOfPoint = entitiesOfObservableUnits[point.index];


                var distance = position.Distance(positionOfPoint);
                bool onSight = distance <= sightRange.Value + radiusOfPoint;
                if (onSight)
                {
                    var pointToAdd = new EntityOnVision(entityOfPoint, colliderOfPoint, teamOfPoint, positionOfPoint);

                    //add the entity on the list dictionary if it is not there already.
                    List<EntityOnVision> entitiesOnSightOfTeam;
                    if (UnitsOnVisionRangeOfTeams.TryGetValue(team.Number, out entitiesOnSightOfTeam))
                    {
                        if (! entitiesOnSightOfTeam.Contains(pointToAdd))
                        {
                            entitiesOnSightOfTeam.Add(pointToAdd);
                            UnitsOnVisionRangeOfTeams[team.Number] = entitiesOnSightOfTeam;
                        }
                    }
                    else
                    {
                        entitiesOnSightOfTeam = new List<EntityOnVision>() { pointToAdd };
                        UnitsOnVisionRangeOfTeams.Add(team.Number, entitiesOnSightOfTeam);
                    }


                    //Add the entity to the hash set dicionary if it is not there already.
                    HashSet<Entity> entitiesOnSightOfTeamHashSet;
                    if (EntitiesOnVisionRangeOfTeamsHashset.TryGetValue(team.Number, out entitiesOnSightOfTeamHashSet))
                    {
                        if (!entitiesOnSightOfTeamHashSet.Contains(entityOfPoint))
                        {
                            entitiesOnSightOfTeamHashSet.Add(entityOfPoint);
                            EntitiesOnVisionRangeOfTeamsHashset[team.Number] = entitiesOnSightOfTeamHashSet;
                        }
                    }
                    else
                    {
                        entitiesOnSightOfTeamHashSet = new HashSet<Entity>() { entityOfPoint };
                        EntitiesOnVisionRangeOfTeamsHashset.Add(team.Number, entitiesOnSightOfTeamHashSet);
                    }
                    
                }
            }
        });

    }
}
