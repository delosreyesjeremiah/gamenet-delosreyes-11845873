using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    #region Fields

    [Header("UI Panels")]
    [SerializeField] private List<GameObject> _panels;

    [Header("Connection Status Panel")]
    [SerializeField] private Text _connectionStatusText;

    [Header("Login UI Panel")]
    [SerializeField] private InputField _playerNameInputField;

    [Header("Create Room Panel")]
    [SerializeField] private InputField _roomNameInputField;
    [SerializeField] private InputField _playerCountInputField;

    [Header("Inside Room Pane")]
    [SerializeField] private Text _roomInfoText;
    [SerializeField] private GameObject _playerListPrefab;
    [SerializeField] private GameObject _playerListParent;
    [SerializeField] private GameObject _startGameButton;

    [Header("Room List Panel")]
    [SerializeField] private GameObject _roomListPrefab;
    [SerializeField] private GameObject _roomListParent;

    private Dictionary<string, RoomInfo> _roomList;
    private Dictionary<string, GameObject> _roomListItems;
    private Dictionary<int, GameObject> _playerListItems;

    #endregion

    #region Unity Messages

    private void Start()
    {
        _roomList = new Dictionary<string, RoomInfo>();
        _roomListItems = new Dictionary<string, GameObject>();

        UpdateConnectionStatusText();
        ActivatePanel(_panels[0]);

        PhotonNetwork.AutomaticallySyncScene = true;
    }

    #endregion

    #region UI Callbacks

    public void Login()
    {
        string playerName = _playerNameInputField.text;
        if (string.IsNullOrEmpty(playerName))
        {
            Debug.LogWarning("Player name is invalid");
        }
        else
        {
            PhotonNetwork.LocalPlayer.NickName = playerName;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public void JoinRandomRoom(GameObject randomRoomPanel)
    {
        ActivatePanel(randomRoomPanel);
        PhotonNetwork.JoinRandomRoom();
    }

    public void EnterCreateRoomPanel(GameObject createRoomPanel)
    {
        ActivatePanel(createRoomPanel);
    }

    public void CreateRoom()
    {
        string roomName = _roomNameInputField.text;
        if (string.IsNullOrEmpty(roomName))
        {
            roomName = $"Room {UnityEngine.Random.Range(1000, 10000)}";
        }

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = (byte)int.Parse(_playerCountInputField.text);
        roomOptions.MaxPlayers = Mathf.Clamp(roomOptions.MaxPlayers, 1, 20);

        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    public void LeaveCreateRoomPanel(GameObject gameOptionsPanel)
    {
        ActivatePanel(gameOptionsPanel);
    }

    public void EnterLobby(GameObject roomListPanel)
    {
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }

        ActivatePanel(roomListPanel);
    }

    public void LeaveLobby(GameObject gameOptionsPanel)
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }

        ActivatePanel(gameOptionsPanel);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void LoadGameScene()
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.LoadLevel("GameScene");
    }

    #endregion

    #region PUN Callbacks

    public override void OnConnected()
    {
        Debug.Log("Connected to the internet.");
        UpdateConnectionStatusText();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log($"{PhotonNetwork.LocalPlayer.NickName} has connected to Photon servers.");
        UpdateConnectionStatusText();
        ActivatePanel(_panels[1]);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning($"Disconnected from Photon servers: {cause}");
        UpdateConnectionStatusText();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogWarning(message);
        CreateRandomRoom();
    }

    public override void OnCreatedRoom()
    {
        Debug.Log($"{PhotonNetwork.CurrentRoom.Name} created.");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"{PhotonNetwork.LocalPlayer.NickName} has joined {PhotonNetwork.CurrentRoom.Name}.");
        ActivatePanel(_panels[5]);

        _startGameButton.SetActive(PhotonNetwork.LocalPlayer.IsMasterClient);

        _roomInfoText.text = $"Room Name: {PhotonNetwork.CurrentRoom.Name} Current Player Count: {PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}";

        if (_playerListItems == null)
        {
            _playerListItems = new Dictionary<int, GameObject>();
        }

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            CreateListItem(_playerListPrefab, _playerListParent.transform, SetupPlayerListItem, player);
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        ClearListItems(_roomListItems);

        foreach (RoomInfo roomInfo in roomList)
        {
            Debug.Log(roomInfo.Name);

            if (!roomInfo.IsOpen || !roomInfo.IsVisible || roomInfo.RemovedFromList)
            {
                if (_roomList.ContainsKey(roomInfo.Name))
                {
                    _roomList.Remove(roomInfo.Name);
                }
            }
            else
            {
                if (_roomList.ContainsKey(roomInfo.Name))
                {
                    _roomList[roomInfo.Name] = roomInfo;
                }
                else
                {
                    _roomList.Add(roomInfo.Name, roomInfo);
                }   
            }
        }

        foreach (RoomInfo roomInfo in _roomList.Values)
        {
            CreateListItem(_roomListPrefab, _roomListParent.transform, SetupRoomListItem, roomInfo);
        }
    }

    public override void OnLeftLobby()
    {
        ClearListItems(_roomListItems);
        _roomList.Clear();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        _roomInfoText.text = $"Room Name: {PhotonNetwork.CurrentRoom.Name} Current Player Count: {PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}";
        CreateListItem(_playerListPrefab, _playerListParent.transform, SetupPlayerListItem, newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        _startGameButton.SetActive(PhotonNetwork.LocalPlayer.IsMasterClient);

        _roomInfoText.text = $"Room Name: {PhotonNetwork.CurrentRoom.Name} Current Player Count: {PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}";

        Destroy(_playerListItems[otherPlayer.ActorNumber]);
        _playerListItems.Remove(otherPlayer.ActorNumber);
    }

    public override void OnLeftRoom()
    {
        ClearListItems(_playerListItems);
        _playerListItems = null;

        if (_panels[1] != null)
        {
            ActivatePanel(_panels[1]);
        }  
    }

    #endregion

    #region Private Methods

    private void UpdateConnectionStatusText()
    {
        if (_connectionStatusText != null)
        {
            _connectionStatusText.text = $"Connection status: {PhotonNetwork.NetworkClientState}";
        }    
    }

    private void ActivatePanel(GameObject panelToBeActivated)
    {
        foreach (GameObject panel in _panels)
        {
            panel.SetActive(panelToBeActivated.Equals(panel));
        }
    }

    private void CreateRandomRoom()
    {
        string roomName = $"Room: {UnityEngine.Random.Range(1000, 10000)}";
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 20;
        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    private void JoinRoom(string roomName)
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }

        PhotonNetwork.JoinRoom(roomName);
    }

    private void CreateListItem(GameObject prefab, Transform parent, Action<Transform, object> setupAction, object item)
    {
        GameObject listItem = Instantiate(prefab);
        listItem.transform.SetParent(parent);
        listItem.transform.localScale = Vector3.one;

        setupAction(listItem.transform, item);
    }

    private void SetupRoomListItem(Transform listItemTransform, object item)
    {
        RoomInfo roomInfo = (RoomInfo)item;
        listItemTransform.Find("RoomNameText").GetComponent<Text>().text = roomInfo.Name;
        listItemTransform.Find("RoomPlayersText").GetComponent<Text>().text = $"Player count: {roomInfo.PlayerCount}/{roomInfo.MaxPlayers}";
        listItemTransform.Find("JoinRoomButton").GetComponent<Button>().onClick.AddListener(() => JoinRoom(roomInfo.Name));

        _roomListItems.Add(roomInfo.Name, listItemTransform.gameObject);
    }

    private void SetupPlayerListItem(Transform listItemTransform, object item)
    {
        Player player = (Player)item;
        listItemTransform.Find("PlayerNameText").GetComponent<Text>().text = player.NickName;
        listItemTransform.Find("PlayerIndicator").gameObject.SetActive(player.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber);

        _playerListItems.Add(player.ActorNumber, listItemTransform.gameObject);
    }

    private void ClearListItems<T>(Dictionary<T, GameObject> listItems)
    {
        foreach (var listItem in listItems.Values)
        {
            Destroy(listItem);
        }

        listItems.Clear();
    }

    #endregion
}
