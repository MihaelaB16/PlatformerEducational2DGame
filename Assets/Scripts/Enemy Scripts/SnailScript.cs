using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnailScript : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 1f;

    [Header("Collision Points")]
    public Transform left_Collision;
    public Transform right_Collision;
    public Transform top_Collision;
    public Transform down_Collision;

    [Header("Detection")]
    public LayerMask playerLayer;

    private Rigidbody2D myBody;
    private Animator anim;
    private AudioSource coinSound;
    private bool moveLeft;
    private bool canMove;
    private bool stunned;
    private int hitCount = 0;
    private bool isProcessingHit = false;

    void Awake()
    {
        myBody = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        coinSound = GetComponent<AudioSource>();
    }

    void Start()
    {
        moveLeft = true;
        canMove = true;
    }

    void Update()
    {
        MoveEnemy();
        CheckCollision();
    }

    // Controleaza miscarea inamicului
    void MoveEnemy()
    {
        if (canMove)
        {
            if (moveLeft)
            {
                myBody.velocity = new Vector2(-moveSpeed, myBody.velocity.y);
            }
            else
            {
                myBody.velocity = new Vector2(moveSpeed, myBody.velocity.y);
            }
        }
    }

    // Verifica coliziunile cu jucatorul si mediul
    void CheckCollision()
    {
        RaycastHit2D leftHit = Physics2D.Raycast(left_Collision.position, Vector2.left, 0.1f, playerLayer);
        RaycastHit2D rightHit = Physics2D.Raycast(right_Collision.position, Vector2.right, 0.1f, playerLayer);
        Collider2D topHit = Physics2D.OverlapCircle(top_Collision.position, 0.2f, playerLayer);

        // Verifica daca jucatorul sare pe inamic
        if (topHit != null && !isProcessingHit && topHit.gameObject.tag == MyTags.PLAYER_TAG)
        {
            isProcessingHit = true;

            if (!stunned)
            {
                topHit.gameObject.GetComponent<Rigidbody2D>().velocity =
                    new Vector2(topHit.gameObject.GetComponent<Rigidbody2D>().velocity.x, 7f);

                hitCount++;

                if (hitCount >= 2)
                {
                    if (coinSound != null)
                        coinSound.Play();

                    GameManager.instance.AddScore(1);
                    canMove = false;
                    myBody.velocity = new Vector2(0, 0);
                    anim.Play("Stunned");
                    stunned = true;

                    if (tag == MyTags.BEETLE_TAG)
                    {
                        StartCoroutine(Dead(0.2f));
                    }
                }
                else
                {
                    anim.Play("Stunned");
                }
                StartCoroutine(ResetHitProcessing());
            }
        }

        // Verifica coliziunile laterale cu jucatorul
        if (leftHit && leftHit.collider.gameObject.tag == MyTags.PLAYER_TAG)
        {
            if (!stunned)
            {
                leftHit.collider.gameObject.GetComponent<PlayerDamage>().DealDamage();
            }
            else if (tag != MyTags.BEETLE_TAG)
            {
                myBody.velocity = new Vector2(15f, myBody.velocity.y);
                StartCoroutine(Dead(3f));
            }
        }

        if (rightHit && rightHit.collider.gameObject.tag == MyTags.PLAYER_TAG)
        {
            if (!stunned)
            {
                rightHit.collider.gameObject.GetComponent<PlayerDamage>().DealDamage();
            }
            else if (tag != MyTags.BEETLE_TAG)
            {
                myBody.velocity = new Vector2(-15f, myBody.velocity.y);
                StartCoroutine(Dead(3f));
            }
        }

        // Schimba directia daca nu mai exista pamant in fata
        if (!Physics2D.Raycast(down_Collision.position, Vector2.down, 0.01f))
        {
            ChangeDirection();
        }
    }

    // Schimba directia de miscare si orientarea sprite-ului
    void ChangeDirection()
    {
        moveLeft = !moveLeft;
        Vector3 tempScale = transform.localScale;

        if (moveLeft)
        {
            tempScale.x = Mathf.Abs(tempScale.x);
        }
        else
        {
            tempScale.x = -Mathf.Abs(tempScale.x);
        }

        transform.localScale = tempScale;
    }

    IEnumerator Dead(float timer)
    {
        yield return new WaitForSeconds(timer);
        gameObject.SetActive(false);
    }

    IEnumerator ResetHitProcessing()
    {
        yield return new WaitForSeconds(0.1f);
        isProcessingHit = false;
    }

    // Gestioneaza coliziunea cu proiectilele
    void OnTriggerEnter2D(Collider2D target)
    {
        if (target.tag == MyTags.BULLET_TAG)
        {
            if (tag == MyTags.BEETLE_TAG)
            {
                anim.Play("Stunned");
                canMove = false;
                myBody.velocity = new Vector2(0, 0);
                StartCoroutine(Dead(0.4f));
            }
            else if (tag == MyTags.SNAIL_TAG)
            {
                if (!stunned)
                {
                    anim.Play("Stunned");
                    canMove = false;
                    stunned = true;
                    myBody.velocity = new Vector2(0, 0);
                }
                else
                {
                    gameObject.SetActive(false);
                }
            }
        }
    }
}