using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class LapController : MonoBehaviourPunCallbacks
{
    public enum RaiseEventsCode
    {
        WhoFinishedEventCode = 0
    }

    public List<GameObject> LapTriggers = new List<GameObject>();

    private int FinishOrder = 0;

    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    private void Start()
    {
        foreach (GameObject lapTrigger in RacingGameManager.Instance.LapTriggers)
        {
            LapTriggers.Add(lapTrigger);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (LapTriggers.Contains(other.gameObject))
        {
            int indexOfTrigger = LapTriggers.IndexOf(other.gameObject);

            LapTriggers[indexOfTrigger].SetActive(false);
        }

        if (other.gameObject.tag == "FinishTrigger")
        {
            GameFinish();
        }
    }

    public void GameFinish()
    {
        GetComponent<PlayerSetup>().Camera.transform.parent = null;
        GetComponent<PlayerVehicleMovement>().enabled = false;

        FinishOrder++;

        string nickName = photonView.Owner.NickName;
        int viewID = photonView.ViewID;

        object[] data = new object[] { nickName, FinishOrder, viewID };

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions
        {
            Receivers = ReceiverGroup.All,
            CachingOption = EventCaching.AddToRoomCache
        };

        SendOptions sendOptions = new SendOptions
        {
            Reliability = false
        };

        PhotonNetwork.RaiseEvent((byte)RaiseEventsCode.WhoFinishedEventCode, data, raiseEventOptions, sendOptions);
    }

    private void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == (byte)RaiseEventsCode.WhoFinishedEventCode)
        {
            object[] data = (object[])photonEvent.CustomData;

            string nickNameOfFinishedPlayer = (string)data[0];
            FinishOrder = (int)data[1];
            int viewID = (int)data[2];

            Debug.Log($"{nickNameOfFinishedPlayer} {FinishOrder}");

            GameObject finishOrderUIText = RacingGameManager.Instance.FinisherTextsUI[FinishOrder - 1];
            finishOrderUIText.SetActive(true);

            if (viewID == photonView.ViewID)
            {
                finishOrderUIText.GetComponent<Text>().text = $"{FinishOrder} {nickNameOfFinishedPlayer} (YOU)";
                finishOrderUIText.GetComponent<Text>().color = Color.red;
            }
            else
            {
                finishOrderUIText.GetComponent<Text>().text = $"{FinishOrder} {nickNameOfFinishedPlayer}";
            } 
        }
    }
}
