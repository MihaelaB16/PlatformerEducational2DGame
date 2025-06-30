using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using System.Collections;

// Manager pentru sistemul de quiz-uri educationale
public class QuizManager : MonoBehaviour
{
    public Image questionImage;
    public GameObject imageQuestion;
    public Button[] answerButtons;
    public GameObject quizCanvas;
    public GameObject continueButton;
    public GameObject btnBack;
    public GameObject backgroundOverlay;

    private List<Question> questions = new List<Question>();
    private Question currentQuestion;

    public CollectCoinsButton collectCoinsButton;

    public GameObject coliderLeftCheckpoint;
    public GameObject coliderRightCheckpoint;

    private int questionCounter;

    public TextAsset questionsFile;
    public GameObject noCoinsMessage;
    private int rightAnswer = 0;
    private int wrongAnswer = 0;

    public string currentLevel;

    // Urmarirea intrebarilor incercate pentru prima oara
    private HashSet<int> attemptedQuestionIndices = new HashSet<int>();
    private int currentQuestionId = 0;

    // Sistemul de bonusuri pentru raspunsuri consecutive
    private int consecutiveCorrectAnswers = 0;
    private int[] bonusPoints = { 5, 10, 15, 20, 30, 50 };

    [Header("Bonus Display")]
    public Text bonusText;
    public Text bonusLifeText;
    public float bonusDisplayTime = 0.1f;

    // Initializare quiz cu 6 intrebari si incarcare din JSON
    void Start()
    {
        questionCounter = 6;
        consecutiveCorrectAnswers = 0;
        LoadQuestionsFromJSON();
        ShuffleQuestions();

        HideBonusMessage();
        HideLifeBonusMessage();

        DisplayNextQuestion();

        if (continueButton != null)
        {
            continueButton.SetActive(false);
        }

        attemptedQuestionIndices.Clear();
    }

    // Incarca intrebarile din fisierul JSON
    void LoadQuestionsFromJSON()
    {
        if (questionsFile != null)
        {
            string json = questionsFile.text;
            QuestionData[] loadedQuestions = JsonUtility.FromJson<QuestionDataWrapper>(json).items;

            foreach (var data in loadedQuestions)
            {
                Sprite questionImage = Resources.Load<Sprite>(data.question);
                questions.Add(new Question(questionImage, data.answers, data.correctAnswer));
            }
        }
    }

    // Structuri pentru deserializarea JSON
    [System.Serializable]
    private class QuestionData
    {
        public string question;
        public string[] answers;
        public int correctAnswer;
    }

    [System.Serializable]
    private class QuestionDataWrapper
    {
        public QuestionData[] items;
    }

    public static class JsonHelper
    {
        public static T[] FromJson<T>(string json)
        {
            string wrappedJson = "{\"items\":" + json + "}";
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(wrappedJson);
            return wrapper.items;
        }

        [System.Serializable]
        private class Wrapper<T>
        {
            public T[] items;
        }
    }

    // Amesteca intrebarile folosind algoritmul Fisher-Yates
    void ShuffleQuestions()
    {
        for (int i = 0; i < questions.Count; i++)
        {
            Question temp = questions[i];
            int randomIndex = Random.Range(i, questions.Count);
            questions[i] = questions[randomIndex];
            questions[randomIndex] = temp;
        }
    }

    // Afiseaza urmatoarea intrebare si configureaza butoanele
    void DisplayNextQuestion()
    {
        if (questionCounter > 0 && questions.Count > 0)
        {
            int randomIndex = Random.Range(0, questions.Count);
            currentQuestion = questions[randomIndex];
            questions.RemoveAt(randomIndex);

            currentQuestionId = currentQuestion.GetHashCode();
            questionImage.sprite = currentQuestion.question;

            for (int i = 0; i < answerButtons.Length; i++)
            {
                answerButtons[i].onClick.RemoveAllListeners();

                if (i < currentQuestion.answers.Length)
                {
                    answerButtons[i].GetComponentInChildren<Text>().text = currentQuestion.answers[i];
                    int index = i;

                    // Previne click-urile multiple
                    answerButtons[i].onClick.AddListener(delegate {
                        foreach (Button btn in answerButtons)
                        {
                            btn.interactable = false;
                        }

                        CheckAnswer(index);
                        Invoke("ReenableButtons", 0.5f);
                    });

                    answerButtons[i].gameObject.SetActive(true);
                }
                else
                {
                    answerButtons[i].gameObject.SetActive(false);
                }
            }

            questionCounter--;
        }
        else
        {
            ShowContinueButton();
        }
    }

    // Reactiveaza butoanele dupa verificarea raspunsului
    private void ReenableButtons()
    {
        foreach (Button btn in answerButtons)
        {
            btn.interactable = true;
        }
    }

    private bool isProcessingAnswer = false;

