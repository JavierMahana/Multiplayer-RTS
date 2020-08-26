using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;

using Unity.Entities;

using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static List<int> PlayerTeams { get; private set; } = new List<int>();     
    public bool togleOfflineMode;
    [ShowIf("i_ShowControlAllTeamsFlag")]
    public bool controlAllTeamsOnOfflineMode = true;
    [ShowIf("i_ShowListOfControlablableTeam")]
    public List<int> offlineTeams = new List<int>();

    //INSPECTOR PROPERTIES;
    private bool i_ShowControlAllTeamsFlag => togleOfflineMode;
    private bool i_ShowListOfControlablableTeam => i_ShowControlAllTeamsFlag && !controlAllTeamsOnOfflineMode;

    #region Unity CallBacks
    private void Start()
    {
        if (togleOfflineMode) { OfflineMode.SetOffLineMode(true); }

        if (PhotonNetwork.IsConnected)
        {
            SetTeams();
            StartSimulation();
        }
        else if (OfflineMode.OffLineMode)
        {
            SetTeamsOnOfflineMode();
            StartSimulation();
        }
        else
        {
            Debug.LogWarning("Game cant start without connection! Put offlineMode or connect Going back to the launcher");
            SceneManager.LoadScene(0);
        }
    }
    #endregion

    #region Photon Callbacks
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        EndSimulation();
    }
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }


    #endregion


    #region Private Methods
    private void SetTeams()
    {
        var playerProperties = PhotonNetwork.LocalPlayer.CustomProperties;
        if (playerProperties.TryGetValue("team", out object team))
        {
            PlayerTeams.Add((int)team);
        }
        else 
        {
            Debug.LogError("the properties of the player don't containt a team element");
        }
    }
    private void SetTeamsOnOfflineMode()
    {
        //check de seguridad, por si el usuario no pone un equipo para la lista de equipos, el modo offline te da acceso a todos los equipos.
        bool offlineTeamsListExistAndIsNotEmpty = offlineTeams != null;
        if (offlineTeamsListExistAndIsNotEmpty) offlineTeamsListExistAndIsNotEmpty = offlineTeams.Count != 0;

        if (controlAllTeamsOnOfflineMode || ! offlineTeamsListExistAndIsNotEmpty)
        {
            var teamQuerry = World.Active.EntityManager.CreateEntityQuery(typeof(Team));

            var allTeamComponents = teamQuerry.ToComponentDataArray<Team>(Unity.Collections.Allocator.TempJob);

            List<int> listOfAllTeams = new List<int>();
            foreach (var team in allTeamComponents)
            {
                if (listOfAllTeams.Contains(team.Number))
                {
                    continue;
                }
                else
                {
                    listOfAllTeams.Add(team.Number);
                }
            }
            allTeamComponents.Dispose();
            PlayerTeams = listOfAllTeams;
        }
        //HAY UNA LISTA DE EQUIPOS A CONTROLAR
        else 
        {
            PlayerTeams = offlineTeams;
        }
    }
    private void StartSimulation()
    {
        World.Active.EntityManager.CreateEntity(typeof(Simulate));
    }
    private void EndSimulation()
    {
        EntityManager em = World.Active.EntityManager;
        em.DestroyEntity(em.CreateEntityQuery(typeof(Simulate)));
    }

    #endregion

    #region Button Methods
    public void LeaveRoom()
    {
        EndSimulation();
        if (OfflineMode.OffLineMode) 
        {
            SceneManager.LoadScene(0);
        }
        
        else 
        {
            PhotonNetwork.LeaveRoom();
        }
    }

    #endregion
}
