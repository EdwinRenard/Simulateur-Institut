using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUIController : MonoBehaviour
{

    Text statusText, masterText, lifeText, nbPlayers;
    //private PlayerScript player;
    // Use this for initialization
    void Start()
    {
        statusText = GameObject.Find("statusText").GetComponent<Text>();
        masterText = GameObject.Find("masterText").GetComponent<Text>();
        lifeText = GameObject.Find("lifeText").GetComponent<Text>();
        nbPlayers = GameObject.Find("nbPlayers").GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        statusText.text = "Status : " + PhotonNetwork.connectionStateDetailed.ToString();
        masterText.text = "isMasterClient : " + PhotonNetwork.isMasterClient;
        /*if (player != null)
        {
            lifeText.text = "Life : " + player.getLife();
        }*/

    }
    public void setLocalPlayer(GameObject localPLayer)
    {
    
       // player = localPLayer.GetComponent<PlayerScript>();
    }

    public void updateNbrePlayers()
    {
        int nbrJoueurs = PhotonNetwork.playerList.Length;
        nbPlayers.text = "Nb Players : " + nbrJoueurs.ToString();
    }

}