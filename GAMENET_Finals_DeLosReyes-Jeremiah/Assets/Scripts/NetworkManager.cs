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
    [SerializeField] private GameObject loginUIPanel;
    [SerializeField] private InputField playerNameInput;

    [Header("Connecting Info Panel")]
    [SerializeField] private GameObject connectingInfoUIPanel;

    [Header("Creating Room Info Panel")]
    [SerializeField] private GameObject creatingRoomInfoUIPanel;

    [Header("GameOptions Panel")]
    [SerializeField] private GameObject gameOptionsUIPanel;

    [Header("Create Room Panel")]
    [SerializeField] private GameObject createRoomUIPanel;
    [SerializeField] private InputField roomNameInputField;
    [SerializeField] private string gameMode;

    [Header("Inside Room Panel")]
    [SerializeField] private GameObject insideRoomUIPanel;
    [SerializeField] private GameObject startGameButton;
    [SerializeField] private GameObject playerListPrefab;
    [SerializeField] private GameObject playerListParent;
    [SerializeField] private Text roomInfoText;
    [SerializeField] private Text gameModeText;
    [SerializeField] private Button playerSelectionNextButton;
    [SerializeField] private Button playerSelectionPreviousButton;
   
    [Header("Join Random Room Panel")]
    [SerializeField] private GameObject joinRandomRoomUIPanel;

    private Dictionary<int, GameObject> playerLists;

    private PlayerSelection playerSelection;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        playerSelection = GetComponent<PlayerSelection>();
    }

    private void Start()
    {
        ActivatePanel(loginUIPanel.name);
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    #endregion

    #region UI Callback Methods
    public void OnLoginButtonClicked()
    {
        string playerName = playerNameInput.text;

        if (!string.IsNullOrEmpty(playerName))
        {
            ActivatePanel(connectingInfoUIPanel.name);

            if (!PhotonNetwork.IsConnected)
            {
                PhotonNetwork.LocalPlayer.NickName = playerName;
                PhotonNetwork.ConnectUsingSettings();
            }
        }
        else
        {
            Debug.Log("Player Name is invalid!");
        }
    }

    public void OnCancelButtonClicked()
    {
        ActivatePanel(gameOptionsUIPanel.name);
    }
    
    public void OnCreateRoomButtonClicked()
    {
        if (gameMode != null)
        {
            ActivatePanel(creatingRoomInfoUIPanel.name);
            CreateRoomWithGameModeProperties();
        }
    }

    public void OnJoinRandomRoomClicked(string gameModeName)
    {
        gameMode = gameModeName;

        ExitGames.Client.Photon.Hashtable expectedCustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { Constants.GAME_MODE, gameMode } };
        PhotonNetwork.JoinRandomRoom(expectedCustomRoomProperties, 0);
    }

    public void OnBackButtonClicked()
    {
        ActivatePanel(gameOptionsUIPanel.name);
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
        Debug.Log($"{PhotonNetwork.LocalPlayer.NickName} is connected to Photon");
        ActivatePanel(gameOptionsUIPanel.name);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log($"{PhotonNetwork.CurrentRoom} has been created.");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"{PhotonNetwork.LocalPlayer.NickName} has joined {PhotonNetwork.CurrentRoom.Name}.");
        Debug.Log($"Player count: {PhotonNetwork.CurrentRoom.PlayerCount}");

        ActivatePanel(insideRoomUIPanel.name);

        if (insideRoomUIPanel.activeSelf)
        {
            Debug.Log("Active");
        }

        UpdateRoomWithChosenGameMode();

        if (playerLists == null)
        {
            playerLists = new Dictionary<int, GameObject>();
        }

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            CreatePlayerListItem(player);
        }

        startGameButton.SetActive(CheckIfAllPlayersAreReady());
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log(message);

        if (gameMode != null)
        {
            CreateRoomWithGameModeProperties();
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        CreatePlayerListItem(newPlayer);
        UpdateRoomInfoText();

        startGameButton.SetActive(CheckIfAllPlayersAreReady());
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        DestroyAndRemovePlayerListItem(otherPlayer);
        UpdateRoomInfoText();
    }

    public override void OnLeftRoom()
    {
        ActivatePanel(gameOptionsUIPanel.name);
        DestroyAndClearPlayerLists();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (playerLists.TryGetValue(targetPlayer.ActorNumber, out GameObject playerListItem))
        {
            if (changedProps.TryGetValue(Constants.PLAYER_READY, out object isPlayerReady))
            {
                playerListItem.GetComponent<PlayerListItemInitializer>().SetPlayerReadyImage((bool)isPlayerReady);
            }
        }

        playerSelection.ActivateSelectablePlayer(playerSelection.PlayerSelectionIndex);
        startGameButton.SetActive(CheckIfAllPlayersAreReady());
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == newMasterClient.ActorNumber)
        {
            startGameButton.SetActive(CheckIfAllPlayersAreReady());
        }
    }

    #endregion

    #region Public Methods

    public void ActivatePanel(string panelNameToBeActivated)
    {
        loginUIPanel.SetActive(loginUIPanel.name.Equals(panelNameToBeActivated));
        connectingInfoUIPanel.SetActive(connectingInfoUIPanel.name.Equals(panelNameToBeActivated));
        creatingRoomInfoUIPanel.SetActive(creatingRoomInfoUIPanel.name.Equals(panelNameToBeActivated));
        createRoomUIPanel.SetActive(createRoomUIPanel.name.Equals(panelNameToBeActivated));
        gameOptionsUIPanel.SetActive(gameOptionsUIPanel.name.Equals(panelNameToBeActivated));
        joinRandomRoomUIPanel.SetActive(joinRandomRoomUIPanel.name.Equals(panelNameToBeActivated));
        insideRoomUIPanel.SetActive(insideRoomUIPanel.name.Equals(panelNameToBeActivated));
    }

    public void SetGameMode(string gameModeName)
    {
        gameMode = gameModeName;
    }

    #endregion

    #region Private Methods

    private void CreateRoomWithGameModeProperties()
    {
        string roomName = roomNameInputField.text;

        if (string.IsNullOrEmpty(roomName))
        {
            roomName = $"Room {Random.Range(Constants.MIN_ROOM_RANGE_INDEX, Constants.MAX_ROOM_RANGE_INDEX)}";
        }

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = Constants.MAX_PLAYERS_IN_ROOM;

        string[] roomPropertiesInLobby = { Constants.GAME_MODE };

        ExitGames.Client.Photon.Hashtable expectedCustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { Constants.GAME_MODE, gameMode } };

        roomOptions.CustomRoomPropertiesForLobby = roomPropertiesInLobby;
        roomOptions.CustomRoomProperties = expectedCustomRoomProperties;

        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    private void InitializePlayerListItem(GameObject playerListItem, Player player)
    {
        playerListItem.transform.SetParent(playerListParent.transform);
        playerListItem.transform.localScale = Vector3.one;

        PlayerListItemInitializer playerListItemInitializer = playerListItem.GetComponent<PlayerListItemInitializer>();
        playerListItemInitializer.PlayerSelection = playerSelection;
        playerListItemInitializer.PlayerSelectionNextButton = playerSelectionNextButton;
        playerListItemInitializer.PlayerSelectionPreviousButton = playerSelectionPreviousButton;
        playerListItemInitializer.Initialize(player.ActorNumber, player.NickName);

        if (player.CustomProperties.TryGetValue(Constants.PLAYER_READY, out object isPlayerReady))
        {
            playerListItem.GetComponent<PlayerListItemInitializer>().SetPlayerReadyImage((bool)isPlayerReady);
        }
    }

    private void CreatePlayerListItem(Player player)
    {
        GameObject playerListItem = Instantiate(playerListPrefab);
        InitializePlayerListItem(playerListItem, player);
        playerLists.Add(player.ActorNumber, playerListItem);
    }

    private void DestroyAndRemovePlayerListItem(Player player)
    {
        Destroy(playerLists[player.ActorNumber].gameObject);
        playerLists.Remove(player.ActorNumber);
    }

    private void DestroyAndClearPlayerLists()
    {
        foreach (GameObject playerList in playerLists.Values)
        {
            Destroy(playerList);
        }

        playerLists.Clear();
        playerLists = null;
    }

    private void UpdateRoomInfoText()
    {
        roomInfoText.text = $"Room name: {PhotonNetwork.CurrentRoom.Name} {PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}";
    }

    private void SetGameModeTextToChosenGameMode()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue(Constants.GAME_MODE_FREE_FOR_ALL))
        {
            gameModeText.text = Constants.GAME_MODE_FREE_FOR_ALL_HEADING;
        }
        else if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue(Constants.GAME_MODE_ELEMENTAL_DOMINATION))
        {
            gameModeText.text = Constants.GAME_MODE_ELEMENTAL_DOMINATION_HEADING;
        }
    }

    private void UpdateRoomWithChosenGameMode()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(Constants.GAME_MODE, out object gameModeName))
        {
            Debug.Log(gameModeName.ToString());
            UpdateRoomInfoText();
            SetGameModeTextToChosenGameMode();
        }
    }

    private void LoadGameModeScene()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(Constants.GAME_MODE))
        {
            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue(Constants.GAME_MODE_FREE_FOR_ALL))
            {
                PhotonNetwork.LoadLevel(Constants.LEVEL_FREE_FOR_ALL_DEATHMATCH);
            }
            else if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue(Constants.GAME_MODE_ELEMENTAL_DOMINATION))
            {
                PhotonNetwork.LoadLevel(Constants.LEVEL_ELEMENTAL_DOMINATION);
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
