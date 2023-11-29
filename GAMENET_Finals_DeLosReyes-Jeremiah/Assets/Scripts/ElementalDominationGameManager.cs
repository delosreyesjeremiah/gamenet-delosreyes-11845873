using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class ElementalDominationGameManager : MonoBehaviourPunCallbacks
{
    public Transform[] SpawnPoints
    {
        get => spawnPoints;
    }

    public static ElementalDominationGameManager Instance
    {
        get => instance;
    }

    [Header("Player Configurations")]
    [SerializeField] private GameObject[] playerPrefabs;
    [SerializeField] private Transform[] spawnPoints;

    [Header("Elemental Zone Configurations")]
    [SerializeField] private GameObject[] zoneTiles;
    [SerializeField] private Material[] elementMaterials;
    [SerializeField] private MeshRenderer[] zoneRenderers;
    [SerializeField] private int[] zoneControls;

    [Header("Game UI")]
    [SerializeField] private TextMeshProUGUI[] zoneControlsTextUI;
    [SerializeField] private TextMeshProUGUI winnerTextUI;

    private static ElementalDominationGameManager instance = null;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        if (PhotonNetwork.IsConnectedAndReady)
        {
            SpawnPlayerAtRandomSpawnPoint();
            UpdateWinnerTextUI("");

            foreach (TextMeshProUGUI zoneControlsText in zoneControlsTextUI)
            {
                zoneControlsText.text = "0";
            }
        }
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.LoadLevel("LobbyScene");
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void ShowKillAnnouncement(string killerName, string victimName)
    {
        var photonViews = PhotonNetwork.PhotonViewCollection;
        foreach (var photonViewPair in photonViews)
        {
            PhotonView announcerView = PhotonView.Find(photonViewPair.ViewID);
            if (announcerView != null)
            {
                announcerView.RPC("AnnounceKill", RpcTarget.AllBuffered, killerName, victimName);
            }
        }
    }

    public void ConvertZone(int zoneIndex, int elementIndex)
    {
        if (zoneControls[elementIndex] < Constants.NUMBER_OF_ZONES)
        {
            UpdateZoneMaterial(zoneIndex, elementIndex);
            zoneControls[elementIndex]++;
            UpdateZoneControlsTextUIAtIndex(elementIndex, zoneControls[elementIndex].ToString());
        }
       
        if (zoneControls[elementIndex] >= Constants.NUMBER_OF_ZONES)
        {
            GameOver((ElementType)elementIndex);
        }
    }

    public void DeductZoneControl(int elementIndex)
    {
        zoneControls[(int)elementIndex]--;
        UpdateZoneControlsTextUIAtIndex(elementIndex, zoneControls[elementIndex].ToString());
    }

    private void GameOver(ElementType elementWinner)
    {
        Debug.Log("We have a winner!");
        EvaluateWinner(elementWinner);
    }

    private void UpdateZoneMaterial(int zoneIndex, int elementIndex)
    {
        if (zoneRenderers[zoneIndex] != null)
        {
            zoneRenderers[zoneIndex].material = elementMaterials[elementIndex];
        }
    }

    private void SpawnPlayerAtRandomSpawnPoint()
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(Constants.PLAYER_SELECTION_INDEX, out object playerSelectionIndex))
        {
            Debug.Log((int)playerSelectionIndex);

            int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
            Vector3 spawnPosition = spawnPoints[actorNumber - 1].position;

            PhotonNetwork.Instantiate(playerPrefabs[(int)playerSelectionIndex].name, spawnPosition, Quaternion.identity);
        }
    }

    private void EvaluateWinner(ElementType elementWinner)
    {
        switch (elementWinner)
        {
            case ElementType.Fire:
                DisplayWinner("Fire");
                UpdateWinnerTextUI("SCORCHING!");
                break;
            case ElementType.Water:
                DisplayWinner("Water");
                UpdateWinnerTextUI("REFRESHING!");
                break;
            case ElementType.Earth:
                DisplayWinner("Earth");
                UpdateWinnerTextUI("HARDCORE!");
                break;
            case ElementType.None:
                break;
        }
    }

    private void DisplayWinner(string winnerName)
    {
        var photonViews = PhotonNetwork.PhotonViewCollection;
        foreach (var photonViewPair in photonViews)
        {
            PhotonView announcerView = PhotonView.Find(photonViewPair.ViewID);
            if (announcerView != null)
            {
                announcerView.RPC("AnnounceWinner", RpcTarget.AllBuffered, winnerName);
            }
        }
    }

    private void UpdateWinnerTextUI(string text)
    {
        winnerTextUI.text = text;
    }

    private void UpdateZoneControlsTextUIAtIndex(int index, string text)
    {
        zoneControlsTextUI[index].text = text;
    }
}
