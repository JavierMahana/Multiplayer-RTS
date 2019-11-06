using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Entities;

using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviourPunCallbacks
{
    public bool togleOfflineMode;

    #region Unity CallBacks
    private void Start()
    {
        if (togleOfflineMode) { OfflineMode.SetOffLineMode(true); }

        if (PhotonNetwork.IsConnected)
        {
            StartSimulation();
        }
        else if (OfflineMode.OffLineMode)
        {
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
