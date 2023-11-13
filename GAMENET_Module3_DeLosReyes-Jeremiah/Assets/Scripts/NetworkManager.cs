using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    #region Fields

    [Header("Login UI")]
    public GameObject LoginUIPanel;
    public InputField PlayerNameInput;

    [Header("Connecting Info Panel")]
    public GameObject ConnectingInfoUIPanel;

    [Header("Creating Room Info Panel")]
    public GameObject CreatingRoomInfoUIPanel;

    [Header("GameOptions  Panel")]
    public GameObject GameOptionsUIPanel;

    [Header("Create Room Panel")]
    public GameObject CreateRoomUIPanel;
    public InputField RoomNameInputField;
    public string GameMode;

    [Header("Inside Room Panel")]
    public GameObject InsideRoomUIPanel;
    public GameObject PlayerListPrefab;
    public GameObject PlayerListParent;
    public GameObject StartGameButton;
    public Text RoomInfoText;
    public Text GameModeText;
   
    [Header("Join Random Room Panel")]
    public GameObject JoinRandomRoomUIPanel;

    private Dictionary<int, GameObject> playerListGameObjects;

    #endregion

    #region Unity Methods

    void Start()
    {
        ActivatePanel(LoginUIPanel.name);
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    #endregion

    #region UI Callback Methods

    public void OnLoginButtonClicked()
    {
        string playerName = PlayerNameInput.text;

        if (!string.IsNullOrEmpty(playerName))
        {
            ActivatePanel(ConnectingInfoUIPanel.name);

            if (!PhotonNetwork.IsConnected)
            {
                PhotonNetwork.LocalPlayer.NickName = playerName;
                PhotonNetwork.ConnectUsingSettings();
            }
        }
        else
        {
            Debug.Log("PlayerName is invalid!");
        }
    }

    public void OnCancelButtonClicked()
    {
        ActivatePanel(GameOptionsUIPanel.name);
    }
    
    public void OnCreateRoomButtonClicked()
    {
        ActivatePanel(CreatingRoomInfoUIPanel.name);

        if (GameMode != null)
        {
            CreateRoomWithGameModeProperties();
        }
    }

    public void OnJoinRandomRoomClicked(string gameMode)
    {
        GameMode = gameMode;

        ExitGames.Client.Photon.Hashtable expectedCustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "gm", gameMode } };
        PhotonNetwork.JoinRandomRoom(expectedCustomRoomProperties, 0);
    }

    public void OnBackButtonClicked()
    {
        ActivatePanel(GameOptionsUIPanel.name);
    }

    public void OnLeaveGameButtonClicked()
    {
        PhotonNetwork.LeaveRoom();
    }
    
    public void OnStartGameButtonClicked()
    {
        LoadGameModeScene();
    }
    
    #endregion

    #region Photon Callbacks

    public override void OnConnected()
    {
        Debug.Log("Connected to Internet");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log(PhotonNetwork.LocalPlayer.NickName+ " is connected to Photon");
        ActivatePanel(GameOptionsUIPanel.name);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log($"{PhotonNetwork.CurrentRoom} has been created.");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"{PhotonNetwork.LocalPlayer.NickName} has joined {PhotonNetwork.CurrentRoom.Name}.");
        Debug.Log($"Player count: {PhotonNetwork.CurrentRoom.PlayerCount}");

        ActivatePanel(InsideRoomUIPanel.name);

        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("gm", out object gameModeName))
        {
            Debug.Log(gameModeName.ToString());
            UpdateRoomInfoText();
            SetGameModeTextToChosenGameMode();
        }

        if (playerListGameObjects == null)
        {
            playerListGameObjects = new Dictionary<int, GameObject>();
        }

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            CreatePlayerListItem(player);
        }

        StartGameButton.SetActive(CheckIfAllPlayersAreReady());
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log(message);

        if (GameMode != null)
        {
            CreateRoomWithGameModeProperties();
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        CreatePlayerListItem(newPlayer);
        UpdateRoomInfoText();

        StartGameButton.SetActive(CheckIfAllPlayersAreReady());
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        DestroyAndRemovePlayerListItem(otherPlayer);
        UpdateRoomInfoText();
    }

    public override void OnLeftRoom()
    {
        ActivatePanel(GameOptionsUIPanel.name);
        DestroyAndClearPlayerListGameObjects();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (playerListGameObjects.TryGetValue(targetPlayer.ActorNumber, out GameObject playerListGameObject))
        {
            if (changedProps.TryGetValue(Constants.PLAYER_READY, out object isPlayerReady))
            {
                playerListGameObject.GetComponent<PlayerListItemInitializer>().SetPlayerReady((bool)isPlayerReady);
            }
        }

        StartGameButton.SetActive(CheckIfAllPlayersAreReady());
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == newMasterClient.ActorNumber)
        {
            StartGameButton.SetActive(CheckIfAllPlayersAreReady());
        }
    }

    #endregion

    #region Public Methods

    public void ActivatePanel(string panelNameToBeActivated)
    {
        LoginUIPanel.SetActive(LoginUIPanel.name.Equals(panelNameToBeActivated));
        ConnectingInfoUIPanel.SetActive(ConnectingInfoUIPanel.name.Equals(panelNameToBeActivated));
        CreatingRoomInfoUIPanel.SetActive(CreatingRoomInfoUIPanel.name.Equals(panelNameToBeActivated));
        CreateRoomUIPanel.SetActive(CreateRoomUIPanel.name.Equals(panelNameToBeActivated));
        GameOptionsUIPanel.SetActive(GameOptionsUIPanel.name.Equals(panelNameToBeActivated));
        JoinRandomRoomUIPanel.SetActive(JoinRandomRoomUIPanel.name.Equals(panelNameToBeActivated));
        InsideRoomUIPanel.SetActive(InsideRoomUIPanel.name.Equals(panelNameToBeActivated));
    }

    public void SetGameMode(string gameMode)
    {
        GameMode = gameMode;
    }

    #endregion

    #region Private Methods

    private void CreateRoomWithGameModeProperties()
    {
        string roomName = RoomNameInputField.text;

        if (string.IsNullOrEmpty(roomName))
        {
            roomName = "Room " + Random.Range(1000, 10000);
        }

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 3;

        string[] roomPropertiesInLobby = { "gm" };

        ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "gm", GameMode } };

        roomOptions.CustomRoomPropertiesForLobby = roomPropertiesInLobby;
        roomOptions.CustomRoomProperties = customRoomProperties;

        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    private void CreatePlayerListItem(Player player)
    {
        GameObject playerListItem = Instantiate(PlayerListPrefab);
        playerListItem.transform.SetParent(PlayerListParent.transform);
        playerListItem.transform.localScale = Vector3.one;

        playerListItem.GetComponent<PlayerListItemInitializer>().Initialize(player.ActorNumber, player.NickName);

        object isPlayerReady;
        if (player.CustomProperties.TryGetValue(Constants.PLAYER_READY, out isPlayerReady))
        {
            playerListItem.GetComponent<PlayerListItemInitializer>().SetPlayerReady((bool)isPlayerReady);
        }

        playerListGameObjects.Add(player.ActorNumber, playerListItem);
    }

    private void DestroyAndRemovePlayerListItem(Player player)
    {
        Destroy(playerListGameObjects[player.ActorNumber].gameObject);
        playerListGameObjects.Remove(player.ActorNumber);
    }

    private void DestroyAndClearPlayerListGameObjects()
    {
        foreach (GameObject playerListGameObject in playerListGameObjects.Values)
        {
            Destroy(playerListGameObject);
        }

        playerListGameObjects.Clear();
        playerListGameObjects = null;
    }

    private void UpdateRoomInfoText()
    {
        RoomInfoText.text = $"Room name: {PhotonNetwork.CurrentRoom.Name} {PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}";
    }

    private void SetGameModeTextToChosenGameMode()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue("rc"))
        {
            GameModeText.text = "Racing Mode";
        }
        else if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue("dr"))
        {
            GameModeText.text = "Death Race Mode";
        }
    }
    
    private void LoadGameModeScene()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("gm"))
        {
            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue("rc"))
            {
                PhotonNetwork.LoadLevel("RacingScene");
            }
            else if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue("dr"))
            {
                PhotonNetwork.LoadLevel("DeathRaceScene");
            }
        }
    }
    
    private bool CheckIfAllPlayersAreReady()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return false;
        }

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.CustomProperties.TryGetValue(Constants.PLAYER_READY, out object isPlayerReady))
            {
                if (!(bool)isPlayerReady)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        return true;
    }
    
    #endregion
}
