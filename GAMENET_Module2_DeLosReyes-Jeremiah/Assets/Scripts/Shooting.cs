using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Shooting : MonoBehaviourPunCallbacks
{
    [SerializeField] private Camera _camera;
    [SerializeField] private GameObject _hitEffectPrefab;

    private int _damage = 25;
    private float _hitEffectDuration = 0.2f;

    public void Fire()
    {
        Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        if (Physics.Raycast(ray, out RaycastHit hit, 200))
        {
            Debug.Log(hit.collider.gameObject.name);

            photonView.RPC("CreateHitEffects", RpcTarget.All, hit.point);

            if (hit.collider.gameObject.CompareTag("Player") && !hit.collider.gameObject.GetComponent<PhotonView>().IsMine)
            {
                int attackerViewID = photonView.ViewID;
                hit.collider.gameObject.GetComponent<PhotonView>().RPC("ApplyDamage", RpcTarget.AllBuffered, _damage, attackerViewID);
            }
        }
    }

    [PunRPC]
    public void CreateHitEffects(Vector3 position)
    {
        GameObject hitEffect = Instantiate(_hitEffectPrefab, position, Quaternion.identity);
        Destroy(hitEffect, _hitEffectDuration);
    }
}
