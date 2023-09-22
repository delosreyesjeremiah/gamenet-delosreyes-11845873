using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks
{
    #region Properties

    public static GameManager Instance
    {
        get => _instance;
    }

    #endregion

    #region Fields

    [SerializeField] private GameObject _playerPrefab;
    private float _playerSpawnDelay = 3.0f;

    private static GameManager _instance;

    #endregion

    #region Unity Messages

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            if (_playerPrefab != null)
            {
                StartCoroutine(CO_DelayPlayerSpawn());
            }
        }
    }

    private void OnDestroy()
    {
        _instance = null;
    }

    #endregion

    #region Photon Server Methods

    public override void OnJoinedRoom()
    {
        Debug.Log($"{PhotonNetwork.NickName} has joined the room.");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"{newPlayer.NickName} has entered {PhotonNetwork.CurrentRoom.Name}.");
        Debug.Log($"The room now has {PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers} players.");
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("GameLauncherScene");
    }

    #endregion

    #region Public Methods

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    #endregion

    #region Private Methods

    private IEnumerator CO_DelayPlayerSpawn()
    {
        yield return new WaitForSeconds(_playerSpawnDelay);
        Vector3 randomSpawnLocation = new Vector3(Random.Range(-40, 40), 0.0f, Random.Range(-40, 40));
        PhotonNetwork.Instantiate(_playerPrefab.name, randomSpawnLocation, Quaternion.identity);
    }

    #endregion
}
