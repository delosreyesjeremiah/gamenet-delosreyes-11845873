using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class LaunchManager : MonoBehaviourPunCallbacks
{
    #region Fields

    [SerializeField] private GameObject _enterGamePanel;
    [SerializeField] private GameObject _connectionStatusPanel;
    [SerializeField] private GameObject _lobbyPanel;

    #endregion

    #region Unity Messages

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start()
    {
        _enterGamePanel.SetActive(true);
        _connectionStatusPanel.SetActive(false);
        _lobbyPanel.SetActive(false);
    }

    #endregion

    #region Photon Server Methods

    public override void OnConnected()
    {
        Debug.Log("Connected to the Internet.");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log($"{PhotonNetwork.NickName} is connected to Photon Servers.");
        _connectionStatusPanel.SetActive(false);
        _lobbyPanel.SetActive(true);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogWarning(message);
        CreateAndJoinRoom();
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"{PhotonNetwork.NickName} has entered {PhotonNetwork.CurrentRoom.Name}.");
        PhotonNetwork.LoadLevel("GameScene");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"{newPlayer.NickName} has entered {PhotonNetwork.CurrentRoom.Name}. The room now has {PhotonNetwork.CurrentRoom.PlayerCount} players.");
    }

    #endregion

    #region Public Methods

    public void ConnectToPhotonServer()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
            _connectionStatusPanel.SetActive(true);
            _enterGamePanel.SetActive(false);
        }
    }

    public void JoinRandomRoom()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    #endregion

    #region Private Methods

    private void CreateAndJoinRoom()
    {
        string randomRoomName = $"Room {Random.Range(0, 10000)}";

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsOpen = true;
        roomOptions.IsVisible = true;
        roomOptions.MaxPlayers = 20;

        PhotonNetwork.CreateRoom(randomRoomName, roomOptions);
    }

    #endregion
}
