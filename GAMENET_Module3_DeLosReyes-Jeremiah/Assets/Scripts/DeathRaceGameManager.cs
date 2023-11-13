using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class DeathRaceGameManager : MonoBehaviour
{
    public GameObject[] VehiclePrefabs;
    public Transform[] StartingPositions;
    public Text TimeText;
    public Text WinnerText;
    public Text EliminationText;
    public int RemainingPlayers;

    public static DeathRaceGameManager Instance = null;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        Health.OnPlayerEliminated += DisplayPlayerElimination;
    }

    private void OnDisable()
    {
        Health.OnPlayerEliminated -= DisplayPlayerElimination;
    }

    private void Start()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            SpawnPlayerVehicle();
            RemainingPlayers = PhotonNetwork.PlayerList.Length;
        }
    }

    private void SpawnPlayerVehicle()
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(Constants.PLAYER_SELECTION_NUMBER, out object playerSelectionNumber))
        {
            Debug.Log((int)playerSelectionNumber);

            int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;

            Vector3 spawnPosition = StartingPositions[actorNumber - 1].position;
            Quaternion spawnRotation = StartingPositions[actorNumber - 1].rotation;

            PhotonNetwork.Instantiate(VehiclePrefabs[(int)playerSelectionNumber].name, spawnPosition, spawnRotation);
        }
    }

    private void DisplayPlayerElimination(int actorNumber)
    {
        RemainingPlayers--;

        if (RemainingPlayers <= 1)
        {
            int winnerActorNumber = GetWinnerActorNumber();
            string winnerName = GetPlayerNameByActorNumber(winnerActorNumber);

            WinnerText.text = $"{winnerName} is the winner!";
            DisableWinnerControls(winnerActorNumber);
        }

        string eliminatedPlayerName = GetPlayerNameByActorNumber(actorNumber);
        EliminationText.text = $"{eliminatedPlayerName} has been eliminated.";

        StartCoroutine(ResetEliminationText());
    }

    private int GetWinnerActorNumber()
    {
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (!player.CustomProperties.ContainsKey(Constants.PLAYER_ELIMINATED))
            {
                return player.ActorNumber;
            }
        }

        return 0;
    }

    private string GetPlayerNameByActorNumber(int actorNumber)
    {
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player.ActorNumber == actorNumber)
            {
                return player.NickName;
            } 
        }

        return "";
    }

    private void DisableWinnerControls(int winnerActorNumber)
    {
        foreach (var vehicle in GameObject.FindGameObjectsWithTag("Player"))  
        {
            PhotonView photonView = vehicle.GetComponent<PhotonView>();
            if (photonView != null && photonView.Owner.ActorNumber == winnerActorNumber)
            {
                vehicle.GetComponent<PlayerVehicleMovement>().enabled = false;
                vehicle.GetComponent<PlayerShooting>().enabled = false;
            }
        }
    }

    private IEnumerator ResetEliminationText()
    {
        yield return new WaitForSeconds(2.0f);
        EliminationText.text = "";
    }
}
