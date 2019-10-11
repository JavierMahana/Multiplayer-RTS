using ExitGames.Client.Photon;
using Photon.Realtime;
using Photon.Pun;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
using UnityEngine;

public class NetworkEventCatchingSystem : ComponentSystem, IOnEventCallback
{
    public const byte EmptyCommandEventCode = 1;
    public const byte ReceivedCommandConfirmationEventCode = 2;

    public void OnEvent(EventData photonEvent)
    {

        //debe crear los mensajes correspondientes a la info que le llega en el evento.
        //los mensajes luego deben tener info para ejecutar comandos, pero eso despues.
        //Ahora los mensajes son ultra sensillos, unicamente deben tener un byte para identificar que tipo de mensaje es.
        //son empty
        if (photonEvent.Code == EmptyCommandEventCode) { EmptyCommandCallback(photonEvent); }
        else if (photonEvent.Code == ReceivedCommandConfirmationEventCode) { ReceivedCommandConfirmationCallback(photonEvent); }
    }

    protected override void OnCreate()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }
    protected override void OnDestroy()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }
    protected override void OnUpdate()
    {
        
    }

    #region Network EventCallBacks
    //event data layout:
    //it must be a object array.
    //and always the first element is the turn of execution.
    private void EmptyCommandCallback(EventData photonEvent) 
    {
        object[] eventData = (object[])photonEvent.CustomData;
        int turnToExecute = (int)eventData[0];

        var entity = EntityManager.CreateEntity(typeof(Message));
        EntityManager.SetComponentData(entity, new Message() { TurnToExecute = turnToExecute, Type = MessageType.COMMAND_OTHER });

        Debug.Log($"EmptyCommand recieved For Turn {turnToExecute}");
    }
    private void ReceivedCommandConfirmationCallback(EventData photonEvent)
    {
        object[] eventData = (object[])photonEvent.CustomData;
        int turnToExecute = (int)eventData[0];

        var entity = EntityManager.CreateEntity(typeof(Message));
        EntityManager.SetComponentData(entity, new Message() { TurnToExecute = turnToExecute, Type = MessageType.CONFIRMATION });

        Debug.Log($"Recieved command Confirmation recieved For Turn {turnToExecute}");
    }

    #endregion

}