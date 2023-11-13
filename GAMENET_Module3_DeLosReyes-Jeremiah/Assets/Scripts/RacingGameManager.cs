using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class RacingGameManager : MonoBehaviour
{
    public GameObject[] FinisherTextsUI;
    public GameObject[] VehiclePrefabs;
    public Transform[] StartingPositions;
    public Text TimeText;
    public List<GameObject> LapTriggers;

    public static RacingGameManager Instance = null;

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

    private void Start()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            SpawnPlayerVehicle();
        }

        foreach (GameObject finisherTextUI in FinisherTextsUI)
        {
            finisherTextUI.SetActive(false);
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
}
