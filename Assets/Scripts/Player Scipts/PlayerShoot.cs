using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{

    public GameObject fireBullet;

   


    void Update()
    {
        ShootBullet();
    }


    void ShootBullet()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            GameObject bullet= Instantiate(fireBullet, transform.position, Quaternion.identity); // nu e nevoie de rotire dar trebuia pus parametrul asa ca am pus rotire pe 0 0 0
            bullet.GetComponent<FireBullet>().Speed *= transform.localScale.x;
        }
    }
} //class





















