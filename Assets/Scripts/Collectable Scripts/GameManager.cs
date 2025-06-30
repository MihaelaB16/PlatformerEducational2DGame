using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Manager central pentru scor, vieti si timpul de joc
public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public int scoreCount = 0;
    public int lifeCount = 3;

    public Text lifeText;
    public Text coinTextScore;
    public Text quizCoinTextScore;
    public Text quizLifeText;

    private float sessionStartTime;
    public float currentSessionTime;

    private float gamePlayTimeStart;
    private float totalGamePlayTime;
    private bool isTrackingTime;

    // Initializare singleton si abonare la evenimente de scene
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    // Initializare UI si pornire cronometru sesiune
    void Start()
    {
        coinTextScore = GameObject.Find("CoinsText").GetComponent<Text>();
        coinTextScore.text = "x" + scoreCount;
        sessionStartTime = Time.time;
    }

    // Actualizare timp sesiune si timp gameplay
    void Update()
    {
        currentSessionTime = Time.time - sessionStartTime;

        if (isTrackingTime)
        {
            float currentTime = Time.time - gamePlayTimeStart + totalGamePlayTime;
        }
    }

    // Adauga puncte la scor si actualizeaza UI
    public void AddScore(int amount)
    {
        scoreCount += amount;

        if (scoreCount <= 0)
        {
            scoreCount = 0;
        }

        if (coinTextScore != null)
            coinTextScore.text = "x" + scoreCount;

        if (quizCoinTextScore != null)
            quizCoinTextScore.text = "x" + scoreCount;
    }

    // Adauga vieti si sincronizeaza cu PlayerDamage
    public void AddLife(int amount)
    {
        if (PlayerDamage.instance != null)
        {
            PlayerDamage.instance.SetLives(PlayerDamage.instance.GetLives() + amount);

            lifeCount = PlayerDamage.instance.GetLives();

            if (quizLifeText != null)
            {
                quizLifeText.text = "x" + lifeCount;
            }
        }
    }

    // Sincronizeaza contorul de vieti cu PlayerDamage
    public void SyncLifeCount()
    {
        if (PlayerDamage.instance != null)
        {
            lifeCount = PlayerDamage.instance.GetLives();
        }
    }

    // Afiseaza toate timpurile pentru debugging
    public void DisplayAllTimes()
    {
        UserManager.instance.DisplayTimeInConsole();
    }

    // Dezabonare de la evenimente la distrugere
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    // Porneste urmarirea timpului cand se incarca GamePlay
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GamePlay")
        {
            StartTimeTracking();
        }
    }

    // Opreste urmarirea timpului cand se descarca GamePlay
    void OnSceneUnloaded(Scene scene)
    {
        if (scene.name == "GamePlay")
        {
            StopTimeTracking();
        }
    }

    // Incepe urmarirea timpului de joc
    public void StartTimeTracking()
    {
        gamePlayTimeStart = Time.time;
        isTrackingTime = true;
    }

    // Opreste urmarirea si salveaza timpul acumulat
    public void StopTimeTracking()
    {
        if (isTrackingTime)
        {
            float currentTime = Time.time;
            float elapsedTime = currentTime - gamePlayTimeStart;
            float newTotal = totalGamePlayTime + elapsedTime;

            totalGamePlayTime = newTotal;
            isTrackingTime = false;

            if (UserManager.instance != null)
            {
                UserManager.instance.SaveCurrentGameplayTime(totalGamePlayTime);
            }
        }
    }

    // Obtine timpul curent de gameplay
    public float GetCurrentGameplayTime()
    {
        float result;

        if (isTrackingTime)
        {
            float currentTime = Time.time;
            float elapsedTime = currentTime - gamePlayTimeStart;
            result = totalGamePlayTime + elapsedTime;
        }
        else
        {
            result = totalGamePlayTime;
        }

        return result;
    }

    // Reseteaza cronometrul de gameplay
    public void ResetGameplayTime()
    {
        totalGamePlayTime = 0;
        if (isTrackingTime)
        {
            gamePlayTimeStart = Time.time;
        }
    }

    // Incarca timpul salvat de gameplay
    public void LoadSavedGameplayTime(float savedTime)
    {
        totalGamePlayTime = savedTime;
    }

    // Incarca meniul principal si salveaza progresul
    public void LoadMainMenu()
    {
        if (UserManager.instance != null)
        {
            UserManager.instance.SavePlayerPosition();
            UserManager.instance.SaveProgressData();
        }
        SceneManager.LoadScene("MainMenu");
    }

    // Actualizeaza referintele UI pentru toate canvas-urile
    public void RefreshUIReferences()
    {
        SyncLifeCount();

        // Actualizeaza referintele pentru HUD principal
        coinTextScore = GameObject.Find("CoinsText")?.GetComponent<Text>();
        if (coinTextScore != null)
        {
            coinTextScore.text = "x" + scoreCount;
        }

        lifeText = GameObject.Find("LifeText")?.GetComponent<Text>();
        if (lifeText != null)
        {
            lifeText.text = "x" + lifeCount;
        }

        // Actualizeaza referintele pentru QuizCanvas
        quizCoinTextScore = GameObject.Find("QuizCoinsText")?.GetComponent<Text>();
        if (quizCoinTextScore != null)
        {
            quizCoinTextScore.text = "x" + scoreCount;
        }

        quizLifeText = GameObject.Find("QuizLifeText")?.GetComponent<Text>();
        if (quizLifeText != null)
        {
            quizLifeText.text = "x" + lifeCount;
        }
    }
}