using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    [Header("Bullet Settings")]
    public GameObject fireBullet;

    void Update()
    {
        ShootBullet();
    }

    // Creeaza un proiectil cand se apasa tasta S
    void ShootBullet()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            GameObject bullet = Instantiate(fireBullet, transform.position, Quaternion.identity);
            bullet.GetComponent<FireBullet>().Speed *= transform.localScale.x;
        }
    }
}