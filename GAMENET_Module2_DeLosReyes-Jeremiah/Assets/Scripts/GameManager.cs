using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance
    {
        get => _instance;
    }

    private static GameManager _instance;

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            SpawnerManager.Instance.SpawnPlayer();
        }  
    }

    private void OnDestroy()
    {
        _instance = null;
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("LobbyScene");
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
}
