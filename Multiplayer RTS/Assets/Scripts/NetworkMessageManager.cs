using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class NetworkMessageManager : MonoBehaviourPunCallbacks
{

    public static NetworkMessageManager Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = (NetworkMessageManager)FindObjectOfType(typeof(NetworkMessageManager));
                if (m_Instance == null)
                {
                    Debug.Log("Creating a NetworkMessageManager gameobject");
                    var singletonObject = new GameObject();
                    m_Instance = singletonObject.AddComponent<NetworkMessageManager>();
                    singletonObject.name = typeof(NetworkMessageManager).ToString() + " (Singleton)";
                }
            }
            return m_Instance;
        }
    }
    private static NetworkMessageManager m_Instance;





    [SerializeField]
    private string syncRPCMethodName = "SimpleSyncMessage";
    [SerializeField]
    private string confirmationRPCMethodName = "ReceivedConfirmationMessage";

    public void CallRPC(int turnToExecute, MessageType type)
    {
        switch (type)
        {
            case MessageType.COMMAND_OTHER:
                photonView.RPC(syncRPCMethodName, RpcTarget.Others, turnToExecute);
                break;
            case MessageType.CONFIRMATION:
                photonView.RPC(confirmationRPCMethodName, RpcTarget.Others, turnToExecute);
                break;
            default:
                Debug.LogError("invalidMessageType");
                break;
        }
    }
    #region RPC
    //builded with 1v1 in mind.

    [PunRPC]
    public void SimpleSyncMessage(int lockstepTurn)
    {
        EntityManager entityManager = World.Active.EntityManager;

        Debug.LogWarning("Creating a entity and a component in main thread. Forced Sync point, decreases performance");
        Entity entity = entityManager.CreateEntity();

        entityManager.AddComponentData(entity, new Message() { Type = MessageType.COMMAND_SELF });
        entityManager.AddSharedComponentData(entity, new TurnToExecute() { turnToExecute = lockstepTurn });



        //send confirmation message
        CallRPC(lockstepTurn, MessageType.CONFIRMATION);
    }
    [PunRPC]
    public void ReceivedConfirmationMessage(int lockstepTurn)
    {
        EntityManager entityManager = World.Active.EntityManager;

        Debug.LogWarning("Creating a entity and a component in main thread. Forced Sync point, decreases performance");
        Entity entity = entityManager.CreateEntity();

        entityManager.AddComponentData(entity, new Message() { Type = MessageType.CONFIRMATION });
        entityManager.AddSharedComponentData(entity, new TurnToExecute() { turnToExecute = lockstepTurn });
    }

    #endregion
}
