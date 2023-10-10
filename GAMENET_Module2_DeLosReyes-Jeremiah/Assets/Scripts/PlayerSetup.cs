using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityStandardAssets.Characters.FirstPerson;
using TMPro;

public class PlayerSetup : MonoBehaviourPunCallbacks
{
    [Header("Player Models")]
    [SerializeField] private GameObject _fpsModel;
    [SerializeField] private GameObject _nonFPSModel;

    [Header("Player UI")]
    [SerializeField] private GameObject _playerUI;
    [SerializeField] private GameObject _playerUIRespawnPanel;
    [SerializeField] private GameObject _playerUIWinnerPanel;
    [SerializeField] private Button _playerUIFireButton;
    [SerializeField] private Text _numberOfKillsText;
    [SerializeField] private Text _killAnnouncerText;
    [SerializeField] private Text _winnerNameText;
    [SerializeField] private TextMeshProUGUI _playerNameText;

    [Header("Player FPS Camera")]
    [SerializeField] private Camera _fpsCamera;

    [Header("Player Animator Avatars")]
    [SerializeField] private Avatar _fpsAvatar;
    [SerializeField] private Avatar _nonFPSAvatar;

    [Header("Health Component")]
    [SerializeField] private Health _health;

    private PlayerMovementController _playerMovementController;
    private Shooting _shooting;
    private Animator _animator;

    private int _killCount = 0;
    private int _numberOfKillsToWin = 10;

    private void Start()
    {
        _fpsModel.SetActive(photonView.IsMine);
        _nonFPSModel.SetActive(!photonView.IsMine);

        _playerMovementController = GetComponent<PlayerMovementController>();
        _shooting = GetComponent<Shooting>();

        _animator = GetComponent<Animator>();
        _animator.SetBool("isLocalPlayer", photonView.IsMine);
        _animator.avatar = photonView.IsMine ? _fpsAvatar : _nonFPSAvatar;

        if (photonView.IsMine)
        {
            _playerUI.SetActive(true);
            _playerUIRespawnPanel.SetActive(false);
            _playerUIWinnerPanel.SetActive(false);
            _playerUIFireButton.onClick.AddListener(_shooting.Fire);

            _fpsCamera.enabled = true;
        }
        else
        {
            _playerUI.SetActive(false);
            _playerMovementController.enabled = false;
            _fpsCamera.enabled = false;
            GetComponent<RigidbodyFirstPersonController>().enabled = false;
        }

        _playerNameText.text = photonView.Owner.NickName;
    }

    [PunRPC]
    public void IncreaseKillCount()
    {
        _killCount++;
        UpdateKillCountUI();

        if (_killCount >= _numberOfKillsToWin && photonView.IsMine)
        {
            GameManager.Instance.DisplayWinner(photonView.Owner.NickName);
        }
    }

    [PunRPC]
    public void AnnounceKill(string killerName, string victimName)
    {
        _killAnnouncerText.text = $"{killerName} killed {victimName}";
        StartCoroutine(CO_FadeKillAnnouncement());
        StartCoroutine(CO_MoveKillAnnouncement());
    }

    [PunRPC]
    public void AnnounceWinner(string winnerName)
    {
        StartCoroutine(CO_ReturnToLobbyCountdown(winnerName));
    }

    private void UpdateKillCountUI()
    {
        _numberOfKillsText.text = $"Kills: {_killCount}";
    }

    private IEnumerator CO_FadeKillAnnouncement()
    {
        float fadeDuration = 1.5f;
        Color originalColor = _killAnnouncerText.color;
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            float opacity = 1.0f - (t / fadeDuration);
            _killAnnouncerText.color = new Color(originalColor.r, originalColor.g, originalColor.b, opacity);
            yield return null;
        }

        _killAnnouncerText.text = "";
        _killAnnouncerText.color = originalColor;
    }

    private IEnumerator CO_MoveKillAnnouncement()
    {
        float moveDuration = 1.5f;
        float amountOfDisplacement = 50.0f;

        float startPositionY = _killAnnouncerText.rectTransform.localPosition.y;
        float targetPositionY = startPositionY + amountOfDisplacement;

        Vector3 originalPosition = _killAnnouncerText.rectTransform.localPosition;

        for (float t = 0; t < moveDuration; t += Time.deltaTime)
        {
            float newPositionY = Mathf.Lerp(startPositionY, targetPositionY, t / moveDuration);
            _killAnnouncerText.rectTransform.localPosition = new Vector3(originalPosition.x, newPositionY, originalPosition.z);
            yield return null;
        }

        _killAnnouncerText.rectTransform.localPosition = originalPosition;
    }

    private IEnumerator CO_ReturnToLobbyCountdown(string winnerName)
    {
        _playerMovementController.enabled = false;
        _playerUIRespawnPanel.SetActive(false);
        _playerUIWinnerPanel.SetActive(true);

        float countdownTime = 5.0f;
        while (countdownTime > 0.0f)
        {
            yield return new WaitForSeconds(1.0f);
            countdownTime--;

            _winnerNameText.text = $"{winnerName} wins! Returning to lobby in {countdownTime:.00}.";
        }

        _winnerNameText.text = "";
        _playerUIWinnerPanel.SetActive(false);

        GameManager.Instance.LeaveRoom();
    }
}
