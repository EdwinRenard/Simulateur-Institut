using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NetworkController : Photon.MonoBehaviour {

    private string _gameVersion = "0.1";
    public GameObject[] playerPrefab;
    public GameObject[] playerSpawn;

    //public GameObject[] lifeBar;
    public Slider[] lifeSliders;
    public Text[] playerName;
    public bool test;

    private int nbPlayer = 0;

    public PhotonPlayer[] playerList;

    //Syn: Gui Controll
    public GameObject guiHolder;
    private GUIController guiController;

    // Use this for initialization
    void Start () {
        PhotonNetwork.ConnectUsingSettings(_gameVersion);
        guiController = guiHolder.GetComponent<GUIController>();
    }

    void Update()
    {

        //appel de retour --> voir doc photon
    }
    void OnJoinedLobby()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    void OnPhotonRandomJoinFailed()
    { 
        PhotonNetwork.CreateRoom(null);
      
    }

    void OnJoinedRoom()
    {
      
        nbPlayer = PhotonNetwork.player.ID % 4;
        GameObject localPLayer = PhotonNetwork.Instantiate("Vehicles/" + playerPrefab[nbPlayer-1].name, playerSpawn[nbPlayer-1].transform.position, Quaternion.identity, 0);
        guiController.updateNbrePlayers();
        guiController.setLocalPlayer(localPLayer);
        playerStatStart(nbPlayer);


    }

    private void playerStatStart(int prefabid)
    {

        PhotonNetwork.player.SetTeam((prefabid != 0) ? PunTeams.Team.blue : PunTeams.Team.red);
    }

    void playerUpdateLife(int lifeToAdd)
    {
        PhotonNetwork.player.CustomProperties["life"] = (int)PhotonNetwork.player.CustomProperties["life"] + lifeToAdd;
    }
    
    //Syn : Update Life
    void OnPhotonPlayerConnected()
    {
        guiController.updateNbrePlayers();
    }

    void OnPhotonPlayerDisconnected()
    {
        guiController.updateNbrePlayers();
    }

}
