using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class GameManager : MonoBehaviourPunCallbacks
{
    [Header("Stats")]
    public bool gameEnded = false;
    public float timeToWin;
    public float invincibleDuration;
    private float hatPickupTime;

    [Header("Players")]
    public string playerPrefabLocation1;
    public string playerPrefabLocation2;
    public string playerPrefabLoc3;
    public string playerPrefabLoc4;
    public Transform[] spawnPoints;
    public PlayerController[] players;
    public int playerWithHat;
    private int playersInGame;

    // instance
    public static GameManager instance;

    private void Awake()
    {
        // instance
        instance = this;
    }

    private void Start()
    {
        players = new PlayerController[PhotonNetwork.PlayerList.Length];
        photonView.RPC("ImInGame", RpcTarget.All);
    }

    [PunRPC]
    void ImInGame()
    {
        playersInGame++;

        // When all players are in screen, spawn players
        if (playersInGame == PhotonNetwork.PlayerList.Length)
            SpawnPlayer();
    }

    void SpawnPlayer()
    {
        GameObject playerObj;

        // maybe change to not be random?
        // instantiate the player across the network
        switch (PhotonNetwork.LocalPlayer.ActorNumber)
        {
            case 1:
                playerObj = PhotonNetwork.Instantiate(playerPrefabLocation1, spawnPoints[PhotonNetwork.LocalPlayer.ActorNumber-1].position, Quaternion.identity);
                break;
            case 2:
                playerObj = PhotonNetwork.Instantiate(playerPrefabLocation2, spawnPoints[PhotonNetwork.LocalPlayer.ActorNumber-1].position, Quaternion.identity);
                break;
            case 3:
                playerObj = PhotonNetwork.Instantiate(playerPrefabLoc3, spawnPoints[PhotonNetwork.LocalPlayer.ActorNumber-1].position, Quaternion.identity);
                break;
            case 4:
                playerObj = PhotonNetwork.Instantiate(playerPrefabLoc4, spawnPoints[PhotonNetwork.LocalPlayer.ActorNumber-1].position, Quaternion.identity);
                break;
            default:
                playerObj = PhotonNetwork.Instantiate(playerPrefabLocation1, spawnPoints[PhotonNetwork.LocalPlayer.ActorNumber].position, Quaternion.identity);
                break;
        }

        // get the player script
        PlayerController playerScript = playerObj.GetComponent<PlayerController>();

        // initialize the player
        playerScript.photonView.RPC("Initialize", RpcTarget.All, PhotonNetwork.LocalPlayer);
    }

    public PlayerController GetPlayer (int playerId)
    {
        return players.First(x => x.id == playerId);
    }

    public PlayerController GetPlayer(GameObject playerObj)
    {
        return players.First(x => x.gameObject == playerObj);
    }

    // called when a player hits the hatted player
    [PunRPC]
    public void GiveHat( int playerId, bool initialGive)
    {
        // remove the hat from current player
        if (!initialGive)
            GetPlayer(playerWithHat).SetHat(false);

        // give hat to new player
        playerWithHat = playerId;
        GetPlayer(playerId).SetHat(true);
        hatPickupTime = Time.time;
    }

    // Is it passble outside of invincible time?
    public bool CanGetHat()
    {
        if (Time.time > hatPickupTime + invincibleDuration)
            return true;
        else
            return false;
    }

    [PunRPC]
    void WinGame (int playerId)
    {
        gameEnded = true;
        PlayerController player = GetPlayer(playerId);

        GameUI.instance.SetWinText(player.photonPlayer.NickName);

        Invoke("GoBackToMenu", 3f);
    }

    void GoBackToMenu ()
    {
        PhotonNetwork.LeaveRoom();
        NetworkManager.instance.ChangeScene("Menu");
    }
}
