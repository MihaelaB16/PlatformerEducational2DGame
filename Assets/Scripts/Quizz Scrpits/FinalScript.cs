using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEditor.Timeline.TimelinePlaybackControls;

public class FinalScript : MonoBehaviour
{
    public GameObject quizCanvas;
    public Text coinsText;
    public Text rightAnswersText;
    public Text wrongAnswersText;

    public void ShowFinalStats()
    {
        string username = LoginManager.instance?.GetLoggedInUsername();
        if (string.IsNullOrEmpty(username))
        {
            Debug.LogError("No logged-in user found!");
            return;
        }

        // Get the current scene name
        string currentScene = SceneManager.GetActiveScene().name;

        // Get the user's progress from UserManager
        UserProgress progress = UserManager.instance.GetCurrentUserProgress();
        if (progress == null || progress.Scenes == null || !progress.Scenes.ContainsKey(currentScene))
        {
            Debug.LogError($"Failed to get current user progress for scene {currentScene}!");
            return;
        }

        // Get the current scene's data
        SceneData sceneData = progress.Scenes[currentScene];

        // Display the values from the current scene's progress
        coinsText.text = "" + sceneData.Coins;

        // Sumează valorile din Level1 și Level2
        rightAnswersText.text = "x" + (sceneData.Level1.rightAnswer + sceneData.Level2.rightAnswer);
        wrongAnswersText.text = "x" + (sceneData.Level1.wrongAnswer + sceneData.Level2.wrongAnswer);

        Debug.Log($"Displaying stats for user '{username}' in scene '{currentScene}': " +
                  $"Coins={sceneData.Coins}, " +
                  $"Correct={sceneData.Level1.rightAnswer + sceneData.Level2.rightAnswer}, " +
                  $"Wrong={sceneData.Level1.wrongAnswer + sceneData.Level2.wrongAnswer}");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) 
        {
            StartCoroutine(HandleCheckpoint());
        }
    }

    IEnumerator HandleCheckpoint()
    {
        Time.timeScale = 0f; // Oprește jocul
        yield return new WaitForSecondsRealtime(0.5f);
        if (UserManager.instance != null)
        {
            UserManager.instance.SavePlayerPosition();
            UserManager.instance.SaveProgressData();
        }
        else
        {
            Debug.LogError("UserManager instance is null. Cannot save player position or progress data.");
        }
        quizCanvas.SetActive(true);
        ShowFinalStats();
    }
    public void LoadMainMenu()
    {
        if (UserManager.instance != null)
        {
            UserManager.instance.SavePlayerPosition();
            UserManager.instance.SaveProgressData();
        }
        else
        {
            Debug.LogError("UserManager instance is null. Cannot save player position or progress data.");
        }
        Debug.Log("Se încarcă scena MainMenu...");
        quizCanvas.SetActive(true);
        SceneManager.LoadScene("MainMenu");
    }
}
