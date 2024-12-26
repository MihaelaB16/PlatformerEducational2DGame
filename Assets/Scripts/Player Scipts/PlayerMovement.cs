using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    public float speed = 5f;
    private Rigidbody2D myBody;
    private Animator anim;

    public Transform groundCheckPosition;
    public LayerMask groundLayer;

    private bool isGrounded;
    private bool jumped;

    private float jumpPower = 12f;

    void Awake()  //prima apelata dupa run, apoi e start
    {

        myBody = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
         //if (Physics2D.Raycast(groundCheckPosition.position, Vector2.down, 0.5f, groundLayer)) { 
                                                                                    
         //   print("Collision with ground ");
         //}

        CheckIfGrounded();
        PlayerJump();
    }
    private void FixedUpdate()
    {
        PlayerWalk();
    }

    void PlayerWalk()
    {
        float h = Input.GetAxisRaw("Horizontal");

        if(h > 0)
        {
            myBody.velocity = new Vector2(speed, myBody.velocity.y);
            ChangeDirection(1); //merge la dreapta
        }
        else if (h < 0)
        {
            myBody.velocity = new Vector2(-speed, myBody.velocity.y);
            ChangeDirection(-1); //merge la stanga
        }
        else
        {
            myBody.velocity = new Vector2(0f, myBody.velocity.y);
        }

        anim.SetInteger("Speed", Mathf.Abs((int)myBody.velocity.x));
        
    }

    void ChangeDirection(int direction)
    {
        Vector3 tempScale = transform.localScale;
        tempScale.x = direction;
        transform.localScale = tempScale;
    }

    private void OnCollisionEnter2D(Collision2D target)
    {
        //if(target.gameObject.tag == "Ground")
        //{
        //    print("Collision with ground detected!");
        //}

    }
    void OnTriggerEnter2D(Collider2D target)
    {
       // if (target.tag == "Ground")
        //{
         //   print("Collision with tag");
        //}
    }

    void CheckIfGrounded()
    {
        isGrounded = Physics2D.Raycast(groundCheckPosition.position, Vector2.down, 0.1f, groundLayer);

        if(isGrounded)
        {
            if(jumped)
            {
                jumped = false;
                anim.SetBool("Jump", false);
            }
        }
    }

    void PlayerJump()
    {
        if (isGrounded)
        {
            if(Input.GetKey(KeyCode.Space))
            {
                jumped = true;
                myBody.velocity = new Vector2(myBody.velocity.x, jumpPower);

                anim.SetBool("Jump", true);
            }
        }
    }


}
