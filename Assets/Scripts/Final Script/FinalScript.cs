using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FinalScript : MonoBehaviour
{
    [Header("UI References")]
    public GameObject quizCanvas;
    public Text coinsText;
    public Text rightAnswersText;
    public Text wrongAnswersText;

    [Header("Score System")]
    public Text finalScoreText;        // Text pentru afișarea scorului final
    public Image starImage;            // Image component pentru afișarea stelelor

    [Header("Score Calculator")]
    public ScoreCalculator scoreCalculator; // Referință către ScoreCalculator

    [Header("Details Canvas Manager")]
    public DetailsCanvasManager detailsCanvasManager; // Referință către DetailsCanvasManager

    void Start()
    {
        // Debug pentru a verifica referințele la început
        Debug.Log("=== FINAL SCRIPT REFERENCES CHECK ===");
        Debug.Log($"finalScoreText: {(finalScoreText != null ? "✅ SET" : "❌ NULL")}");
        Debug.Log($"starImage: {(starImage != null ? "✅ SET" : "❌ NULL")}");
        Debug.Log($"scoreCalculator: {(scoreCalculator != null ? "✅ SET" : "❌ NULL")}");
        Debug.Log($"detailsCanvasManager: {(detailsCanvasManager != null ? "✅ SET" : "❌ NULL")}");

        if (scoreCalculator != null && scoreCalculator.starSprites != null)
        {
            Debug.Log($"Star Sprites count: {scoreCalculator.starSprites.Length}");
            for (int i = 0; i < scoreCalculator.starSprites.Length; i++)
            {
                Debug.Log($"  Star Sprite {i + 1}: {(scoreCalculator.starSprites[i] != null ? scoreCalculator.starSprites[i].name : "NULL")}");
            }
        }
        Debug.Log("=====================================");
    }

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

        // **CALCULEAZĂ SCORUL ȘI STELELE**
        SceneScoreData scoreData = ScoreCalculator.CalculateSceneScore(sceneData);

        // **ACTUALIZEAZĂ DATELE ÎN JSON**
        sceneData.FinalScore = scoreData.Score;
        sceneData.Stars = scoreData.Stars;
        UserManager.instance.SaveProgressData(); // Salvează scorul în JSON

        // **AFIȘEAZĂ STATISTICILE EXISTENTE**
        coinsText.text = "" + sceneData.Coins;

        // Afișează numărul total de răspunsuri corecte din prima încercare
        int firstAttemptCorrect = sceneData.Level1.firstAttemptRightAnswer + sceneData.Level2.firstAttemptRightAnswer;
        rightAnswersText.text = "x" + firstAttemptCorrect;

        // Afișează numărul total de răspunsuri greșite
        wrongAnswersText.text = "x" + (sceneData.Level1.wrongAnswer + sceneData.Level2.wrongAnswer);

        // **GĂSEȘTE ȘI AFIȘEAZĂ SCORUL FINAL** - Caută în Canvas activ
        Text finalScoreTextComponent = finalScoreText;
        if (finalScoreTextComponent == null && quizCanvas != null)
        {
            Text[] texts = quizCanvas.GetComponentsInChildren<Text>(true);
            foreach (Text t in texts)
            {
                if (t.name.Contains("FinalScore") || t.name.Contains("Score"))
                {
                    finalScoreTextComponent = t;
                    break;
                }
            }
        }

        if (finalScoreTextComponent == null)
        {
            // Căutare globală ca backup
            Text[] allTexts = FindObjectsOfType<Text>(true);
            foreach (Text t in allTexts)
            {
                if (t.name.Contains("FinalScore") || t.name.Contains("Score"))
                {
                    finalScoreTextComponent = t;
                    break;
                }
            }
        }

        if (finalScoreTextComponent != null)
        {
            finalScoreTextComponent.gameObject.SetActive(true);
            finalScoreTextComponent.text = $"Scor Final: {scoreData.Score:F0}";
            Debug.Log($"✅ Final Score Text updated: {finalScoreTextComponent.text}");
        }
        else
        {
            Debug.LogError("❌ Could not find any Text component for Final Score!");
        }

        // **GĂSEȘTE ȘI AFIȘEAZĂ STELELE** - Caută în Canvas activ
        Image starImageComponent = starImage;
        if (starImageComponent == null && quizCanvas != null)
        {
            Image[] images = quizCanvas.GetComponentsInChildren<Image>(true);
            starImageComponent = System.Array.Find(images, img =>
                img.name.Contains("Star") || img.name.Contains("Display"));
        }

        if (starImageComponent == null)
        {
            // Căutare globală ca backup
            Image[] allImages = FindObjectsOfType<Image>(true);
            starImageComponent = System.Array.Find(allImages, img =>
                img.name.Contains("Star") || img.name.Contains("Display"));
        }

        // Găsește ScoreCalculator
        ScoreCalculator calculator = scoreCalculator;
        if (calculator == null)
        {
            calculator = FindObjectOfType<ScoreCalculator>();
        }

        if (starImageComponent != null && calculator != null)
        {
            starImageComponent.gameObject.SetActive(true);
            Sprite starSprite = calculator.GetStarSprite(scoreData.Stars);
            if (starSprite != null)
            {
                starImageComponent.sprite = starSprite;
                Debug.Log($"✅ Star Image updated: {scoreData.Stars} stars sprite applied");
            }
            else
            {
                Debug.LogError($"❌ Could not get star sprite for {scoreData.Stars} stars!");
            }
        }
        else
        {
            if (starImageComponent == null) Debug.LogError("❌ Could not find any Image component for Stars!");
            if (calculator == null) Debug.LogError("❌ Could not find ScoreCalculator component!");
        }

        // **ACTUALIZEAZĂ DETAILS CANVAS MANAGER**
        if (detailsCanvasManager != null)
        {
            detailsCanvasManager.RefreshDetails();
            Debug.Log("✅ Details Canvas Manager refreshed!");
        }
        else
        {
            // Încearcă să găsească DetailsCanvasManager în scenă
            DetailsCanvasManager foundManager = FindObjectOfType<DetailsCanvasManager>();
            if (foundManager != null)
            {
                foundManager.RefreshDetails();
                Debug.Log("✅ Found and refreshed Details Canvas Manager!");
            }
            else
            {
                Debug.LogWarning("⚠️ Details Canvas Manager not found!");
            }
        }

        Debug.Log($"Final Stats for '{username}' in '{currentScene}':");
        Debug.Log($"  Coins: {sceneData.Coins}");
        Debug.Log($"  Time: {sceneData.Time:F1}s");
        Debug.Log($"  First Attempt Correct: {firstAttemptCorrect}");
        Debug.Log($"  Wrong Answers: {sceneData.Level1.wrongAnswer + sceneData.Level2.wrongAnswer}");
        Debug.Log($"  Final Score: {scoreData.Score:F1}");
        Debug.Log($"  Stars: {scoreData.Stars}");
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

        // Salvează progresul înainte de a afișa statisticile finale
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
        ShowFinalStats(); // Calculează și afișează statisticile finale
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
        quizCanvas.SetActive(false);
        SceneManager.LoadScene("MainMenu");
    }

    // Metodă pentru debug - afișează toate scorurile calculate
    [ContextMenu("Debug All Scores")]
    public void DebugAllScores()
    {
        string username = LoginManager.instance?.GetLoggedInUsername();
        if (string.IsNullOrEmpty(username)) return;

        UserProgress progress = UserManager.instance.GetCurrentUserProgress();
        if (progress?.Scenes == null) return;

        Debug.Log($"=== SCORE DEBUG FOR USER: {username} ===");

        foreach (var kvp in progress.Scenes)
        {
            string sceneName = kvp.Key;
            SceneData sceneData = kvp.Value;

            if (sceneName == "GamePlay" || sceneName == "GamePlayRomana")
            {
                SceneScoreData scoreData = ScoreCalculator.CalculateSceneScore(sceneData);

                Debug.Log($"Scene: {sceneName}");
                Debug.Log($"  Coins: {sceneData.Coins}");
                Debug.Log($"  Time: {sceneData.Time:F1}s");
                Debug.Log($"  First Attempt Correct: {sceneData.Level1.firstAttemptRightAnswer + sceneData.Level2.firstAttemptRightAnswer}");
                Debug.Log($"  Wrong Answers: {sceneData.Level1.wrongAnswer + sceneData.Level2.wrongAnswer}");
                Debug.Log($"  Calculated Score: {scoreData.Score:F1}");
                Debug.Log($"  Stars: {scoreData.Stars}");
                Debug.Log($"  Saved Score: {sceneData.FinalScore:F1}");
                Debug.Log($"  Saved Stars: {sceneData.Stars}");
            }
        }

        Debug.Log("=== END SCORE DEBUG ===");
    }
}