using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class Health : MonoBehaviourPunCallbacks
{
    [SerializeField] private Image _healthBar;

    private int _currentHealth;
    private int _maxHealth = 100;

    private void Start()
    {
        _currentHealth = _maxHealth;
        _healthBar.fillAmount = _currentHealth / _maxHealth;
    }

    [PunRPC]
    public void ApplyDamage(int damage)
    {
        _currentHealth -= damage;
        _healthBar.fillAmount = (float)_currentHealth / (float)_maxHealth;
        Debug.Log(_currentHealth);
        if (_currentHealth <= 0 )
        {
            Die();
        }
    }

    private void Die()
    {
        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
            GameManager.Instance.LeaveRoom();
        }    
    }
}
