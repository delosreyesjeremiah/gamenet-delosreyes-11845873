using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Bullet : MonoBehaviourPunCallbacks
{
    public PhotonView OwnerVehiclePhotonView;
    public int Damage = 10;

    private void OnTriggerEnter(Collider other)
    {
        PhotonView otherPhotonView = other.GetComponent<PhotonView>();

        if (otherPhotonView != null)
        {
            if (OwnerVehiclePhotonView != null)
            {
                if (otherPhotonView.ViewID != OwnerVehiclePhotonView.ViewID)
                {
                    if (other.CompareTag("Player"))
                    {
                        Debug.Log("Collided with player");
                        otherPhotonView.RPC("ApplyDamage", RpcTarget.AllBuffered, Damage);
                    }

                    if (photonView.IsMine)
                    {
                        photonView.RPC("DestroyBullet", RpcTarget.AllBuffered);
                    }
                }
            }
        }
        else
        {
            if (photonView.IsMine)
            {
                photonView.RPC("DestroyBullet", RpcTarget.AllBuffered);
            }
        }
    }

    [PunRPC]
    public void DestroyBullet()
    {
        Destroy(gameObject);
    }
}
