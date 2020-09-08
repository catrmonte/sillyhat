﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    [HideInInspector]
    public int id;

    [Header("Info")]
    public float moveSpeed;
    public float jumpForce;
    public GameObject hatObject;

    [HideInInspector]
    public float curHatTime;

    [Header("Components")]
    public Rigidbody rig;
    public Player photonPlayer;

    [PunRPC]
    public void Initialize (Player player)
    {
        photonPlayer = player;
        id = player.ActorNumber;

        GameManager.instance.players[id - 1] = this;

        // give first player the hat
        if (id == 1)
            GameManager.instance.GiveHat(id, true);


        // If this isn't our local player, disable phsycis as that's 
        // controlled by the user and synced to all other clients
        if (!photonView.IsMine)
        {
            rig.isKinematic = true;
        }
    }


    void Update()
    {
        // Add to hat score 
        if (PhotonNetwork.IsMasterClient)
        {
            if (curHatTime >= GameManager.instance.timeToWin && !GameManager.instance.gameEnded)
            {
                GameManager.instance.gameEnded = true;
                GameManager.instance.photonView.RPC("WinGame", RpcTarget.All, id);
            }
        }

        Move();

        if (Input.GetKeyDown(KeyCode.Space))
            TryJump();

        // track the amount of time wearing the hat
        if (hatObject.activeInHierarchy)
        {
            curHatTime += Time.deltaTime;
        }
    }

    void Move()
    {
        float x = Input.GetAxis("Horizontal") * moveSpeed;
        float z = Input.GetAxis("Vertical") * moveSpeed;

        rig.velocity = new Vector3(x, rig.velocity.y, z);
    }

    void TryJump()
    {
        Ray ray = new Ray(transform.position, Vector3.down);

        if (Physics.Raycast(ray, 0.7f))
        {
            rig.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    // sets the hat as active or not
    public void SetHat (bool hasHat)
    {
        hatObject.SetActive(hasHat);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!photonView.IsMine)
            return;

        // did we hit another player?
        if(collision.gameObject.CompareTag("Player"))
        {
            //do they have the hat?
            // do we have the hat?
            if(id == GameManager.instance.playerWithHat)
            {
                // can we give the hat?
                if (GameManager.instance.CanGetHat())
                {
                    // give the hat
                    GameManager.instance.photonView.RPC("GiveHat", RpcTarget.All, GameManager.instance.GetPlayer(collision.gameObject).id, false);
                }
            }
        }
    }

    public void OnPhotonSerializeView (PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(curHatTime);
        }
        else if (stream.IsReading)
        {
            curHatTime = (float)stream.ReceiveNext();
        }
    }
}