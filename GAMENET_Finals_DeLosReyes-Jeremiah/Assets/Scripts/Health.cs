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
        get => isDead;
        set => isDead = value;
    }

    public Action OnPlayerDeath;

    [SerializeField] private Image healthBar;

    [SerializeField] private float currentHealth;
    [SerializeField] private float maxHealth = 100.0f;
    [SerializeField] private bool isDead = false;

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBarUI();
    }

    [PunRPC]
    public void ApplyDamage(int attackerViewID, PhotonMessageInfo info)
    {
        if (currentHealth > 0.0f)
        {
            float damage = 0.0f;

            PhotonView attackerView = PhotonView.Find(attackerViewID);
            PlayerSetup attackerPlayerSetup = attackerView.GetComponent<PlayerSetup>();

            PlayerSetup victimPlayerSetup = GetComponent<PlayerSetup>();

            switch (attackerPlayerSetup.ElementType)
            {
                case ElementType.Fire:
                    switch (victimPlayerSetup.ElementType)
                    {
                        case ElementType.Fire:
                            damage = attackerPlayerSetup.Damage - victimPlayerSetup.Defense;
                            break;
                        case ElementType.Water:
                            damage = attackerPlayerSetup.Damage * Constants.WEAKNESS_MULTIPLIER - victimPlayerSetup.Defense;
                            break;
                        case ElementType.Earth:
                            damage = attackerPlayerSetup.Damage * Constants.STRENGTH_MULTIPLIER - victimPlayerSetup.Defense;
                            break;
                    }
                    break;
                case ElementType.Water:
                    switch (victimPlayerSetup.ElementType)
                    {
                        case ElementType.Fire:
                            damage = attackerPlayerSetup.Damage * Constants.STRENGTH_MULTIPLIER - victimPlayerSetup.Defense;
                            break;
                        case ElementType.Water:
                            damage = attackerPlayerSetup.Damage - victimPlayerSetup.Defense;
                            break;
                        case ElementType.Earth:
                            damage = attackerPlayerSetup.Damage * Constants.WEAKNESS_MULTIPLIER - victimPlayerSetup.Defense;
                            break;
                    }
                    break;
                case ElementType.Earth:
                    switch (victimPlayerSetup.ElementType)
                    {
                        case ElementType.Fire:
                            damage = attackerPlayerSetup.Damage * Constants.WEAKNESS_MULTIPLIER - victimPlayerSetup.Defense;
                            break;
                        case ElementType.Water:
                            damage = attackerPlayerSetup.Damage * Constants.STRENGTH_MULTIPLIER - victimPlayerSetup.Defense;
                            break;
                        case ElementType.Earth:
                            damage = attackerPlayerSetup.Damage - victimPlayerSetup.Defense;
                            break;
                    }
                    break;
            }

            currentHealth -= damage;
            UpdateHealthBarUI();
        }

        if (currentHealth <= 0 && !isDead && photonView.IsMine)
        {
            Die();
            PhotonView attackerView = PhotonView.Find(attackerViewID);
            attackerView.RPC("IncreaseKillCount", RpcTarget.AllBuffered);

            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue(Constants.GAME_MODE_FREE_FOR_ALL))
            {
                FreeForAllDeathmatchGameManager.Instance.ShowKillAnnouncement(info.Sender.NickName, photonView.Owner.NickName);
            }
            else if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue(Constants.GAME_MODE_ELEMENTAL_DOMINATION))
            {
                ElementalDominationGameManager.Instance.ShowKillAnnouncement(info.Sender.NickName, photonView.Owner.NickName);
            }
        }
    }

    [PunRPC]
    public void RegainHealth()
    {
        currentHealth = maxHealth;
        UpdateHealthBarUI();
    }

    private void UpdateHealthBarUI()
    {
        healthBar.fillAmount = currentHealth / maxHealth;
    }

    private void Die()
    {
        isDead = true;
        OnPlayerDeath?.Invoke();
    }
}
