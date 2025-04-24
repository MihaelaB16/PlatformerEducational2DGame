using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance; // Singleton pentru acces global
    public int scoreCount = 0; // Scorul global
    public Text coinTextScore; // Referința la textul UI pentru scor
    private float sessionStartTime;
    public float currentSessionTime;

    private float gamePlayTimeStart;
    private float totalGamePlayTime;
    private bool isTrackingTime;
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

    void Start()
    {
        // Găsește și inițializează CoinsText
        coinTextScore = GameObject.Find("CoinsText").GetComponent<Text>();
        coinTextScore.text = "x" + scoreCount;
        sessionStartTime = Time.time;
    }

    public void AddScore(int amount)
    {
        Debug.Log("🔄 AddScore() apelată! Modific scorul cu: " + amount);

        scoreCount += amount;

        if (scoreCount <= 0)
        {
            Debug.Log("⚠️ Scorul a ajuns la 0 sau mai mic. Resetare la 0.");
            scoreCount = 0;
        }

        Debug.Log("✅ Scor nou: " + scoreCount);
        coinTextScore.text = "x" + scoreCount;
    }
    void Update()
    {
        currentSessionTime = Time.time - sessionStartTime;

        // You can uncomment this if you want to continuously display the current session time
         Debug.Log("Current session time: " + currentSessionTime.ToString("F2") + " seconds");
        if (isTrackingTime)
        {
            float currentTime = Time.time - gamePlayTimeStart + totalGamePlayTime;
            // Uncomment to display current time in console
             Debug.Log($"Current gameplay time: {currentTime.ToString("F2")} seconds");
        }
    }
    public void DisplayAllTimes()
    {
        UserManager.instance.DisplayTimeInConsole();
        Debug.Log($"Current session time: {currentSessionTime.ToString("F2")} seconds");
    }

    void OnDestroy()
    {
        // Unregister from scene events
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GamePlay")
        {
            StartTimeTracking();
        }
    }

    void OnSceneUnloaded(Scene scene)
    {
        if (scene.name == "GamePlay")
        {
            StopTimeTracking();
        }
    }
    public void StartTimeTracking()
    {
        gamePlayTimeStart = Time.time;
        isTrackingTime = true;
        Debug.Log("Started tracking gameplay time");
    }

    public void StopTimeTracking()
    {
        if (isTrackingTime)
        {
            totalGamePlayTime += (Time.time - gamePlayTimeStart);
            isTrackingTime = false;
            Debug.Log($"Stopped tracking gameplay time. Total: {totalGamePlayTime.ToString("F2")} seconds");

            // Save the current accumulated time
            if (UserManager.instance != null)
            {
                UserManager.instance.SaveCurrentGameplayTime(totalGamePlayTime);
            }
        }
    }
    public float GetCurrentGameplayTime()
    {
        if (isTrackingTime)
        {
            return totalGamePlayTime + (Time.time - gamePlayTimeStart);
        }
        return totalGamePlayTime;
    }

    // Reset the gameplay time counter (e.g., when switching users)
    public void ResetGameplayTime()
    {
        totalGamePlayTime = 0;
        if (isTrackingTime)
        {
            gamePlayTimeStart = Time.time;
        }
    }

    public void LoadSavedGameplayTime(float savedTime)
    {
        totalGamePlayTime = savedTime;
        Debug.Log($"Loaded saved gameplay time: {totalGamePlayTime.ToString("F2")} seconds");
    }

}
