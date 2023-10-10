using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerRespawn : MonoBehaviourPunCallbacks
{
    [Header("Health Component")]
    [SerializeField] private Health _health;

    [Header("Respawn UI")]
    [SerializeField] private Text _respawnText;
    [SerializeField] private GameObject _respawnPanel;

    private PlayerMovementController _playerMovementController;
    private Animator _animator;

    private new void OnEnable()
    {
        _health.OnPlayerDeath += RespawnCountdown;
    }

    private new void OnDisable()
    {
        _health.OnPlayerDeath -= RespawnCountdown;
    }

    private void Start()
    {
        _playerMovementController = GetComponent<PlayerMovementController>();
        _animator = GetComponent<Animator>();
    }

    private IEnumerator CO_RespawnCountdown()
    {
        _respawnPanel.SetActive(true);
        float respawnTime = 5.0f;

        while (respawnTime > 0.0f)
        {
            yield return new WaitForSeconds(1.0f);
            respawnTime--;

            _playerMovementController.enabled = false;
            _respawnText.text = $"You are killed. Respawning in {respawnTime:.00}.";
        }

        _respawnPanel.SetActive(false);
        Respawn();
    }

    private void Respawn()
    {
        Transform[] spawnPoints = SpawnerManager.Instance.SpawnPoints;
        int randomIndex = Random.Range(0, spawnPoints.Length);
        Vector3 spawnLocation = spawnPoints[randomIndex].position;
        transform.position = spawnLocation;

        _playerMovementController.enabled = true;
        _animator.SetBool("isDead", false);
        _health.IsDead = false;
        _respawnText.text = "";

        photonView.RPC("RegainHealth", RpcTarget.AllBuffered);
    }

    private void RespawnCountdown()
    {
        StartCoroutine(CO_RespawnCountdown());
    }
}
