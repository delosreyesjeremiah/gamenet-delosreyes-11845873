using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class PlayerRespawn : MonoBehaviourPunCallbacks
{
    [Header("Health Component")]
    [SerializeField] private Health health;

    [Header("Respawn UI")]
    [SerializeField] private GameObject respawnPanel;
    [SerializeField] private TextMeshProUGUI respawnText;

    private PlayerMovementController playerMovementController;

    private void OnEnable()
    {
        health.OnPlayerDeath += RespawnCountdown;
    }

    private void OnDisable()
    {
        health.OnPlayerDeath -= RespawnCountdown;
    }

    private void Awake()
    {
        playerMovementController = GetComponent<PlayerMovementController>();
    }

    private IEnumerator CO_RespawnCountdown()
    {
        respawnPanel.SetActive(true);
        float respawnTime = 5.0f;

        while (respawnTime > 0.0f)
        {
            yield return new WaitForSeconds(1.0f);
            respawnTime--;

            playerMovementController.enabled = false;
            respawnText.text = $"You are killed. Respawning in {respawnTime:.00}.";
        }

        respawnPanel.SetActive(false);
        Respawn();
    }

    private void Respawn()
    {
        Transform[] spawnPoints = null;
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue(Constants.GAME_MODE_FREE_FOR_ALL))
        {
            spawnPoints = FreeForAllDeathmatchGameManager.Instance.SpawnPoints;
        }
        else if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue(Constants.GAME_MODE_ELEMENTAL_DOMINATION))
        {
            spawnPoints = ElementalDominationGameManager.Instance.SpawnPoints;
        }
        
        int randomIndex = Random.Range(0, spawnPoints.Length);
        Vector3 spawnLocation = spawnPoints[randomIndex].position;
        transform.position = spawnLocation;

        playerMovementController.enabled = true;
        health.IsDead = false;
        respawnText.text = "";

        photonView.RPC("RegainHealth", RpcTarget.AllBuffered);
    }

    private void RespawnCountdown()
    {
        StartCoroutine(CO_RespawnCountdown());
    }
}
