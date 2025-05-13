using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    private void Start()
    {
        string currentUser = LoginManager.instance?.GetLoggedInUsername();
        Debug.Log($"Current logged-in user: {currentUser}");
        if (string.IsNullOrEmpty(currentUser))
        {
            Debug.LogError("No logged-in user found. Cannot restore player progress.");
            return;
        }

        string currentScene = SceneManager.GetActiveScene().name;
        if (string.IsNullOrEmpty(currentScene))
        {
            Debug.LogError("Current scene name is null or empty. Cannot restore player progress.");
            return;
        }

        // Încarcă progresul complet
        var progress = UserManager.instance.LoadPlayerProgress(currentUser, currentScene);

        // Setează poziția
        transform.position = progress.Position;

        // Setează monedele în GameManager
        if (GameManager.instance != null)
        {
            GameManager.instance.scoreCount = progress.Coins;
            if (GameManager.instance.coinTextScore != null)
                GameManager.instance.coinTextScore.text = "x" + progress.Coins;
        }

        // Setează viețile în PlayerDamage
        var playerDamage = GetComponent<PlayerDamage>();
        if (playerDamage != null)
        {
            playerDamage.SetLives(progress.Lives);
        }
    }


    // Update is called once per frame
    void Update()
    {
        //if (Physics2D.Raycast(groundCheckPosition.position, Vector2.down, 0.5f, groundLayer)) { 

        //   print("Collision with ground ");
        //}
        Debug.Log($"Player position in Update: {transform.position}");
        float move = Input.GetAxis("Horizontal");
        transform.position += new Vector3(move * Time.deltaTime * 1f, 0, 0);

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


} //class











