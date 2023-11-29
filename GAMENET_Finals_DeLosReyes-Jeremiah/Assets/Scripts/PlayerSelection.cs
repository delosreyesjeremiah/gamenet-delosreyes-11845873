using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class PlayerSelection : MonoBehaviour
{
    public int PlayerSelectionIndex
    {
        get => playerSelectionIndex;
    }

    [SerializeField] private GameObject[] selectablePlayers;
    [SerializeField] private int playerSelectionIndex;

    private void Start()
    {
        playerSelectionIndex = 0;
        ActivateSelectablePlayer(playerSelectionIndex);
    }

    public void GoToNextSelectablePlayer()
    {
        playerSelectionIndex++;

        if (playerSelectionIndex >= selectablePlayers.Length)
        {
            playerSelectionIndex = 0;
        }

        SetSelectablePlayerProperties(-1);
        ActivateSelectablePlayer(playerSelectionIndex);
    }

    public void GoToPreviousSelectablePlayer()
    {
        playerSelectionIndex--;

        if (playerSelectionIndex < 0)
        {
            playerSelectionIndex = selectablePlayers.Length - 1;
        }

        SetSelectablePlayerProperties(-1);
        ActivateSelectablePlayer(playerSelectionIndex);
    }

    public void SetSelectablePlayerProperties(int index)
    {
        ExitGames.Client.Photon.Hashtable playerSelectionProperties = new ExitGames.Client.Photon.Hashtable() { { Constants.PLAYER_SELECTION_INDEX, index } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerSelectionProperties);
    }

    public void ActivateSelectablePlayer(int index)
    {
        foreach (GameObject selectablePlayer in selectablePlayers)
        {
            selectablePlayer.SetActive(false);
        }

        selectablePlayers[index].SetActive(true);
        Text[] texts = selectablePlayers[index].GetComponentsInChildren<Text>();
        foreach (Text text in texts)
        {
            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(Constants.PLAYER_READY, out object isPlayerReady))
            {
                if ((bool)isPlayerReady)
                {
                    text.color = Color.green;
                }
                else
                {
                    text.color = Color.black;
                }
            }
        }

        foreach (Player player in PhotonNetwork.PlayerListOthers)
        {
            if (player.CustomProperties.TryGetValue(Constants.PLAYER_SELECTION_INDEX, out object playerIndex))
            {
                if ((int)playerIndex == index)
                {
                    foreach (Text text in texts)
                    {
                        text.color = Color.gray;
                    }

                    return;
                }
            }
        }
    }
}
