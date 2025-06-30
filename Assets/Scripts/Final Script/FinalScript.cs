using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Script pentru afisarea rezultatelor finale la sfarsitul jocului
public class FinalScript : MonoBehaviour
{
    [Header("UI References")]
    public GameObject quizCanvas;
    public Text coinsText;
    public Text rightAnswersText;
    public Text wrongAnswersText;

    [Header("Score System")]
    public Text finalScoreText;
    public Image starImage;

    [Header("Score Calculator")]
    public ScoreCalculator scoreCalculator;

    [Header("Details Canvas Manager")]
    public DetailsCanvasManager detailsCanvasManager;

    void Start()
    {
      
    }

    // Calculeaza si afiseaza statisticile finale pentru utilizatorul curent
    public void ShowFinalStats()
    {
        string username = LoginManager.instance?.GetLoggedInUsername();
        if (string.IsNullOrEmpty(username))
        {
            return;
        }

        string currentScene = SceneManager.GetActiveScene().name;

        UserProgress progress = UserManager.instance.GetCurrentUserProgress();
        if (progress == null || progress.Scenes == null || !progress.Scenes.ContainsKey(currentScene))
        {
            return;
        }

        SceneData sceneData = progress.Scenes[currentScene];

        // Calculeaza scorul si stelele
        SceneScoreData scoreData = ScoreCalculator.CalculateSceneScore(sceneData);

        // Actualizeaza datele in JSON
        sceneData.FinalScore = scoreData.Score;
        sceneData.Stars = scoreData.Stars;
        UserManager.instance.SaveProgressData();

        // Afiseaza statisticile existente
        coinsText.text = "" + sceneData.Coins;

        // Afiseaza numarul total de raspunsuri corecte din prima incercare
        int firstAttemptCorrect = sceneData.Level1.firstAttemptRightAnswer + sceneData.Level2.firstAttemptRightAnswer;
        rightAnswersText.text = "x" + firstAttemptCorrect;

        // Afiseaza numarul total de raspunsuri gresite
        wrongAnswersText.text = "x" + (sceneData.Level1.wrongAnswer + sceneData.Level2.wrongAnswer);

        // Gaseste si afiseaza scorul final
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
            // Cautare globala ca backup
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
        }

        // Gaseste si afiseaza stelele
        Image starImageComponent = starImage;
        if (starImageComponent == null && quizCanvas != null)
        {
            Image[] images = quizCanvas.GetComponentsInChildren<Image>(true);
            starImageComponent = System.Array.Find(images, img =>
                img.name.Contains("Star") || img.name.Contains("Display"));
        }

        if (starImageComponent == null)
        {
            // Cautare globala ca backup
            Image[] allImages = FindObjectsOfType<Image>(true);
            starImageComponent = System.Array.Find(allImages, img =>
                img.name.Contains("Star") || img.name.Contains("Display"));
        }

        // Gaseste ScoreCalculator
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
            }
        }

        // Actualizeaza Details Canvas Manager
        if (detailsCanvasManager != null)
        {
            detailsCanvasManager.RefreshDetails();
        }
        else
        {
            // Incearca sa gaseasca DetailsCanvasManager in scena
            DetailsCanvasManager foundManager = FindObjectOfType<DetailsCanvasManager>();
            if (foundManager != null)
            {
                foundManager.RefreshDetails();
            }
        }
    }

    // Detecteaza cand jucatorul atinge checkpoint-ul final
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(HandleCheckpoint());
        }
    }

    // Gestioneaza checkpoint-ul final cu pauza si afisarea rezultatelor
    IEnumerator HandleCheckpoint()
    {
        Time.timeScale = 0f; // Opreste jocul
        yield return new WaitForSecondsRealtime(0.5f);

        // Salveaza progresul inainte de a afisa statisticile finale
        if (UserManager.instance != null)
        {
            UserManager.instance.SavePlayerPosition();
            UserManager.instance.SaveProgressData();
        }

        quizCanvas.SetActive(true);
        ShowFinalStats(); // Calculeaza si afiseaza statisticile finale
    }

    // Incarca meniul principal si salveaza progresul
    public void LoadMainMenu()
    {
        if (UserManager.instance != null)
        {
            UserManager.instance.SavePlayerPosition();
            UserManager.instance.SaveProgressData();
        }

        quizCanvas.SetActive(false);
        SceneManager.LoadScene("MainMenu");
    }

    // Metoda pentru debugging - afiseaza toate scorurile calculate
    [ContextMenu("Debug All Scores")]
    public void DebugAllScores()
    {
        string username = LoginManager.instance?.GetLoggedInUsername();
        if (string.IsNullOrEmpty(username)) return;

        UserProgress progress = UserManager.instance.GetCurrentUserProgress();
        if (progress?.Scenes == null) return;

        foreach (var kvp in progress.Scenes)
        {
            string sceneName = kvp.Key;
            SceneData sceneData = kvp.Value;

            if (sceneName == "GamePlay" || sceneName == "GamePlayRomana")
            {
                SceneScoreData scoreData = ScoreCalculator.CalculateSceneScore(sceneData);
                // Informatii de debugging pentru console
            }
        }
    }
}