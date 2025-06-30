using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BonusBlock : MonoBehaviour
{
    [Header("Collision Settings")]
    public Transform bottomCollision;
    public LayerMask playerLayer;

    private Animator anim;
    private AudioSource audioManager;
    private Vector3 moveDirection = Vector3.up;
    private Vector3 originPosition;
    private Vector3 animPosition;
    private bool startAnim;
    private bool canAnimate = true;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        audioManager = GetComponent<AudioSource>();
    }

    void Start()
    {
        originPosition = transform.position;
        animPosition = transform.position;
        animPosition.y += 0.15f;
    }

    void Update()
    {
        CheckForCollision();
        AnimateUpDown();
    }

    // Verifica daca jucatorul loveste blocul din partea de jos
    void CheckForCollision()
    {
        if (canAnimate)
        {
            RaycastHit2D hit2D = Physics2D.Raycast(bottomCollision.position, Vector2.down, 0.1f, playerLayer);

            if (hit2D && hit2D.collider.gameObject.tag == MyTags.PLAYER_TAG)
            {
                audioManager.Play();
                GameManager.instance.AddScore(1);
                anim.Play("BlockIdle");
                startAnim = true;
                canAnimate = false;
            }
        }
    }

    // Animeaza blocul sus-jos cand este lovit
    void AnimateUpDown()
    {
        if (startAnim)
        {
            transform.Translate(moveDirection * Time.smoothDeltaTime);

            if (transform.position.y >= animPosition.y)
            {
                moveDirection = Vector3.down;
            }
            else if (transform.position.y <= originPosition.y)
            {
                startAnim = false;
            }
        }
    }
}