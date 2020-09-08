using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    // create the instance
    public static NetworkManager instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            gameObject.SetActive(false);
        }
        else
        {
            // set the instance
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    // Connect to master server
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    // attempt to create a room
    public void CreateRoom(string roomName)
    {
        PhotonNetwork.CreateRoom(roomName);
    }

    // Join an existing room
    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    // Change the scene
    [PunRPC]
    public void ChangeScene(string sceneName)
    {
        PhotonNetwork.LoadLevel(sceneName);
    }
}
