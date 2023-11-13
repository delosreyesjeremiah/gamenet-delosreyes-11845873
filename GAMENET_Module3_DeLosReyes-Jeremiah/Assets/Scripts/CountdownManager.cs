using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class CountdownManager : MonoBehaviourPunCallbacks
{
    public Text TimerText;
    public float TimeToStartRace = 5.0f;

    private void Start()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue("rc"))
        {
            TimerText = RacingGameManager.Instance.TimeText;
        }
        else if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue("dr"))
        {
            TimerText = DeathRaceGameManager.Instance.TimeText;
        }
    }

    private void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (TimerText != null)
            {
                if (TimeToStartRace > 0.0f)
                {
                    TimeToStartRace -= Time.deltaTime;
                    photonView.RPC("SetTime", RpcTarget.AllBuffered, TimeToStartRace);
                }
                else
                {
                    photonView.RPC("StartRace", RpcTarget.AllBuffered);
                }
            }
            
        }
    }

    [PunRPC]
    public void SetTime(float time)
    {
        if (TimerText != null)
        {
            if (time > 0.0f)
            {
                TimerText.text = time.ToString("F1");
            }
            else
            {
                TimerText.text = "";
            }
        }
    }

    [PunRPC]
    public void StartRace()
    {
        GetComponent<PlayerVehicleMovement>().IsControlEnabled = true;
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue("dr"))
        {
            GetComponent<PlayerShooting>().IsControlEnabled = true;
        }

        this.enabled = false;
    }
}
