using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnailScript : MonoBehaviour
{

    public float moveSpeed = 1f;
    private Rigidbody2D myBody;
    private Animator anim;

    private bool moveLeft;

    private bool canMove;
    private bool stunned;

    public Transform left_Collision ,right_Collision ,top_Collision ,down_Collision;

    private Vector3 left_Collision_Pos, right_Collision_Pos;

    public LayerMask playerLayer;

    private int hitCount = 0;
    private bool isProcessingHit = false;

    void Awake()
    {
        myBody = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        left_Collision_Pos = left_Collision.position;
        right_Collision_Pos = right_Collision.position;
    }

    void Start()
    {
        moveLeft = true;
        canMove = true;
    }

    void Update()
    {
        if (canMove) { 
            if (moveLeft)
            {
                myBody.velocity = new Vector2(-moveSpeed, myBody.velocity.y);
            }
            else
            {
                myBody.velocity = new Vector2(moveSpeed, myBody.velocity.y);
            }
        }
        CheckCollision();
    }

    void CheckCollision()
    {
        RaycastHit2D leftHit = Physics2D.Raycast(left_Collision.position, Vector2.left, 0.1f,playerLayer);
        RaycastHit2D rightHit = Physics2D.Raycast(right_Collision.position, Vector2.right, 0.1f, playerLayer);


        
        Collider2D topHit = Physics2D.OverlapCircle(top_Collision.position, 0.2f, playerLayer);

        if (topHit != null && !isProcessingHit)
        {
            if (topHit.gameObject.tag == MyTags.PLAYER_TAG)
            {

                isProcessingHit = true;

                if (!stunned)
                {
                    topHit.gameObject.GetComponent<Rigidbody2D>().velocity =
                        new Vector2(topHit.gameObject.GetComponent<Rigidbody2D>().velocity.x, 7f);

                    hitCount++;
                    Debug.Log("Hit Count: " + hitCount);
                    if (hitCount >= 2) // Dacă a fost lovit de doua ori
                    {

                        canMove = false;
                        myBody.velocity = new Vector2(0, 0);
                        anim.Play("Stunned");
                        stunned = true;

                        //daca este gandacul atunci moare
                        if (tag == MyTags.BEETLE_TAG)
                        {
                            anim.Play("Stunned");
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
        }

        if (leftHit)
        {
            if (leftHit.collider.gameObject.tag == MyTags.PLAYER_TAG)
            {
                if(!stunned)
                {
                    //print("Damage left");
                    leftHit.collider.gameObject.GetComponent<PlayerDamage>().DealDamage();
                }
                else
                {
                   

                    if (tag!= MyTags.BEETLE_TAG)
                    {
                        myBody.velocity = new Vector2(15f, myBody.velocity.y);
                        StartCoroutine(Dead(3f));
                    }
                }
            }

        }

        if(rightHit)
        {
            if (rightHit.collider.gameObject.tag == MyTags.PLAYER_TAG)
            {
                if (!stunned)
                {
                    print("Damage right");
                    rightHit.collider.gameObject.GetComponent<PlayerDamage>().DealDamage();
                }
                else
                {
                    if (tag != MyTags.BEETLE_TAG)
                    {
                        myBody.velocity = new Vector2(-15f, myBody.velocity.y);
                        StartCoroutine(Dead(3f));
                    }
                }
            }
        }
        //if (Physics2D.Raycast(left_Collision.position, Vector2.down, 0.01f))
        //{
        //    ChangeDirection();
        //}
        //if (Physics2D.Raycast(right_Collision.position, Vector2.down, 0.01f))
        //{
        //    ChangeDirection();
        //}

        //daca nu exista coliziune cu pamantul, schimba directia
        if (!Physics2D.Raycast(down_Collision.position, Vector2.down, 0.1f))
        {
           ChangeDirection();

        }
    }

    void ChangeDirection()
    {
        moveLeft = !moveLeft;
        Vector3 tempScale = transform.localScale;
        if (moveLeft)
        {
            tempScale.x = Mathf.Abs(tempScale.x);

            left_Collision.position = left_Collision_Pos;
            right_Collision.position = right_Collision_Pos;
        }
        else
        {
            tempScale.x = -Mathf.Abs(tempScale.x);

            left_Collision.position = right_Collision_Pos;
            right_Collision.position = left_Collision_Pos;
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
        yield return new WaitForSeconds(0.1f); // Timp de așteptare pentru a evita dublarea loviturilor
        isProcessingHit = false; // Resetăm starea
    }

    //private void OnCollisionEnter2D(Collision2D target)
    //{
    //    if(target.gameObject.tag == "Player")
    //    {
    //        anim.Play("Stunned");
    //    }
    //}

    void OnTriggerEnter2D(Collider2D target)
    {
        if (target.tag == MyTags.BULLET_TAG)
        {
            if(tag == MyTags.BEETLE_TAG)
            {
                anim.Play("Stunned");
                canMove = false;
                myBody.velocity = new Vector2(0, 0);
                StartCoroutine(Dead(0.4f));
            }
            if(tag == MyTags.SNAIL_TAG)
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
} //class



























