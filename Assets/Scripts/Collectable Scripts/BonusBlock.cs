using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class BonusBlock : MonoBehaviour
{
    public Transform bottomCollision;
    private Animator anim;

    public LayerMask playerLayer;

    private Vector3 moveDirection = Vector3.up;
    private Vector3 originPosition;
    private Vector3 animPosition;
    private bool startAnim;

    private bool canAnimate=true;

    private Text coinTextScore;
    private AudioSource audioManager;
    private int scoreCount;
    private void Awake()
    {
        anim = GetComponent<Animator>();
        audioManager = GetComponent<AudioSource>();
    }

    void Start()
    {

        coinTextScore = GameObject.Find("CoinsText").GetComponent<Text>();

        int.TryParse(coinTextScore.text.Replace("x", "").Trim(), out scoreCount);

        originPosition = transform.position;
        animPosition = transform.position;
        animPosition.y += 0.15f;
    }

    // Update is called once per frame
    void Update()
    {
        CheckForCollision();
        AnimateUpDown();
    }
    void CheckForCollision()
    {
        if (canAnimate)
        {
            RaycastHit2D hit2D = Physics2D.Raycast(bottomCollision.position, Vector2.down, 0.1f, playerLayer);

            if (hit2D)
            {
                if (hit2D.collider.gameObject.tag == MyTags.PLAYER_TAG)
                {
                    audioManager.Play();

                    // Actualizăm scorul prin GameManager
                    GameManager.instance.AddScore(1);

                    anim.Play("BlockIdle");
                    startAnim = true;
                    canAnimate = false;
                }
            }
        }
        
    }
    void AnimateUpDown() {
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
}//class




















