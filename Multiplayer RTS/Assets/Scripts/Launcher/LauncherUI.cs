using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class LauncherUI : MonoBehaviourPunCallbacks
{

    #region Private SerializableFields    
    [SerializeField]
    private GameObject connectButton = null;
    [SerializeField]
    private GameObject connectingLabel = null;
    [SerializeField]
    private GameObject waitingForPlayersLabel = null;
    [SerializeField]
    private Text waitingForPlayersText = null;
    [SerializeField]
    private GameObject cancelButton = null;
    [SerializeField]
    private Toggle offlineModeToggle = null;
    #endregion
    private bool onDisconected;

    void Start()
    {
        waitingForPlayersText.text = "Waiting for a Rival";
        offlineModeToggle.isOn = OfflineMode.OffLineMode;
        DisconectedView();
    }

    #region PUN CallBacks
    public override void OnLeftRoom()
    {
        DisconectedView();
    }
    public override void OnJoinedRoom()
    {
        ConnectedView();
    }

    #endregion

    public void ConnectingView()    
    {
        connectButton.SetActive(false);
        connectingLabel.SetActive(true);
        waitingForPlayersLabel.SetActive(false);
        cancelButton.SetActive(false);
    }

    public void OfflineConnected() 
    {

    }
    public void ConnectedView()
    {
        connectButton.SetActive(false);
        connectingLabel.SetActive(false);
        waitingForPlayersLabel.SetActive(true);
        cancelButton.SetActive(true);
    }
    public void DisconectedView()
    {
        connectButton.SetActive(true);
        connectingLabel.SetActive(false);
        waitingForPlayersLabel.SetActive(false);
        cancelButton.SetActive(false);
    }


}
