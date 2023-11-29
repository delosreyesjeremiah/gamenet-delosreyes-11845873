using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Shooting : MonoBehaviourPunCallbacks
{
    [SerializeField] private Camera fpsCamera;
    [SerializeField] private float fireRate = 0.1f;

    private float fireTimer = 0.0f;

    private void Update()
    {
        if (photonView.IsMine)
        {
            if (fireTimer < fireRate)
            {
                fireTimer += Time.deltaTime;
            }

            if (Input.GetButton("Fire1") && fireTimer > fireRate)
            {
                fireTimer = 0.0f;
                Ray ray = fpsCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
                if (Physics.Raycast(ray, out RaycastHit hit, 100))
                {
                    Debug.Log(hit.collider.gameObject.name);
                    if (hit.collider.gameObject.CompareTag("Player") && !hit.collider.gameObject.GetComponent<PhotonView>().IsMine)
                    {
                        int attackerViewID = photonView.ViewID;
                        hit.collider.gameObject.GetComponent<PhotonView>().RPC("ApplyDamage", RpcTarget.AllBuffered, attackerViewID);
                    }
                }
            }
        }
    }
}