    // Verifica raspunsul si actualizeaza scorul si statisticile
    public void CheckAnswer(int index)
    {
        if (isProcessingAnswer)
        {
            return;
        }

        isProcessingAnswer = true;

        string currentUser = LoginManager.instance?.GetLoggedInUsername();
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (index == currentQuestion.correctAnswer)
        {
            // Raspuns corect - incrementeaza bonusurile consecutive
            consecutiveCorrectAnswers++;

            int bonusToAdd = 0;
            if (consecutiveCorrectAnswers <= bonusPoints.Length)
            {
                bonusToAdd = bonusPoints[consecutiveCorrectAnswers - 1];
            }
            else
            {
                bonusToAdd = 50;
            }

            // Viata bonus la al 6-lea raspuns consecutiv
            bool shouldAddLife = (consecutiveCorrectAnswers % 6 == 0);

            string bonusMessage = $"+{bonusToAdd} monezi";
            if (shouldAddLife)
            {
                GameManager.instance.AddLife(1);
                ShowLifeBonusMessage("+1 viata");
            }

            ShowBonusMessage(bonusMessage);
            rightAnswer++;

            // Verifica daca e prima incercare pentru aceasta intrebare
            if (!attemptedQuestionIndices.Contains(currentQuestionId))
            {
                if (!string.IsNullOrEmpty(currentUser))
                {
                    var userProgress = UserManager.instance.GetCurrentUserProgress();
                    if (userProgress != null && userProgress.Scenes.ContainsKey(currentScene))
                    {
                        var sceneData = userProgress.Scenes[currentScene];
                        LevelStats levelStats = null;

                        if (currentLevel == "Level1") levelStats = sceneData.Level1;
                        else if (currentLevel == "Level2") levelStats = sceneData.Level2;

                        if (levelStats != null)
                        {
                            levelStats.firstAttemptRightAnswer++;
                        }

                        UserManager.instance.SaveProgress(currentUser, userProgress);
                    }
                }
            }

            GameManager.instance.AddScore(bonusToAdd);

            if (collectCoinsButton != null)
            {
                collectCoinsButton.CheckScore();
            }

            CheckGameOver();

            // Trece la urmatoarea intrebare
            if (questions.Count > 0 && questionCounter > 0)
            {
                DisplayNextQuestion();
            }
            else if (questionCounter == 0)
            {
                ShowContinueButton();
            }
        }
        else
        {
            // Raspuns gresit - reseteaza bonusurile consecutive
            consecutiveCorrectAnswers = 0;
            ShowBonusMessage("-5 monezi");
            attemptedQuestionIndices.Add(currentQuestionId);

            wrongAnswer++;
            GameManager.instance.AddScore(-5);

            if (GameManager.instance.scoreCount < 0)
            {
                GameManager.instance.scoreCount = 0;
            }

            if (collectCoinsButton != null)
            {
                collectCoinsButton.ShowCollectButton();
            }

            CheckGameOver();
        }

        // Actualizeaza statisticile in progresul utilizatorului
        if (!string.IsNullOrEmpty(currentUser))
        {
            var userProgress = UserManager.instance.GetCurrentUserProgress();
            if (userProgress != null && userProgress.Scenes.ContainsKey(currentScene))
            {
                var sceneData = userProgress.Scenes[currentScene];
                LevelStats levelStats = null;

                if (currentLevel == "Level1") levelStats = sceneData.Level1;
                else if (currentLevel == "Level2") levelStats = sceneData.Level2;

                if (levelStats != null)
                {
                    levelStats.rightAnswer = rightAnswer;
                    levelStats.wrongAnswer = wrongAnswer;
                }

                // Actualizeaza sumele globale
                userProgress.rightAnswer = 0;
                userProgress.wrongAnswer = 0;
                foreach (var scene in userProgress.Scenes.Values)
                {
                    userProgress.rightAnswer += scene.Level1.rightAnswer + scene.Level2.rightAnswer;
                    userProgress.wrongAnswer += scene.Level1.wrongAnswer + scene.Level2.wrongAnswer;
                }

                UserManager.instance.SaveProgress(currentUser, userProgress);
            }
        }

        Invoke("ResetProcessingFlag", 0.5f);
    }
    // Afiseaza mesajul de bonus cu culoare automata
    void ShowBonusMessage(string message, Color? textColor = null)
    {
        if (bonusText != null)
        {
            bonusText.text = message;

            if (textColor.HasValue)
            {
                bonusText.color = textColor.Value;
            }
            else
            {
                // Verde pentru puncte pozitive, rosu pentru negative
                bonusText.color = message.Contains("-") ? Color.red : Color.green;
            }

            bonusText.gameObject.SetActive(true);
            StartCoroutine(HideBonusAfterDelay());
        }
    }

