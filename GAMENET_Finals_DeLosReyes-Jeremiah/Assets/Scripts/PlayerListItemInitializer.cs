using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class PlayerListItemInitializer : MonoBehaviour
{
    public PlayerSelection PlayerSelection
    {
        get => playerSelection;
        set => playerSelection = value;
    }

    public Button PlayerSelectionNextButton
    {
        get => playerSelectionNextButton;
        set => playerSelectionNextButton = value;
    }

    public Button PlayerSelectionPreviousButton
    {
        get => playerSelectionPreviousButton;
        set => playerSelectionPreviousButton = value;
    }

    [Header("UI References")]
    [SerializeField] private Text playerNameText;
    [SerializeField] private Button playerReadyButton;
    [SerializeField] private Image playerReadyImage;

    private PlayerSelection playerSelection;

    private Button playerSelectionNextButton;
    private Button playerSelectionPreviousButton;

    private bool isPlayerReady = false;

    public void Initialize(int playerID, string playerName)
    {
        playerNameText.text = playerName;

        if (PhotonNetwork.LocalPlayer.ActorNumber != playerID)
        {
            playerReadyButton.gameObject.SetActive(false);
        }
        else
        {
            SetPlayerReadyImageProperties();
            HandlePlayerReadyButtonToggle();
            AddButtonOnClickListenerToTogglePlayerReady(playerSelectionNextButton, false);
            AddButtonOnClickListenerToTogglePlayerReady(playerSelectionPreviousButton, false);
            playerSelection.SetSelectablePlayerProperties(-1);
        }
    }

    public void SetPlayerReadyImage(bool isPlayerReady)
    {
        playerReadyImage.enabled = isPlayerReady;

        if (isPlayerReady)
        {
            playerReadyButton.GetComponentInChildren<Text>().text = "Ready!";
        }
        else
        {
            playerReadyButton.GetComponentInChildren<Text>().text = "Ready?";
        }
    }

    private void SetPlayerReadyImageProperties()
    {
        ExitGames.Client.Photon.Hashtable initializeProperties = new ExitGames.Client.Photon.Hashtable() { { Constants.PLAYER_READY, isPlayerReady } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(initializeProperties);
    }

    private void AddButtonOnClickListenerToTogglePlayerReady(Button button, bool value)
    {
        button.onClick.AddListener(() =>
        {
            isPlayerReady = value;
            SetPlayerReadyImage(value);
            SetPlayerReadyImageProperties();
        });
    }

    private void HandlePlayerReadyButtonToggle()
    {
        playerReadyButton.onClick.AddListener(() =>
        {
            if (isPlayerReady == false)
            {
                foreach (Player player in PhotonNetwork.PlayerListOthers)
                {
                    if (player.CustomProperties.TryGetValue(Constants.PLAYER_SELECTION_INDEX, out object playerIndex))
                    {
                        if ((int)playerIndex == playerSelection.PlayerSelectionIndex)
                        {
                            return;
                        }
                    }
                }

                isPlayerReady = true;
                SetPlayerReadyImage(isPlayerReady);
                SetPlayerReadyImageProperties();
                playerSelection.SetSelectablePlayerProperties(playerSelection.PlayerSelectionIndex);
            }
            else
            {
                isPlayerReady = false;
                SetPlayerReadyImage(isPlayerReady);
                SetPlayerReadyImageProperties();
                playerSelection.SetSelectablePlayerProperties(-1);
            }
        });
    }
}
