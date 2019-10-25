using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.SceneManagement;

//public class EventTester : MonoBehaviourPunCallbacks, IOnEventCallback
//{
//    [SerializeField]
//    private bool sendEventInsideASystem = false;


//    [SerializeField]
//    private bool debugEventRecieved = false;
//    [SerializeField]
//    private bool debugEventSended = false;

//    [SerializeField]
//    private bool sendEventToSelf = false;
//    [SerializeField]
//    private byte idOfEvent = 0;

//    private NetworkEventCatchingSystem networkEventCatchingSystem;
//    private bool isConecting;

//    private void Start()
//    {
//        return;
//        if (SceneManager.GetActiveScene().buildIndex == 0 )
//        {

//            SimulationSystemGroup simGroup = World.Active.GetOrCreateSystem<SimulationSystemGroup>();
//            networkEventCatchingSystem = World.Active.CreateSystem<NetworkEventCatchingSystem>();
//            simGroup.AddSystemToUpdateList(networkEventCatchingSystem);
//        }
//    }

//    public void OnEvent(EventData photonEvent)
//    {        
//        if (photonEvent.Code == (byte)NetworkEventTypes.TestEvent) { TestEventRecieved(photonEvent); }
//    }

//    private void TestEventRecieved(EventData photonEvent)
//    {
//        byte id = (byte)photonEvent.CustomData;
//        if(debugEventRecieved) Debug.Log($"test event recieved in monobehaviour. ID: {id}");
//    }


//    #region pun callbacks
//    public override void OnConnectedToMaster()
//    {
//        Debug.Log("Launcher: OnConnectedToMaster() was called by PUN");

//        if (isConecting)
//        {
//            PhotonNetwork.JoinRandomRoom();
//        }
//    }
//    public override void OnLeftRoom()
//    {
//        Debug.Log("Launcher: left waiting room");
//    }
//    public override void OnDisconnected(DisconnectCause cause)
//    {
//        Debug.LogWarningFormat("Launcher: OnDisconnected() was called by PUN with reason {0}", cause);
//    }

//    public override void OnJoinRandomFailed(short returnCode, string message)
//    {
//        Debug.Log("Launcher:OnJoinRandomFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");
//        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 2 });
//    }
//    public override void OnJoinedRoom()
//    {
//        Debug.Log("Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");
//    }
//    #endregion
//    #region Button Methods
//    public void ToggleSendEventInsideASystem(bool togle)
//    {
//        sendEventInsideASystem = togle;
//    }
//    public void ConnectToNetwork()
//    {
//        isConecting = true;
//        if (PhotonNetwork.IsConnected)
//        {
//            PhotonNetwork.JoinRandomRoom();
//        }
//        else
//        {
//            PhotonNetwork.ConnectUsingSettings();
//        }

        
//    }

//    public void ChangeScene(int sceneNumber)
//    {
//        World.Active.EntityManager.DestroyEntity(World.Active.EntityManager.UniversalQuery);
//        SceneManager.LoadScene(sceneNumber);
//    }
//    public void CreateEmptyEntity()
//    {
//        World.Active.EntityManager.CreateEntity();
        
//    }
//    public void CreateSimulateEnablerEntity()
//    {
//        World.Active.EntityManager.CreateEntity(typeof(Simulate));
//    }
//    public void DestroySimulateEntity()
//    {
//        World.Active.EntityManager.DestroyEntity(World.Active.EntityManager.CreateEntityQuery(typeof(Simulate)));
//    }
//    public void SendTestEvent()
//    {
//        if (sendEventInsideASystem)
//        {
//            EntityManager em = World.Active.EntityManager;
//            Entity e = em.CreateEntity(typeof(TestEventSender));
//            em.SetComponentData(e, new TestEventSender() { id = idOfEvent, sendSelf = sendEventToSelf, debug = debugEventSended });
//        }
//        else
//        {
//            byte eCode = (byte)NetworkEventTypes.TestEvent;
//            byte content = idOfEvent;
//            RaiseEventOptions options = new RaiseEventOptions() { Receivers = sendEventToSelf ? ReceiverGroup.All : ReceiverGroup.Others };
//            SendOptions sendOptions = new SendOptions() { Reliability = true };
//            PhotonNetwork.RaiseEvent(eCode, content, options, sendOptions);

//            if (debugEventSended) Debug.Log($"event sended. code: {eCode}; id: {content}; reciever: {options.Receivers}");            
//        }
//        idOfEvent++;
//    }
//    #endregion
//}
