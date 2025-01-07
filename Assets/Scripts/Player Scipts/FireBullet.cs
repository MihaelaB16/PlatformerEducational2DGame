using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBullet : MonoBehaviour
{
    private float speed = 10f;
    private Animator anim;
    private bool canMove;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }
    void Start()
    {
        canMove= true;
        StartCoroutine(DisableBullet(5f));
    }

    void Update()
    {
        Move();
    }
    void Move()
    {

        if (canMove)
        {
            //nu merge asa ca stocam intr-o variabila temporara
            //transform.position +=speed*Time.deltaTime;

            Vector3 temp = transform.position;
            temp.x += speed * Time.deltaTime;
            transform.position = temp;

        }
    }

    public float Speed
    {
        get
        {
            return speed;
        }
        set
        {
            speed = value;
        }
    }

    IEnumerator DisableBullet(float timer)
    {
        yield return new WaitForSeconds(timer);
        gameObject.SetActive(false);
        // Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D target)
    {
        Debug.Log("Collision with: " + target.gameObject.name);
        if (target.gameObject.tag == MyTags.BEETLE_TAG || target.gameObject.tag == MyTags.SNAIL_TAG || target.gameObject.tag == MyTags.SPIDER_TAG || target.gameObject.tag ==MyTags.FROG_TAG)
        {
            anim.Play("Explode");
            canMove = false;
            StartCoroutine(DisableBullet(0.2f));
        }
    }
}// class



















