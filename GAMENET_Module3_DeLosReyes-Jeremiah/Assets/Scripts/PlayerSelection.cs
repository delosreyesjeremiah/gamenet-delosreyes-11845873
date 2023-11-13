using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSelection : MonoBehaviour
{
    #region Fields

    public GameObject[] SelectablePlayers;
    public int PlayerSelectionNumber;

    #endregion

    #region Unity Methods

    private void Start()
    {
        PlayerSelectionNumber = 0;
        ActivatePlayer(PlayerSelectionNumber);
    }

    #endregion

    #region Public Methods

    public void GoToNextPlayer()
    {
        PlayerSelectionNumber++;

        if (PlayerSelectionNumber >= SelectablePlayers.Length)
        {
            PlayerSelectionNumber = 0;
        }

        ActivatePlayer(PlayerSelectionNumber);
    }

    public void GoToPreviousPlayer()
    {
        PlayerSelectionNumber--;

        if (PlayerSelectionNumber < 0)
        {
            PlayerSelectionNumber = SelectablePlayers.Length - 1;
        }

        ActivatePlayer(PlayerSelectionNumber);
    }

    #endregion

    #region Private Methods

    private void ActivatePlayer(int index)
    {
        foreach (GameObject selectablePlayer in SelectablePlayers)
        {
            selectablePlayer.SetActive(false);
        }

        SelectablePlayers[index].SetActive(true);

        // Setting the player selection for the vehicle
        ExitGames.Client.Photon.Hashtable playerSelectionProperties = new ExitGames.Client.Photon.Hashtable() { { Constants.PLAYER_SELECTION_NUMBER, PlayerSelectionNumber } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerSelectionProperties);
    }

    #endregion
}
