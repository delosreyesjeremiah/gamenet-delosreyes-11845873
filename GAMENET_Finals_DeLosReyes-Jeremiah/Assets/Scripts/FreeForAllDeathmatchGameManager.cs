using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class FreeForAllDeathmatchGameManager : MonoBehaviourPunCallbacks
{
    public Transform[] SpawnPoints
    {
        get => spawnPoints;
    }

    public static FreeForAllDeathmatchGameManager Instance
    {
        get => instance;
    }

    private static FreeForAllDeathmatchGameManager instance = null;

    [SerializeField] private GameObject[] playerPrefabs;
    [SerializeField] private Transform[] spawnPoints;

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

    public void DisplayWinner(string winnerName)
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
}
