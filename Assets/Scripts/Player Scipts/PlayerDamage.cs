using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Manager pentru sistemul de viata si damage al jucatorului
public class PlayerDamage : MonoBehaviour
{
    public static PlayerDamage instance;
    private Text lifeText;
    [SerializeField] private int lives = 3;

    private Vector3 initialPosition;

    public string sceneName;
    private bool canDamage = true;

    // Initializare singleton si componente UI
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

    // Seteaza timpul normal de joc
    void Start()
    {
        Time.timeScale = 1f;
    }

    // Aplica damage jucatorului si gestioneaza consecintele
    public void DealDamage()
    {
        if (!canDamage) return;
        if (canDamage)
        {
            lives--;
            if (lives >= 0)
            {
                Time.timeScale = 0f;
                StartCoroutine(ReturnToFlag());
                UpdateLifeUI();
            }
            else
            {
                // Restart complet al jocului
                Time.timeScale = 0f;
                StartCoroutine(RestartGame());
            }
            UserManager.instance.SaveProgressData();
            canDamage = false;

            StartCoroutine(WaitForDamage());
        }
    }

    // Protectie temporara impotriva damage-ului continuu
    IEnumerator WaitForDamage()
    {
        yield return new WaitForSeconds(2f);
        canDamage = true;
    }

    // Intoarce jucatorul la ultimul checkpoint sau pozitia initiala
    IEnumerator ReturnToFlag()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        Time.timeScale = 1f;

        // Muta personajul la ultimul steag sau la pozitia initiala
        transform.position = (FlagController.lastFlagPosition != Vector3.zero) ? FlagController.lastFlagPosition : initialPosition;

        // Actualizeaza fundalul corect
        BackgroundManager backgroundManager = FindObjectOfType<BackgroundManager>();
        if (backgroundManager != null)
        {
            backgroundManager.RefreshBackground();
        }

        UpdateLifeUI();
    }

    // Restarteaza complet scena si reseteaza progresul
    IEnumerator RestartGame()
    {
        yield return new WaitForSecondsRealtime(2f);
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
        SceneManager.LoadScene(sceneName);
    }

    // Detecteaza coliziuni cu obstacole si obiecte speciale
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(MyTags.WATER_TAG) || collision.CompareTag(MyTags.THORNS_TAG))
        {
            DealDamage();
        }
        else if (collision.CompareTag(MyTags.LIFE_TAG))
        {
            GameManager.instance.AddLife(1);

            // Salveaza imediat progresul
            string currentUser = LoginManager.instance?.GetLoggedInUsername();
            if (!string.IsNullOrEmpty(currentUser))
            {
                UserManager.instance.UpdateLives(GetLives());
            }
            UserManager.instance.SaveProgressData();
            Destroy(collision.gameObject);
        }
    }

    // Obtine numarul curent de vieti
    public int GetLives()
    {
        return lives;
    }

    // Seteaza numarul de vieti si actualizeaza UI-ul
    public void SetLives(int value)
    {
        lives = value;
        UpdateLifeUI();
    }

    // Actualizeaza textul UI pentru vieti
    public void UpdateLifeUI()
    {
        lifeText.text = "x" + lives;
    }
}