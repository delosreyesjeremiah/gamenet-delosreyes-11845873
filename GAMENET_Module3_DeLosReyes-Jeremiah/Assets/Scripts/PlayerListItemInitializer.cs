using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerListItemInitializer : MonoBehaviour
{
    [Header("UI References")]
    public Text PlayerNameText;
    public Button PlayerReadyButton;
    public Image PlayerReadyImage;

    private bool isPlayerReady = false;

    public void Initialize(int playerID, string playerName)
    {
        PlayerNameText.text = playerName;

        if (PhotonNetwork.LocalPlayer.ActorNumber != playerID)
        {
            PlayerReadyButton.gameObject.SetActive(false);
        }
        else
        {
            SetPlayerReadyProperties();

            PlayerReadyButton.onClick.AddListener(() =>
            {
                isPlayerReady = !isPlayerReady;
                SetPlayerReady(isPlayerReady);
                SetPlayerReadyProperties();
            });
        }
    }

    public void SetPlayerReady(bool playerReady)
    {
        PlayerReadyImage.enabled = playerReady;

        if (playerReady)
        {
            PlayerReadyButton.GetComponentInChildren<Text>().text = "Ready!";
        }
        else
        {
            PlayerReadyButton.GetComponentInChildren<Text>().text = "Ready?";
        }
    }

    private void SetPlayerReadyProperties()
    {
        ExitGames.Client.Photon.Hashtable initializeProperties = new ExitGames.Client.Photon.Hashtable() { { Constants.PLAYER_READY, isPlayerReady } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(initializeProperties);
    }
}
