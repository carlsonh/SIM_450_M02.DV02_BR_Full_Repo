﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private int damage;
    private int attackerId;
    private bool isMine;

    public Rigidbody rb;

    public void Initialize(int damage, int attackerId, bool isMine)
    {
        this.damage = damage;
        this.attackerId = attackerId;
        this.isMine = isMine;

        Destroy(gameObject, 7.0f);
    }

    void OnTriggerEnter (Collider other)
    {
        //Did hit other player?
        if (other.CompareTag("Player") && isMine)
        {
            PlayerController player = GameManager.instance.GetPlayer(other.gameObject);

            if (player.id != attackerId)
            {
                player.photonView.RPC("TakeDamage", player.photonPlayer, attackerId, damage);
            }
        }
        Destroy(gameObject);
    }
}