    // Afiseaza mesajul de bonus pentru vieti (galben/auriu)
    void ShowLifeBonusMessage(string message, Color? textColor = null)
    {
        if (bonusLifeText != null)
        {
            bonusLifeText.text = message;
            bonusLifeText.color = textColor ?? Color.yellow;
            bonusLifeText.gameObject.SetActive(true);
            StartCoroutine(HideLifeBonusAfterDelay());
        }
    }

    // Ascunde bonusul dupa timpul setat
    IEnumerator HideBonusAfterDelay()
    {
        yield return new WaitForSeconds(bonusDisplayTime);
        HideBonusMessage();
    }

    IEnumerator HideLifeBonusAfterDelay()
    {
        yield return new WaitForSeconds(bonusDisplayTime);
        HideLifeBonusMessage();
    }

    // Ascunde mesajul de bonus pentru monede
    void HideBonusMessage()
    {
        if (bonusText != null)
        {
            bonusText.gameObject.SetActive(false);
        }
    }

    // Ascunde mesajul de bonus pentru vieti
    void HideLifeBonusMessage()
    {
        if (bonusLifeText != null)
        {
            bonusLifeText.gameObject.SetActive(false);
        }
    }

    // Trece la urmatoarea intrebare dupa afisarea bonusului
    IEnumerator ProceedToNextQuestionAfterBonus()
    {
        yield return new WaitForSeconds(bonusDisplayTime);
        HideBonusMessage();

        if (questions.Count > 0 && questionCounter > 0)
        {
            DisplayNextQuestion();
        }
        else if (questionCounter == 0)
        {
            ShowContinueButton();
        }
    }

    // Reseteaza flag-ul de procesare a raspunsului
    private void ResetProcessingFlag()
    {
        isProcessingAnswer = false;
    }

    // Activeaza butonul Continue cand toate intrebarile sunt finalizate
    void ShowContinueButton()
    {
        if (questionCounter <= 0)
        {
            foreach (Button btn in answerButtons)
            {
                btn.gameObject.SetActive(false);
            }

            if (questionImage != null)
            {
                questionImage.gameObject.SetActive(false);
            }

            if (continueButton != null)
            {
                continueButton.SetActive(true);
            }
        }
        else
        {
            if (continueButton != null)
            {
                continueButton.SetActive(false);
            }
        }
    }

    // Verifica daca jucatorul a ramas fara monede
    void CheckGameOver()
    {
        if (GameManager.instance.scoreCount <= 0)
        {
            GameManager.instance.scoreCount = 0;
            GameManager.instance.coinTextScore.text = "x0";

            if (imageQuestion != null)
            {
                imageQuestion.SetActive(false);
            }

            // Dezactiveaza butoanele de raspuns
            foreach (Button btn in answerButtons)
            {
                btn.gameObject.SetActive(false);
            }

            if (continueButton != null)
            {
                continueButton.SetActive(false);
            }

            if (noCoinsMessage != null)
            {
                noCoinsMessage.SetActive(true);
            }

            // Dezactiveaza butoanele specifice
            GameObject.Find("Button_question1")?.SetActive(false);
            GameObject.Find("Button_question2")?.SetActive(false);
            GameObject.Find("Button_question3")?.SetActive(false);
            GameObject.Find("Button_question4")?.SetActive(false);
            GameObject.Find("Button_ContinueGame")?.SetActive(false);

            if (btnBack != null)
            {
                btnBack.SetActive(true);
            }

            Invoke("EnsureUIUpdated", 0.1f);
        }
    }

    // Asigura actualizarea corecta a UI-ului
    void EnsureUIUpdated()
    {
        GameManager.instance.coinTextScore.text = "x0";
    }

    // Continua jocul dupa finalizarea quiz-ului
    public void ContinueGame()
    {
        quizCanvas.SetActive(false);
        continueButton.SetActive(true);

        if (backgroundOverlay != null)
        {
            backgroundOverlay.SetActive(false);
        }

        if (coliderLeftCheckpoint != null)
        {
            coliderLeftCheckpoint.SetActive(true);
        }
        if (coliderRightCheckpoint != null)
        {
            coliderRightCheckpoint.SetActive(false);
        }

        Time.timeScale = 1f;
    }

    // Intoarce jucatorul la colectarea monedelor
    public void OnBackButtonPressed()
    {
        coliderLeftCheckpoint.SetActive(false);
        quizCanvas.SetActive(false);
    }
}

// Clasa pentru o intrebare cu imagine si raspunsuri
[System.Serializable]
public class Question
{
    public Sprite question;
    public string[] answers;
    public int correctAnswer;

    public Question(Sprite q, string[] a, int correct)
    {
        question = q;
        answers = a;
        correctAnswer = correct;
    }
}

// Statistici pentru un nivel de quiz
[System.Serializable]
public class LevelStats
{
    public int rightAnswer = 0;
    public int wrongAnswer = 0;
    public int firstAttemptRightAnswer = 0;
}