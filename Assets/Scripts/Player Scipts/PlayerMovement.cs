using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Controller pentru miscarea si animatiile jucatorului
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

    private BackgroundManager backgroundManager;

    // Initializare componente si gasire BackgroundManager
    void Awake()
    {
        myBody = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        backgroundManager = FindObjectOfType<BackgroundManager>();
    }

    // Incarca progresul salvat si restaureaza starea jucatorului
    private void Start()
    {
        string currentUser = LoginManager.instance?.GetLoggedInUsername();
        if (string.IsNullOrEmpty(currentUser))
        {
            return;
        }

        string currentScene = SceneManager.GetActiveScene().name;
        if (string.IsNullOrEmpty(currentScene))
        {
            return;
        }

        // Incarca progresul complet
        var progress = UserManager.instance.LoadPlayerProgress(currentUser, currentScene);

        // Seteaza pozitia
        transform.position = progress.Position;

        // Seteaza monedele in GameManager
        if (GameManager.instance != null)
        {
            GameManager.instance.scoreCount = progress.Coins;
            if (GameManager.instance.coinTextScore != null)
                GameManager.instance.coinTextScore.text = "x" + progress.Coins;
        }

        // Seteaza vietile in PlayerDamage
        var playerDamage = GetComponent<PlayerDamage>();
        if (playerDamage != null)
        {
            playerDamage.SetLives(progress.Lives);
        }

        // Seteaza fundalul corect bazat pe pozitia jucatorului
        if (backgroundManager != null)
        {
            Invoke("RefreshBackground", 0.3f);
        }
    }

    // Actualizeaza fundalul dupa restaurarea pozitiei
    private void RefreshBackground()
    {
        if (backgroundManager != null)
        {
            backgroundManager.RefreshBackground();
        }
    }

    // Verifica statusul de pe pamant si gestioneaza saritura
    void Update()
    {
        float move = Input.GetAxis("Horizontal");
        transform.position += new Vector3(move * Time.deltaTime * 1f, 0, 0);

        CheckIfGrounded();
        PlayerJump();
    }

    // Gestioneaza miscarea laterala cu fizica
    private void FixedUpdate()
    {
        PlayerWalk();
    }

    // Miscare laterala si actualizare animatii
    void PlayerWalk()
    {
        float h = Input.GetAxisRaw("Horizontal");

        if (h > 0)
        {
            myBody.velocity = new Vector2(speed, myBody.velocity.y);
            ChangeDirection(1);
        }
        else if (h < 0)
        {
            myBody.velocity = new Vector2(-speed, myBody.velocity.y);
            ChangeDirection(-1);
        }
        else
        {
            myBody.velocity = new Vector2(0f, myBody.velocity.y);
        }

        anim.SetInteger("Speed", Mathf.Abs((int)myBody.velocity.x));
    }

    // Schimba directia personajului prin scalare
    void ChangeDirection(int direction)
    {
        Vector3 tempScale = transform.localScale;
        tempScale.x = direction;
        transform.localScale = tempScale;
    }

    // Placeholder pentru coliziuni fizice
    private void OnCollisionEnter2D(Collision2D target)
    {
    }

    // Placeholder pentru trigger-uri
    void OnTriggerEnter2D(Collider2D target)
    {
    }

    // Verifica daca jucatorul este pe pamant folosind raycast
    void CheckIfGrounded()
    {
        isGrounded = Physics2D.Raycast(groundCheckPosition.position, Vector2.down, 0.1f, groundLayer);

        if (isGrounded)
        {
            if (jumped)
            {
                jumped = false;
                anim.SetBool("Jump", false);
            }
        }
    }

    // Gestioneaza saritura cu spatiu si animatii
    void PlayerJump()
    {
        if (isGrounded)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                jumped = true;
                myBody.velocity = new Vector2(myBody.velocity.x, jumpPower);
                anim.SetBool("Jump", true);
            }
        }
    }
}