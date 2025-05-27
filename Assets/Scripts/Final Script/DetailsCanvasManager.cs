using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DetailsCanvasManager : MonoBehaviour
{
    [Header("UI References")]
    public Text userText;
    public Text timeText;
    public Text coinsText;
    public Text lifeText;
    public Text scorText;
    public Text scorText2;        // Nou câmp pentru al doilea text de scor
    public Image starImage;

    [Header("Level 1 Stats")]
    public Text level1RightAnswer;
    public Text level1RightFirstAnswer;
    public Text level1WrongAnswer;

    [Header("Level 2 Stats")]
    public Text level2RightAnswer;
    public Text level2RightFirstAnswer;
    public Text level2WrongAnswer;

    [Header("Score Calculator")]
    public ScoreCalculator scoreCalculator;

    private void Start()
    {
        // Populeaz? detaliile la start
        PopulateSceneDetails();
    }

    public void PopulateSceneDetails()
    {
        string username = LoginManager.instance?.GetLoggedInUsername();
        if (string.IsNullOrEmpty(username))
        {
            Debug.LogError("No logged-in user found!");
            SetDefaultValues();
            return;
        }

        string currentScene = SceneManager.GetActiveScene().name;

        // Ob?ine progresul utilizatorului
        UserProgress progress = UserManager.instance.GetCurrentUserProgress();
        if (progress == null || progress.Scenes == null || !progress.Scenes.ContainsKey(currentScene))
        {
            Debug.LogError($"No progress found for user '{username}' in scene '{currentScene}'!");
            SetDefaultValues();
            return;
        }

        SceneData sceneData = progress.Scenes[currentScene];

        // Populeaz? UI-ul cu datele din JSON
        PopulateUI(username, sceneData);
    }

    private void PopulateUI(string username, SceneData sceneData)
    {
        // User info
        if (userText != null)
            userText.text = username;

        // Time - converte?te din secunde în format citibil
        if (timeText != null)
        {
            float timeInSeconds = sceneData.Time;
            int minutes = Mathf.FloorToInt(timeInSeconds / 60);
            int seconds = Mathf.FloorToInt(timeInSeconds % 60);
            timeText.text = $"{minutes:00}:{seconds:00}";
        }

        // Coins
        if (coinsText != null)
            coinsText.text = sceneData.Coins.ToString();

        // Lives
        if (lifeText != null)
            lifeText.text = sceneData.Lives.ToString();

        // Score - folose?te scorul calculat ?i salvat
        if (scorText != null)
            scorText.text = sceneData.FinalScore.ToString("F0");

        // Score 2 - acela?i scor ca ?i ScorText
        if (scorText2 != null)
            scorText2.text = sceneData.FinalScore.ToString("F0");

        // Stars - afi?eaz? imaginea corespunz?toare
        if (starImage != null && scoreCalculator != null)
        {
            Sprite starSprite = scoreCalculator.GetStarSprite(sceneData.Stars);
            if (starSprite != null)
            {
                starImage.sprite = starSprite;
                starImage.gameObject.SetActive(true);
            }
        }

        // Level 1 statistics
        if (level1RightAnswer != null)
            level1RightAnswer.text = sceneData.Level1.rightAnswer.ToString();

        if (level1RightFirstAnswer != null)
            level1RightFirstAnswer.text = sceneData.Level1.firstAttemptRightAnswer.ToString();

        if (level1WrongAnswer != null)
            level1WrongAnswer.text = sceneData.Level1.wrongAnswer.ToString();

        // Level 2 statistics
        if (level2RightAnswer != null)
            level2RightAnswer.text = sceneData.Level2.rightAnswer.ToString();

        if (level2RightFirstAnswer != null)
            level2RightFirstAnswer.text = sceneData.Level2.firstAttemptRightAnswer.ToString();

        if (level2WrongAnswer != null)
            level2WrongAnswer.text = sceneData.Level2.wrongAnswer.ToString();

        Debug.Log($"? Details Canvas populated for user '{username}':");
        Debug.Log($"   Time: {sceneData.Time:F1}s, Coins: {sceneData.Coins}, Lives: {sceneData.Lives}");
        Debug.Log($"   Score: {sceneData.FinalScore:F0}, Stars: {sceneData.Stars}");
        Debug.Log($"   Level1 - Right: {sceneData.Level1.rightAnswer}, First: {sceneData.Level1.firstAttemptRightAnswer}, Wrong: {sceneData.Level1.wrongAnswer}");
        Debug.Log($"   Level2 - Right: {sceneData.Level2.rightAnswer}, First: {sceneData.Level2.firstAttemptRightAnswer}, Wrong: {sceneData.Level2.wrongAnswer}");
    }

    private void SetDefaultValues()
    {
        // Seteaz? valori implicite dac? nu exist? date
        if (userText != null) userText.text = "Guest";
        if (timeText != null) timeText.text = "00:00";
        if (coinsText != null) coinsText.text = "0";
        if (lifeText != null) lifeText.text = "3";
        if (scorText != null) scorText.text = "0";
        if (scorText2 != null) scorText2.text = "0";  // Seteaz? ?i ScorText2 la 0

        if (starImage != null && scoreCalculator != null)
        {
            Sprite oneStar = scoreCalculator.GetStarSprite(1);
            if (oneStar != null)
            {
                starImage.sprite = oneStar;
                starImage.gameObject.SetActive(true);
            }
        }

        // Seteaz? statistici Level 1 ?i 2 la 0
        if (level1RightAnswer != null) level1RightAnswer.text = "0";
        if (level1RightFirstAnswer != null) level1RightFirstAnswer.text = "0";
        if (level1WrongAnswer != null) level1WrongAnswer.text = "0";

        if (level2RightAnswer != null) level2RightAnswer.text = "0";
        if (level2RightFirstAnswer != null) level2RightFirstAnswer.text = "0";
        if (level2WrongAnswer != null) level2WrongAnswer.text = "0";

        Debug.LogWarning("?? Set default values for Details Canvas");
    }

    // Metod? public? pentru refresh manual
    public void RefreshDetails()
    {
        PopulateSceneDetails();
    }

    // Metod? pentru debugging - afi?eaz? toate valorile în consol?
    [ContextMenu("Debug Scene Details")]
    public void DebugSceneDetails()
    {
        string username = LoginManager.instance?.GetLoggedInUsername();
        if (string.IsNullOrEmpty(username)) return;

        string currentScene = SceneManager.GetActiveScene().name;
        UserProgress progress = UserManager.instance.GetCurrentUserProgress();

        if (progress?.Scenes?.ContainsKey(currentScene) == true)
        {
            SceneData sceneData = progress.Scenes[currentScene];

            Debug.Log($"=== SCENE DETAILS DEBUG ===");
            Debug.Log($"User: {username}");
            Debug.Log($"Scene: {currentScene}");
            Debug.Log($"Time: {sceneData.Time:F2}s ({sceneData.Time / 60:F1} minutes)");
            Debug.Log($"Coins: {sceneData.Coins}");
            Debug.Log($"Lives: {sceneData.Lives}");
            Debug.Log($"Final Score: {sceneData.FinalScore:F1}");
            Debug.Log($"Stars: {sceneData.Stars}");
            Debug.Log($"Level 1 - Right: {sceneData.Level1.rightAnswer}, First Attempt: {sceneData.Level1.firstAttemptRightAnswer}, Wrong: {sceneData.Level1.wrongAnswer}");
            Debug.Log($"Level 2 - Right: {sceneData.Level2.rightAnswer}, First Attempt: {sceneData.Level2.firstAttemptRightAnswer}, Wrong: {sceneData.Level2.wrongAnswer}");
            Debug.Log($"========================");
        }
        else
        {
            Debug.LogError($"No data found for user '{username}' in scene '{currentScene}'");
        }
    }

    // Metod? pentru a actualiza datele când se schimb? ceva în joc
    public void OnDataChanged()
    {
        // Poate fi apelat? din alte scripturi când se actualizeaz? datele
        Invoke("RefreshDetails", 0.1f); // Mic delay pentru a fi sigur c? datele sunt salvate
    }
}