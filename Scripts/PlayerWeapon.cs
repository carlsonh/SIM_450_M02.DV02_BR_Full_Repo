using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerWeapon : MonoBehaviour
{
    [Header("Stats")]
    public int damage;
    public int curAmmo;
    public int maxAmmo;
    public float bulletSpeed;
    public float shootRate;

    private float lastShootTime;

    public GameObject bulletPrefab;
    public Transform bulletSpawnPos;

    private PlayerController player;


    void Awake()
    {
        //get required components
        player = GetComponent<PlayerController>();
    }

    public void TryShoot()
    {
        //Can we shoot?
        if (curAmmo <= 0 || Time.time - lastShootTime < shootRate)
        {//Cannot shoot
            return;
        }

        curAmmo--;
        lastShootTime = Time.time;

        //Update ammo ui

        //Spawn bullet

        player.photonView.RPC("SpawnBullet", RpcTarget.All, bulletSpawnPos.transform.position, Camera.main.transform.forward);
    }


    [PunRPC]
    void SpawnBullet(Vector3 pos, Vector3 dir)
    {
        
        //Spawn & orient bullet
        GameObject bulletGO = Instantiate(bulletPrefab, pos, Quaternion.identity);
        bulletGO.transform.forward = dir;

        //Get bullet script
        Bullet bulletScript = bulletGO.GetComponent<Bullet>();

        bulletScript.Initialize(damage, player.id, player.photonView.IsMine);
        bulletScript.rb.velocity = dir * bulletSpeed;
    }


   
}
