using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class Menu : MonoBehaviourPunCallbacks
{
    [Header("Screens")]
    public GameObject mainScreen;
    public GameObject lobbyScreen;

    [Header("Main Screen")]
    public Button createRoomButton;
    public Button joinRoomButton;

    [Header("Lobby Screen")]
    public TextMeshProUGUI playerListText;
    public Button startGameButton;

    void Start()
    {
        createRoomButton.interactable = false;
        joinRoomButton.interactable = false;
    }

    // when connected to server, enable create and join room buttons
    public override void OnConnectedToMaster()
    {
        createRoomButton.interactable = true;
        joinRoomButton.interactable = true;
    }

    void SetScreen(GameObject screen)
    {
        // deactivate all screens
        mainScreen.SetActive(false);
        lobbyScreen.SetActive(false);

        // enable the requested screen
        screen.SetActive(true);
    }

    public void OnCreateRoomButton(TMP_InputField roomNameInput)
    {
        NetworkManager.instance.CreateRoom(roomNameInput.text);
        // Debug.Log("creating room called " + roomNameInput.text);
    }

    public void OnJoinRoomButton(TMP_InputField roomNameInput)
    {
        NetworkManager.instance.JoinRoom(roomNameInput.text);
    }

    public void OnPlayerNameUpdate(TMP_InputField playerNameInput)
    {
        PhotonNetwork.NickName = playerNameInput.text;
    }

    public override void OnJoinedRoom()
    {
        SetScreen(lobbyScreen);

        // since there's a new player in the lobby, update UI
        photonView.RPC("UpdateLobbyUI", RpcTarget.All);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // Not an RPC call because all clients already know when 
        // player leaves the room
        UpdateLobbyUI();
    }

    [PunRPC]
    public void UpdateLobbyUI()
    {
        playerListText.text = "";

        // diplay all players currently in lobby
        foreach(Player player in PhotonNetwork.PlayerList)
        {
            playerListText.text += player.NickName + "\n";
        }

        // Only host can start the game
        if (PhotonNetwork.IsMasterClient)
            startGameButton.interactable = true;
        else
            startGameButton.interactable = false;
    }

    public void OnLeaveLobbyButton()
    {
        PhotonNetwork.LeaveLobby();
        SetScreen(mainScreen);
    }

    public void OnStartGameButton()
    {
        NetworkManager.instance.photonView.RPC("ChangeScene", RpcTarget.All, "Game");
    }
}
