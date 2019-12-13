
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;
using ExitGames.Client.Photon;

[RequireComponent(typeof(LauncherUI))]
public class Launcher : MonoBehaviourPunCallbacks
{
    private Dictionary<int, Player> teamDictionary = new Dictionary<int, Player>();

    [SerializeField]
    private bool debug = true;

    #region Private Fields


    /// <summary>
    /// This client's version number. Users are separated from each other by gameVersion (which allows you to make breaking changes).
    /// </summary>
    private string gameVersion = "1";
    public int levelToJoinIndex = 1;
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
        if (isConecting)
        {
            PhotonNetwork.JoinRandomRoom(null, 2);
        }


        if (debug) Debug.Log("Launcher: OnConnectedToMaster() was called by PUN");
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
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = roomMaxSize });


        if (debug) Debug.Log("Launcher:OnJoinRandomFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");
    }
    public override void OnJoinedRoom()
    {
        if (debug) Debug.Log("Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");
    }
    public override void OnCreatedRoom()
    {
        //team assignement
        if (PhotonNetwork.IsMasterClient)
        {
            teamDictionary.Clear();
            teamDictionary.Add(0, PhotonNetwork.LocalPlayer);
        }
    }    
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        //team assignement
        teamDictionary.Clear();
        var players = PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Length; i++)
        {
            teamDictionary.Add(i, players[i]);
        }
    }
    public override void OnPlayerEnteredRoom(Player other)
    {
        //team assignement
        if (PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
            {
                if (!teamDictionary.TryGetValue(i, out Player player))
                {
                    teamDictionary.Add(i, other);
                    break;
                }
            }
        }

        if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                AsignTheTeamForEachPlayer();
                LoadLevel(levelToJoinIndex);
            }
        }

        if (debug) Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName);
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        //team assignement
        foreach (var keyValuePair in teamDictionary)
        {
            var playerNum = keyValuePair.Value;
            if (otherPlayer == playerNum)
            {
                teamDictionary.Remove(keyValuePair.Key);
                break;
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
            PhotonNetwork.JoinRandomRoom(null, 2);
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
    private void AsignTheTeamForEachPlayer()
    {
        foreach (var item in teamDictionary)
        {
            var currPlayer = item.Value;

            currPlayer.SetCustomProperties(new Hashtable() { { "team", item.Key } });
        }
    }
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
