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
    public Text scorText2;
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
        PopulateSceneDetails();
    }

    // Incarca si afiseaza detaliile scenei pentru utilizatorul curent
    public void PopulateSceneDetails()
    {
        string username = LoginManager.instance?.GetLoggedInUsername();
        if (string.IsNullOrEmpty(username))
        {
            SetDefaultValues();
            return;
        }

        string currentScene = SceneManager.GetActiveScene().name;
        UserProgress progress = UserManager.instance.GetCurrentUserProgress();

        if (progress == null || progress.Scenes == null || !progress.Scenes.ContainsKey(currentScene))
        {
            SetDefaultValues();
            return;
        }

        SceneData sceneData = progress.Scenes[currentScene];
        PopulateUI(username, sceneData);
    }

    // Populeaza interfata cu datele din JSON
    private void PopulateUI(string username, SceneData sceneData)
    {
        if (userText != null)
            userText.text = username;

        if (timeText != null)
        {
            float timeInSeconds = sceneData.Time;
            int minutes = Mathf.FloorToInt(timeInSeconds / 60);
            int seconds = Mathf.FloorToInt(timeInSeconds % 60);
            timeText.text = $"{minutes:00}:{seconds:00}";
        }

        if (coinsText != null)
            coinsText.text = sceneData.Coins.ToString();

        if (lifeText != null)
            lifeText.text = sceneData.Lives.ToString();

        if (scorText != null)
            scorText.text = sceneData.FinalScore.ToString("F0");

        if (scorText2 != null)
            scorText2.text = sceneData.FinalScore.ToString("F0");

        if (starImage != null && scoreCalculator != null)
        {
            Sprite starSprite = scoreCalculator.GetStarSprite(sceneData.Stars);
            if (starSprite != null)
            {
                starImage.sprite = starSprite;
                starImage.gameObject.SetActive(true);
            }
        }

        if (level1RightAnswer != null)
            level1RightAnswer.text = sceneData.Level1.rightAnswer.ToString();

        if (level1RightFirstAnswer != null)
            level1RightFirstAnswer.text = sceneData.Level1.firstAttemptRightAnswer.ToString();

        if (level1WrongAnswer != null)
            level1WrongAnswer.text = sceneData.Level1.wrongAnswer.ToString();

        if (level2RightAnswer != null)
            level2RightAnswer.text = sceneData.Level2.rightAnswer.ToString();

        if (level2RightFirstAnswer != null)
            level2RightFirstAnswer.text = sceneData.Level2.firstAttemptRightAnswer.ToString();

        if (level2WrongAnswer != null)
            level2WrongAnswer.text = sceneData.Level2.wrongAnswer.ToString();
    }

    // Seteaza valori implicite cand nu exista date
    private void SetDefaultValues()
    {
        if (userText != null) userText.text = "Guest";
        if (timeText != null) timeText.text = "00:00";
        if (coinsText != null) coinsText.text = "0";
        if (lifeText != null) lifeText.text = "3";
        if (scorText != null) scorText.text = "0";
        if (scorText2 != null) scorText2.text = "0";

        if (starImage != null && scoreCalculator != null)
        {
            Sprite oneStar = scoreCalculator.GetStarSprite(1);
            if (oneStar != null)
            {
                starImage.sprite = oneStar;
                starImage.gameObject.SetActive(true);
            }
        }

        if (level1RightAnswer != null) level1RightAnswer.text = "0";
        if (level1RightFirstAnswer != null) level1RightFirstAnswer.text = "0";
        if (level1WrongAnswer != null) level1WrongAnswer.text = "0";

        if (level2RightAnswer != null) level2RightAnswer.text = "0";
        if (level2RightFirstAnswer != null) level2RightFirstAnswer.text = "0";
        if (level2WrongAnswer != null) level2WrongAnswer.text = "0";
    }

    public void RefreshDetails()
    {
        PopulateSceneDetails();
    }

    public void OnDataChanged()
    {
        Invoke("RefreshDetails", 0.1f);
    }
}