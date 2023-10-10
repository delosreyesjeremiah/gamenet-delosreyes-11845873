using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnerManager : MonoBehaviour
{
    public static SpawnerManager Instance
    {
        get => _instance;
    }

    public Transform[] SpawnPoints
    {
        get => _spawnPoints;
    }

    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private Transform[] _spawnPoints;

    private static SpawnerManager _instance;

    private void Awake()
    {
        _instance = this;
    }

    private void OnDestroy()
    {
        _instance = null;
    }

    public void SpawnPlayer()
    {
        int randomIndex = Random.Range(0, _spawnPoints.Length);
        Vector3 spawnLocation = _spawnPoints[randomIndex].position;
        PhotonNetwork.Instantiate(_playerPrefab.name, spawnLocation, Quaternion.identity);
    }
}
