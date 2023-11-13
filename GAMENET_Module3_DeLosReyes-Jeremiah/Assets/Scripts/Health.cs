using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class Health : MonoBehaviourPunCallbacks
{
    public static Action<int> OnPlayerEliminated;

    public Image HealthBar;

    public int CurrentHealth;
    public int MaxHealth;

    public bool IsDead = false;

    private void Start()
    {
        MaxHealth = 100;
        CurrentHealth = MaxHealth;
        UpdateHealthBar();
    }

    [PunRPC]
    public void ApplyDamage(int damage)
    {
        if (CurrentHealth > 0)
        {
            CurrentHealth -= damage;
            UpdateHealthBar();
        }

        if (CurrentHealth <= 0 && !IsDead && photonView.IsMine)
        {
            IsDead = true;

            GetComponent<PlayerVehicleMovement>().enabled = false;
            GetComponent<PlayerShooting>().enabled = false;

            ExitGames.Client.Photon.Hashtable playerCustomProps = new ExitGames.Client.Photon.Hashtable
            {
                { Constants.PLAYER_ELIMINATED, true }
            };

            photonView.Owner.SetCustomProperties(playerCustomProps);

            photonView.RPC("PlayerEliminated", RpcTarget.AllBuffered, photonView.Owner.ActorNumber);
        }
    }

    [PunRPC]
    public void PlayerEliminated(int actorNumber)
    {
        if (OnPlayerEliminated != null)
        {
            OnPlayerEliminated(actorNumber);
        }
    }

    private void UpdateHealthBar()
    {
        HealthBar.fillAmount = (float)CurrentHealth/(float)MaxHealth;
    }
}
