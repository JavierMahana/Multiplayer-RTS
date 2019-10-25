using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(LauncherUI))]
public class Launcher : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private bool debug = true;

    #region Private Fields


    /// <summary>
    /// This client's version number. Users are separated from each other by gameVersion (which allows you to make breaking changes).
    /// </summary>
    private string gameVersion = "1";
    private int levelToJoinIndex = 1;
    private bool isConecting;
    private const int roomMaxSize = 2;



    #endregion

    #region Monobehaviour Callbacks
    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    #endregion

    #region PUN CallBacks
    public override void OnConnectedToMaster()
    {
        if(debug)Debug.Log("Launcher: OnConnectedToMaster() was called by PUN");

        if (isConecting)
        {
            PhotonNetwork.JoinRandomRoom(null, 2);
        }
    }
    public override void OnLeftRoom()
    {
        if (debug) Debug.Log("Launcher: left waiting room");
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        if (debug) Debug.LogWarningFormat("Launcher: OnDisconnected() was called by PUN with reason {0}", cause);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        if (debug) Debug.Log("Launcher:OnJoinRandomFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = roomMaxSize });
    }
    public override void OnJoinedRoom()
    {
        if (debug) Debug.Log("Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");
    }

    public override void OnPlayerEnteredRoom(Player other)
    {
        if (debug) Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName);
        if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                LoadLevel(levelToJoinIndex);
            }
        }
    }


    #endregion
    #region Button Methods
    public void ToggleOfflineMode(bool value)
    {
        OfflineMode.SetOffLineMode(value);
    }
    public void Connect()
    {
        if (OfflineMode.OffLineMode)
        {
            SceneManager.LoadScene(levelToJoinIndex);
            return;
        }

        isConecting = true;
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            PhotonNetwork.GameVersion = gameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
    }
    public void CancelConnection()
    {
        isConecting = false;
        PhotonNetwork.LeaveRoom();
    }
    #endregion
    #region Private Methods

    private void LoadLevel(int sceneIndex)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogError("Photon Network: Trying to load a level but we are not the master client");
        }
        Debug.LogFormat("Photon Network: Loading level {0}", sceneIndex);
        PhotonNetwork.LoadLevel(sceneIndex);
    }
    #endregion
}
