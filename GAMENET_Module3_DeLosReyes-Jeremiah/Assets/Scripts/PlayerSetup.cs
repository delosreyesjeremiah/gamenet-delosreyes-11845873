using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class PlayerSetup : MonoBehaviourPunCallbacks
{
    public Camera Camera;
    public TextMeshProUGUI PlayerNameTextUI;

    private void Start()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue("rc"))
        {
            GetComponent<PlayerVehicleMovement>().enabled = photonView.IsMine;
            GetComponent<LapController>().enabled = photonView.IsMine;
            Camera.enabled = photonView.IsMine;
        }
        else if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue("dr"))
        {
            GetComponent<PlayerVehicleMovement>().enabled = photonView.IsMine;
            GetComponent<PlayerShooting>().enabled = photonView.IsMine;
            GetComponent<LineRenderer>().enabled = photonView.IsMine;
            Camera.enabled = photonView.IsMine;

            if (photonView.IsMine)
            {
                PlayerNameTextUI.text = "";
            }
            else
            {
                PlayerNameTextUI.text = photonView.Owner.NickName;
            }
        }
    }
}
