using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public enum ElementType
{
    Fire,
    Water,
    Earth,
    None
}

public class PlayerSetup : MonoBehaviourPunCallbacks
{
    public ElementType ElementType
    {
        get => elementType;
        set => elementType = value;
    }

    public float Damage
    {
        get => damage;
        set => damage = value;
    }
    
    public float Defense
    {
        get => defense;
        set => defense = value;
    }

    [Header("Player Stats")]
    [SerializeField] private ElementType elementType;
    [SerializeField] private float damage;
    [SerializeField] private float defense;
    [SerializeField] private float speed;

    [Header("Player FPS Camera")]
    [SerializeField] private Camera fpsCamera;

    [Header("Player UI")]
    [SerializeField] private GameObject playerUI;
    [SerializeField] private GameObject playerUIRespawnPanel;
    [SerializeField] private GameObject playerUIWinnerPanel;
    [SerializeField] private TextMeshProUGUI numberOfKillsText;
    [SerializeField] private TextMeshProUGUI killAnnouncerText;
    [SerializeField] private TextMeshProUGUI winnerNameText;
    [SerializeField] private TextMeshProUGUI playerNameUIText;

    private PlayerMovementController playerMovementController;
    private Shooting shooting;

    private int killCount = 0;
    private int numberOfKillsToWin = 10;

    private void Awake()
    {
        playerMovementController = GetComponent<PlayerMovementController>();
        shooting = GetComponent<Shooting>();
    }

    private void Start()
    {
        playerUI.SetActive(photonView.IsMine);
        playerUIRespawnPanel.SetActive(false);
        playerUIWinnerPanel.SetActive(false);

        playerMovementController.enabled = photonView.IsMine;
        shooting.enabled = photonView.IsMine;
        fpsCamera.enabled = photonView.IsMine;

        playerNameUIText.text = photonView.Owner.NickName;

        InitializePlayerStats();
    }

    [PunRPC]
    public void IncreaseKillCount()
    {
        killCount++;
        UpdateKillCountUI();

        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue(Constants.GAME_MODE_FREE_FOR_ALL))
        {
            if (killCount >= numberOfKillsToWin && photonView.IsMine)
            {
                FreeForAllDeathmatchGameManager.Instance.DisplayWinner(photonView.Owner.NickName);
            }
        }
    }

    [PunRPC]
    public void AnnounceKill(string killerName, string victimName)
    {
        killAnnouncerText.text = $"{killerName} killed {victimName}";
        StartCoroutine(CO_FadeKillAnnouncement());
        StartCoroutine(CO_MoveKillAnnouncement());
    }

    [PunRPC]
    public void AnnounceWinner(string winnerName)
    {
        StartCoroutine(CO_ReturnToLobbyCountdown(winnerName));
    }

    private void InitializePlayerStats()
    {
        switch (elementType)
        {
            case ElementType.Fire:
                damage = Constants.FIRE_ELEMENTAL_DAMAGE;
                defense = Constants.FIRE_ELEMENTAL_DEFENSE;
                speed = Constants.FIRE_ELEMENTAL_SPEED;
                playerMovementController.MovementSpeed = speed;
                break;
            case ElementType.Water:
                damage = Constants.WATER_ELEMENTAL_DAMAGE;
                defense = Constants.WATER_ELEMENTAL_DEFENSE;
                speed = Constants.WATER_ELEMENTAL_SPEED;
                playerMovementController.MovementSpeed = speed;
                break;
            case ElementType.Earth:
                damage = Constants.EARTH_ELEMENTAL_DAMAGE;
                defense = Constants.EARTH_ELEMENTAL_DEFENSE;
                speed = Constants.EARTH_ELEMENTAL_SPEED;
                playerMovementController.MovementSpeed = speed;
                break;
            case ElementType.None:
                break;
        }
    }

    private void UpdateKillCountUI()
    {
        numberOfKillsText.text = $"Kills: {killCount}";
    }

    private IEnumerator CO_FadeKillAnnouncement()
    {
        float fadeDuration = 1.5f;
        Color originalColor = killAnnouncerText.color;
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            float opacity = 1.0f - (t / fadeDuration);
            killAnnouncerText.color = new Color(originalColor.r, originalColor.g, originalColor.b, opacity);
            yield return null;
        }

        killAnnouncerText.text = "";
        killAnnouncerText.color = originalColor;
    }

    private IEnumerator CO_MoveKillAnnouncement()
    {
        float moveDuration = 1.5f;
        float amountOfDisplacement = 50.0f;

        float startPositionY = killAnnouncerText.rectTransform.localPosition.y;
        float targetPositionY = startPositionY + amountOfDisplacement;

        Vector3 originalPosition = killAnnouncerText.rectTransform.localPosition;

        for (float t = 0; t < moveDuration; t += Time.deltaTime)
        {
            float newPositionY = Mathf.Lerp(startPositionY, targetPositionY, t / moveDuration);
            killAnnouncerText.rectTransform.localPosition = new Vector3(originalPosition.x, newPositionY, originalPosition.z);
            yield return null;
        }

        killAnnouncerText.rectTransform.localPosition = originalPosition;
    }

    private IEnumerator CO_ReturnToLobbyCountdown(string winnerName)
    {
        playerMovementController.enabled = false;
        playerUIRespawnPanel.SetActive(false);
        playerUIWinnerPanel.SetActive(true);

        float countdownTime = 5.0f;
        while (countdownTime > 0.0f)
        {
            yield return new WaitForSeconds(1.0f);
            countdownTime--;

            winnerNameText.text = $"{winnerName} wins! Returning to lobby in {countdownTime:.00}.";
        }

        winnerNameText.text = "";
        playerUIWinnerPanel.SetActive(false);

        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue(Constants.GAME_MODE_FREE_FOR_ALL))
        {
            FreeForAllDeathmatchGameManager.Instance.LeaveRoom();
        }
        else if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue(Constants.GAME_MODE_ELEMENTAL_DOMINATION))
        {
            ElementalDominationGameManager.Instance.LeaveRoom();
        }
    }
}
