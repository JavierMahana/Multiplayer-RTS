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
    #region Private Serializable Fields

    [SerializeField]
    private GameObject AlertButton;

    #endregion

    #region Unity CallBacks
    private void Start()
    {
        AlertButton.SetActive(false);
    }
    #endregion

    #region Photon Callbacks

    public override void OnPlayerLeftRoom(Player other)
    {
        PlayerLeftAlert();
    }

    /// <summary>
    /// Called when the local player left the room. We need to load the launcher scene.
    /// </summary>
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }


    #endregion

    #region Private Methods

    private void PlayerLeftAlert()
    {
        AlertButton.SetActive(true);
    }

    #endregion

    #region Public Methods



    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }


    #endregion
}
