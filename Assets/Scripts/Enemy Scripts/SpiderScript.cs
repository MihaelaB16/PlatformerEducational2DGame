using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderScript : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D myBody;
    private Vector3 moveDirection = Vector3.down;
    private string coroutine_Name = "ChangeMovement";

    private void Awake()
    {
        anim = GetComponent<Animator>();
        myBody = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        StartCoroutine(coroutine_Name);
    }

    void Update()
    {
        MoveSpider();
    }

    // Misca paianjenul pe verticala
    void MoveSpider()
    {
        transform.Translate(moveDirection * Time.smoothDeltaTime);
    }

    // Schimba directia de miscare la intervale aleatorii
    IEnumerator ChangeMovement()
    {
        yield return new WaitForSeconds(Random.Range(2f, 5f));

        if (moveDirection == Vector3.down)
        {
            moveDirection = Vector3.up;
        }
        else
        {
            moveDirection = Vector3.down;
        }

        StartCoroutine(coroutine_Name);
    }

    IEnumerator SpiderDead()
    {
        yield return new WaitForSeconds(3f);
        gameObject.SetActive(false);
    }

    // Gestioneaza coliziunile cu proiectilele si jucatorul
    void OnTriggerEnter2D(Collider2D target)
    {
        if (target.tag == MyTags.BULLET_TAG)
        {
            anim.Play("SpiderDead");
            myBody.bodyType = RigidbodyType2D.Dynamic;
            StartCoroutine(SpiderDead());
            StopCoroutine(coroutine_Name);
        }
        else if (target.tag == MyTags.PLAYER_TAG)
        {
            target.gameObject.GetComponent<PlayerDamage>().DealDamage();
        }
    }
}