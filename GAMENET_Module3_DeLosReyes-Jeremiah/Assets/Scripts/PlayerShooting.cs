using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public enum WeaponTypes
{
    Laser,
    MachineGun
}

public class PlayerShooting : MonoBehaviourPunCallbacks
{
    public WeaponTypes WeaponType;
    public GameObject Nozzle;
    public GameObject BulletPrefab;
    public float BulletSpeed = 500.0f;
    public int Damage = 10;

    public float FireRate = 0.1f;
    private float FireTimer = 0.0f;

    public bool IsControlEnabled = false;

    private LineRenderer LaserLineRenderer;

    private void Start()
    {
        if (WeaponType == WeaponTypes.Laser)
        {
            LaserLineRenderer = GetComponent<LineRenderer>();
            LaserLineRenderer.startWidth = 0.1f;
            LaserLineRenderer.endWidth = 0.1f;
        }
    }

    private void Update()
    {
        if (IsControlEnabled)
        {
            if (FireTimer < FireRate)
            {
                FireTimer += Time.deltaTime;
            }

            if (Input.GetMouseButtonDown(0) && FireTimer > FireRate)
            {
                if (photonView.IsMine)
                {
                    if (WeaponType == WeaponTypes.Laser)
                    {
                        photonView.RPC("ShootLaser", RpcTarget.AllBuffered);
                    }
                    else if (WeaponType == WeaponTypes.MachineGun)
                    {
                        photonView.RPC("ShootMachineGun", RpcTarget.AllBuffered);
                    }
                }
            }
        }
    }

    [PunRPC]
    public void ShootLaser()
    {
        Ray ray = new Ray(Nozzle.transform.position, Nozzle.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, 100))
        {
            if (LaserLineRenderer != null && Nozzle != null)
            {
                LaserLineRenderer.SetPosition(0, Nozzle.transform.position);
                LaserLineRenderer.SetPosition(1, hit.point);
            }
            
            Debug.Log(hit.collider.gameObject.name);

            if (hit.collider.CompareTag("Player") && !hit.collider.GetComponent<PhotonView>().IsMine)
            {
                hit.collider.GetComponent<PhotonView>().RPC("ApplyDamage", RpcTarget.AllBuffered, Damage);
            }
        }
        else
        {
            if (LaserLineRenderer != null && Nozzle != null)
            {
                LaserLineRenderer.SetPosition(0, Nozzle.transform.position);
                LaserLineRenderer.SetPosition(1, Nozzle.transform.position + Nozzle.transform.forward * 100);
            }
        }

        if (photonView.IsMine)
        {
            photonView.RPC("HideLaser", RpcTarget.AllBuffered, 0.1f);
        }
    }

    [PunRPC]
    public void HideLaser(float delay)
    {
        if (photonView.IsMine)
        {
            Invoke("HideLaserImplementation", delay);
        }
    }

    [PunRPC]
    private void ShootMachineGun()
    {
        if (photonView.IsMine)
        {
            GameObject bullet = PhotonNetwork.Instantiate(BulletPrefab.name, Nozzle.transform.position, Nozzle.transform.rotation);
            bullet.GetComponent<Rigidbody>().velocity = Nozzle.transform.forward * BulletSpeed;
            bullet.GetComponent<Bullet>().Damage = Damage;
            bullet.GetComponent<Bullet>().OwnerVehiclePhotonView = GetComponent<PhotonView>();
        }
    }

    private void HideLaserImplementation()
    {
        if (photonView.IsMine)
        {
            if (LaserLineRenderer != null && Nozzle != null)
            {
                LaserLineRenderer.SetPosition(0, Nozzle.transform.position);
                LaserLineRenderer.SetPosition(1, Nozzle.transform.position);
            }   
        }
    }
}
