using UnityEngine;
using Photon.Pun;

public class Shooting : MonoBehaviour
{
    [SerializeField] private Camera _fpsCamera;

    [SerializeField] private float _fireRate = 0.1f;
    private float _fireTimer = 0.0f;

    private int _damage = 10;

    private void Update()
    {
        if (_fireTimer < _fireRate)
        {
            _fireTimer += Time.deltaTime;
        }

        if (Input.GetButton("Fire1") && _fireTimer > _fireRate)
        {
            _fireTimer = 0.0f; 
            Ray ray = _fpsCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100))
            {
                Debug.Log(hit.collider.gameObject.name);
                if (hit.collider.gameObject.CompareTag("Player") && !hit.collider.gameObject.GetComponent<PhotonView>().IsMine)
                {
                    hit.collider.gameObject.GetComponent<PhotonView>().RPC("ApplyDamage", RpcTarget.AllBuffered, _damage);
                }
            }
        }
    }
}
