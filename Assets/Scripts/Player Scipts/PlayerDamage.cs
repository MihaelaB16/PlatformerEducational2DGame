using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerDamage : MonoBehaviour
{

    public static PlayerDamage instance;
    private Text lifeText;
    [SerializeField] private int lives = 3;

    private Vector3 initialPosition;

    public string sceneName;
    private bool canDamage = true;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        lifeText = GameObject.Find("LifeText").GetComponent<Text>();

        initialPosition = transform.position;
        

    }
    void Start()
    {
        Time.timeScale = 1f;  //cand se incepe jocul, timpul va fi normal
    }

    // Update is called once per frame
    public void DealDamage()
    {
        if (!canDamage) return;
        if (canDamage)
        {

            lives--;
            if (lives > 0)
            {
                
                Time.timeScale = 0f;
                StartCoroutine(ReturnToFlag());
                UpdateLifeUI();
            }
            else
            {
                //restart game
                // Time.timeScale = 0f;   //face ca totul sa stea pe loc (jocul sa fie oprit) totul va fi inghetat
                Time.timeScale = 0f;
                StartCoroutine(RestartGame());
            }
            UserManager.instance.SaveProgressData();
            canDamage = false;

            StartCoroutine(WaitForDamage());
        }
       
    }
    //void Update()
    //{
    //    float move = Input.GetAxis("Horizontal");
    //    transform.position += new Vector3(move * Time.deltaTime * 5f, 0, 0);
    //}

    IEnumerator WaitForDamage()
    {
        yield return new WaitForSeconds(2f);
        canDamage = true;
    }

    IEnumerator ReturnToFlag()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        Time.timeScale = 1f;

        // Mutăm personajul la ultimul steag sau la poziția inițială
        transform.position = (FlagController.lastFlagPosition != Vector3.zero) ? FlagController.lastFlagPosition : initialPosition;

        // Restaurăm fundalul curent sau setăm fundalul inițial
        GameObject background = GameObject.Find("Background");
        GameObject background2 = GameObject.Find("Background2");

        if (FlagController.currentBackground != null)
        {
            Debug.Log("Restaurăm fundalul curent: " + FlagController.currentBackground.name);

            if (FlagController.currentBackground == background)
            {
                background.SetActive(true);
                background2.SetActive(false);
            }
            else if (FlagController.currentBackground == background2)
            {
                background2.SetActive(true);
                background.SetActive(false);
            }
        }
        else
        {
            // Setăm fundalul inițial dacă currentBackground nu este setat
            if (background != null)
            {
                background.SetActive(true);
                background2.SetActive(false);
                FlagController.currentBackground = background; // Setăm currentBackground la fundalul inițial
                Debug.Log("Restaurăm fundalul inițial: " + background.name);
            }
        }
        UpdateLifeUI();
    }

    IEnumerator RestartGame()
    {
        yield return new WaitForSecondsRealtime(2f); // Așteaptă 2 secunde în timp real
        UserManager.instance.ResetProgressForCurrentScene(
            new Vector3(-10.0f, -3.0f, 0.0f), 
            0,                               
            3,                                 
            0.0f                             
        );

        if (GameManager.instance != null)
        {
            GameManager.instance.ResetGameplayTime();
        }
        SceneManager.LoadScene(sceneName); // Reîncarcă scena specificată
    }


    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(MyTags.WATER_TAG) || collision.CompareTag(MyTags.THORNS_TAG))
        {
            DealDamage();
        }
        else if (collision.CompareTag(MyTags.LIFE_TAG))
        {
            GameManager.instance.AddLife(1);

            // Dacă vrei să salvezi imediat progresul:
            string currentUser = LoginManager.instance?.GetLoggedInUsername();
            if (!string.IsNullOrEmpty(currentUser))
            {
                UserManager.instance.UpdateLives(GetLives()); // Salvează progresul local și în JSON
            }
            UserManager.instance.SaveProgressData();
            Destroy(collision.gameObject);
        }
    }
    public int GetLives()
    {
        return lives;
    }
    public void SetLives(int value)
    {
        lives = value;
        UpdateLifeUI();
    }

    public void UpdateLifeUI()
    {
        
        lifeText.text = "x" + lives;
    }
}//class





















