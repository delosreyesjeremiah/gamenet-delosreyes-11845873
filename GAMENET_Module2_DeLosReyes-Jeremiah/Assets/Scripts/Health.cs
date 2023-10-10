using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class Health : MonoBehaviourPunCallbacks
{
    public bool IsDead
    {
        get => _isDead;
        set => _isDead = value; 
    }

    public Action OnPlayerDeath;

    [SerializeField] private Image _healthBar;

    private Animator _animator;

    private int _currentHealth;
    private int _maxHealth = 100;

    private bool _isDead = false;

    private void Start()
    {
        _animator = GetComponent<Animator>();

        _currentHealth = _maxHealth;
        UpdateHealthBar();
    }

    [PunRPC]
    public void ApplyDamage(int damage, int attackerViewID, PhotonMessageInfo info)
    {
        if (_currentHealth > 0)
        {
            _currentHealth -= damage;
            UpdateHealthBar();
        }
        
        if (_currentHealth <= 0 && !_isDead && photonView.IsMine)
        {
            Die();
            PhotonView attackerView = PhotonView.Find(attackerViewID);
            attackerView.RPC("IncreaseKillCount", RpcTarget.AllBuffered);
            GameManager.Instance.ShowKillAnnouncement(info.Sender.NickName, photonView.Owner.NickName);
        }
    }

    public void Die()
    {
        _isDead = true;
        _animator.SetBool("isDead", true);
        OnPlayerDeath?.Invoke();
    }

    [PunRPC]
    public void RegainHealth()
    {
        _currentHealth = _maxHealth;
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        _healthBar.fillAmount = (float)_currentHealth / (float)_maxHealth;
    }
}
