using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Photon.Pun;

//preocupaciones -
// que pasa si se sale alguien de la sala -> la partida termina y gana el que se queda
// que pasa si es de más de dos jugdores -> es necesario que sea suficientemente general la forma
// que pasa si se sale el host -> la sala se destrulle y los que estaban en ella deben volver a buscar una sala o crear una nueva


public class TeamManager : MonoBehaviourPunCallbacks
{

}
