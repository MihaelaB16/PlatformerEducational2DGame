using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrogScript : MonoBehaviour
{
    [Header("Detection Settings")]
    public LayerMask playerLayer;

    private Animator anim;
    private Rigidbody2D myBody;
    private GameObject player;
    private bool animation_Started;
    private bool animation_Finished;
    private int jumedTimes;
    private bool jumpLeft = true;
    private string coroutine_Name = "FrogJump";

    void Awake()
    {
        anim = GetComponent<Animator>();
        myBody = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        StartCoroutine(coroutine_Name);
        player = GameObject.FindGameObjectWithTag(MyTags.PLAYER_TAG);
    }

    private void Update()
    {
        // Verifica daca jucatorul este in apropiere pentru a aplica damage
        if (Physics2D.OverlapCircle(transform.position, 0.3f, playerLayer))
        {
            player.GetComponent<PlayerDamage>().DealDamage();
        }
    }

    void LateUpdate()
    {
        if (animation_Finished && animation_Started)
        {
            animation_Started = false;
            transform.parent.position = transform.position;
            transform.localPosition = Vector3.zero;
        }
    }

    // Gestioneaza saritul periodic al broastei
    IEnumerator FrogJump()
    {
        yield return new WaitForSeconds(Random.Range(1f, 4f));

        animation_Started = true;
        animation_Finished = false;
        jumedTimes++;

        if (jumpLeft)
        {
            anim.Play("FrogJumpLeft");
        }
        else
        {
            anim.Play("FrogJumpRight");
        }

        StartCoroutine(coroutine_Name);
    }

    // Apelata de animatie cand saritura se termina
    void AnimationFinished()
    {
        animation_Finished = true;

        if (jumpLeft)
        {
            anim.Play("FrogIdleLeft");
        }
        else
        {
            anim.Play("FrogIdleRight");
        }

        // Schimba directia dupa 3 sarituri
        if (jumedTimes == 3)
        {
            jumedTimes = 0;
            Vector3 temp = transform.localScale;
            temp.x = temp.x * -1;
            transform.localScale = temp;
            jumpLeft = !jumpLeft;
        }
    }

    IEnumerator FrogDead()
    {
        yield return new WaitForSeconds(1f);
        gameObject.SetActive(false);
    }

    // Gestioneaza coliziunea cu proiectilele
    void OnTriggerEnter2D(Collider2D target)
    {
        if (target.tag == MyTags.BULLET_TAG)
        {
            anim.Play("FrogDead");
            myBody.bodyType = RigidbodyType2D.Dynamic;
            StartCoroutine(FrogDead());
            StopCoroutine(coroutine_Name);
        }
    }
}