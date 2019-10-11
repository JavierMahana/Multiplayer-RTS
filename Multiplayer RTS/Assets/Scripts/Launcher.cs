using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System;

public class Launcher : MonoBehaviourPunCallbacks
{

    #region Private SerializableFields    
    [SerializeField]
    private GameObject connectButton;
    [SerializeField]
    private GameObject connectingLabel;
    [SerializeField]
    private GameObject waitingForPlayersLabel;
    [SerializeField]
    private Text waitingForPlayersText;
    [SerializeField]
    private GameObject cancelButton;
    #endregion
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

    void Start()
    {
        connectButton.SetActive(true);
        connectingLabel.SetActive(false);
        waitingForPlayersLabel.SetActive(false);
        cancelButton.SetActive(false);
    }
    #endregion

    #region PUN CallBacks
    public override void OnConnectedToMaster()
    {
        Debug.Log("Launcher: OnConnectedToMaster() was called by PUN");

        if (isConecting)
        {
            PhotonNetwork.JoinRandomRoom();
        }
    }
    public override void OnLeftRoom()
    {
        Debug.Log("Launcher: left waiting room");
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarningFormat("Launcher: OnDisconnected() was called by PUN with reason {0}", cause);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Launcher:OnJoinRandomFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = roomMaxSize });
    }
    public override void OnJoinedRoom()
    {
        connectButton.SetActive(false);
        connectingLabel.SetActive(false);
        waitingForPlayersLabel.SetActive(true);
        cancelButton.SetActive(true);
        UpdateWaitingForPlayersLabel();

        Debug.Log("Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");
    }

    public override void OnPlayerEnteredRoom(Player other)
    {
        Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName);
        UpdateWaitingForPlayersLabel();

        if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                LoadLevel(levelToJoinIndex);
            }
        }
    }


    #endregion
    #region Public Methods
    public void Connect()
    {        
        connectButton.SetActive(false);
        connectingLabel.SetActive(true);
        waitingForPlayersLabel.SetActive(false);
        cancelButton.SetActive(false);

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
        connectButton.SetActive(true);
        connectingLabel.SetActive(false);
        waitingForPlayersLabel.SetActive(false);
        cancelButton.SetActive(false);

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
    private void UpdateWaitingForPlayersLabel()
    {
        waitingForPlayersText.text = string.Format("Waiting for Players: {0}|{1}",
            PhotonNetwork.CurrentRoom.PlayerCount, PhotonNetwork.CurrentRoom.MaxPlayers);
    }
    #endregion
}
