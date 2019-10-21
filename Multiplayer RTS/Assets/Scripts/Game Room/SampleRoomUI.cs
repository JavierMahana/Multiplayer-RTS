using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(GameManager))]
public class SampleRoomUI : MonoBehaviourPunCallbacks
{
    #region Private Serializable Fields

    [SerializeField]
    private GameObject AlertButton = null;

    #endregion
    public override void OnPlayerLeftRoom(Player other)
    {
        PlayerLeftAlert();
    }


    private void Start()
    {
        AlertButton.SetActive(false);
    }


    private void PlayerLeftAlert()
    {
        AlertButton.SetActive(true);
    }
}
