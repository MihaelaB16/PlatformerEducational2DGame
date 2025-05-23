using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

public class QuizManager : MonoBehaviour
{
    public Image questionImage;
    public GameObject imageQuestion;
    public Button[] answerButtons;
    public GameObject quizCanvas;
    public GameObject continueButton;
    public GameObject btnBack;
    public GameObject backgroundOverlay; // Referință la panelul de umbrire

    private List<Question> questions = new List<Question>();
    private Question currentQuestion;

    public CollectCoinsButton collectCoinsButton; // Referință directă

    public GameObject coliderLeftCheckpoint; // Collider stânga
    public GameObject coliderRightCheckpoint; // Collider dreapta

    private int questionCounter; // Mutat aici ca să fie clar

    public TextAsset questionsFile; // Obiect JSON atribuit în Unity
    public GameObject noCoinsMessage;
    private int rightAnswer = 0;
    private int wrongAnswer = 0;

    public string currentLevel;

    // Adăugat pentru urmărirea întrebărilor încercate
    private HashSet<int> attemptedQuestionIndices = new HashSet<int>();
    private int currentQuestionId = 0;

    // Noi variabile pentru sistemul de bonusuri consecutive
    private int consecutiveCorrectAnswers = 0;
    private int[] bonusPoints = { 5, 10, 15, 20, 30, 50 }; // Punctele pentru fiecare răspuns consecutiv

    void Start()
    {
        questionCounter = 6; // Resetăm contorul când începe quiz-ul
        consecutiveCorrectAnswers = 0;
        LoadQuestionsFromJSON();
        ShuffleQuestions();
        DisplayNextQuestion();

        if (continueButton != null)
        {
            continueButton.SetActive(false); // Ascundem butonul Continue la start
        }

        // Resetăm HashSet-ul la început
        attemptedQuestionIndices.Clear();
    }

    void LoadQuestionsFromJSON()
    {
        if (questionsFile != null)
        {
            string json = questionsFile.text;
            QuestionData[] loadedQuestions = JsonUtility.FromJson<QuestionDataWrapper>(json).items;

            foreach (var data in loadedQuestions)
            {
                // Încarcă imaginea întrebării din Resources
                Sprite questionImage = Resources.Load<Sprite>(data.question);

                // Adaugă întrebarea în listă
                questions.Add(new Question(questionImage, data.answers, data.correctAnswer));
            }
        }
        else
        {
            Debug.LogError(" Obiectul TextAsset pentru întrebări nu a fost setat în Inspector!");
        }
    }

    [System.Serializable]
    private class QuestionData
    {
        public string question; // Numele fișierului imaginii
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

    void DisplayNextQuestion()
    {
        Debug.Log("Apel DisplayNextQuestion");
        Debug.Log("questionCounter: " + questionCounter);
        Debug.Log("Număr întrebări rămase: " + questions.Count);

        if (questionCounter > 0 && questions.Count > 0)
        {
            int randomIndex = Random.Range(0, questions.Count);
            currentQuestion = questions[randomIndex];
            questions.RemoveAt(randomIndex);

            // Generează un ID unic pentru întrebarea curentă
            currentQuestionId = currentQuestion.GetHashCode();

            // Afișează imaginea întrebării
            questionImage.sprite = currentQuestion.question;

            for (int i = 0; i < answerButtons.Length; i++)
            {
                // Ensure all previous listeners are removed
                answerButtons[i].onClick.RemoveAllListeners();

                if (i < currentQuestion.answers.Length)
                {
                    answerButtons[i].GetComponentInChildren<Text>().text = currentQuestion.answers[i];
                    int index = i;

                    // Add a single listener with a flag to prevent double execution
                    answerButtons[i].onClick.AddListener(delegate {
                        // Disable all buttons immediately to prevent multiple clicks
                        foreach (Button btn in answerButtons)
                        {
                            btn.interactable = false;
                        }

                        CheckAnswer(index);

                        // Re-enable buttons after a short delay
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

    // Add this new method to re-enable buttons
    private void ReenableButtons()
    {
        foreach (Button btn in answerButtons)
        {
            btn.interactable = true;
        }
    }

    private bool isProcessingAnswer = false;  // Add this variable at the class level

    public void CheckAnswer(int index)
    {
        // Prevent multiple calls while processing an answer
        if (isProcessingAnswer)
        {
            Debug.Log("Already processing an answer, ignoring this call");
            return;
        }

        isProcessingAnswer = true;
        Debug.Log("CheckAnswer apelat! Index: " + index);

        string currentUser = LoginManager.instance?.GetLoggedInUsername();
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (index == currentQuestion.correctAnswer)
        {
            // Incrementează contorul de răspunsuri consecutive corecte
            consecutiveCorrectAnswers++;

            // Calculează punctele bonus în funcție de numărul de răspunsuri consecutive
            int bonusToAdd = 0;
            if (consecutiveCorrectAnswers <= bonusPoints.Length)
            {
                bonusToAdd = bonusPoints[consecutiveCorrectAnswers - 1];
            }
            else
            {
                // După 6 răspunsuri consecutive, continuă cu 50 de puncte
                bonusToAdd = 50;
            }

            Debug.Log($"Răspuns corect #{consecutiveCorrectAnswers}! Bonus: {bonusToAdd} puncte");

            // Verifică dacă este al 6-lea răspuns consecutiv (sau multiplu de 6)
            bool shouldAddLife = (consecutiveCorrectAnswers % 6 == 0);

            if (shouldAddLife)
            {
                Debug.Log("🎉 6 răspunsuri consecutive corecte! Primești o viață bonus!");
                GameManager.instance.AddLife(1);
            }

            rightAnswer++;
            Debug.Log($"Răspuns corect! rightAnswer: {rightAnswer}, wrongAnswer: {wrongAnswer}");

            // Verifică dacă este prima încercare pentru această întrebare
            if (!attemptedQuestionIndices.Contains(currentQuestionId))
            {
                // Este prima încercare și răspunsul este corect!
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
                            Debug.Log($"Răspuns corect din prima încercare! firstAttemptRightAnswer incrementat la: {levelStats.firstAttemptRightAnswer}");
                        }

                        // Salvează progresul
                        UserManager.instance.SaveProgress(currentUser, userProgress);
                    }
                }
            }

            // Adaugă punctele bonus
            GameManager.instance.AddScore(bonusToAdd);

            if (collectCoinsButton != null)
            {
                collectCoinsButton.CheckScore();
            }

            CheckGameOver();

            if (questions.Count > 0 && questionCounter > 0)
            {
                DisplayNextQuestion();
            }
            else if (questionCounter == 0)
            {
                Debug.Log("Ultima întrebare a fost răspunsă corect! Afișez butonul Continue.");
                Invoke("ShowContinueButton", 0.5f);
            }
        }
        else
        {
            // RĂSPUNS GREȘIT - Resetează contorul de răspunsuri consecutive
            Debug.Log($"Răspuns greșit! Resetez contorul de răspunsuri consecutive de la {consecutiveCorrectAnswers} la 0");
            consecutiveCorrectAnswers = 0;

            // Marchează această întrebare ca fiind încercată
            attemptedQuestionIndices.Add(currentQuestionId);

            wrongAnswer++;
            Debug.Log($"Răspuns greșit! rightAnswer: {rightAnswer}, wrongAnswer: {wrongAnswer}");
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
            // Nu mai trecem la altă întrebare până nu răspunde corect
            Debug.Log("Răspuns greșit. Reîncearcă aceeași întrebare.");
        }

        // Actualizarea valorilor în LevelStats pentru user progress
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

                // Update global sums
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

        // Reset the flag after a short delay
        Invoke("ResetProcessingFlag", 0.5f);
    }

    private void ResetProcessingFlag()
    {
        isProcessingAnswer = false;
    }

    // Activează butonul Continue la final
    void ShowContinueButton()
    {
        if (questionCounter <= 0) // Doar când toate întrebările sunt finalizate
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

            Debug.Log("Quiz finalizat! Butonul 'Continue' este acum vizibil.");
        }
        else
        {
            Debug.Log("Butonul Continue NU trebuie să apară încă. Întrebări rămase: " + questionCounter);
            if (continueButton != null)
            {
                continueButton.SetActive(false);
            }
        }
    }

    void CheckGameOver()
    {
        Debug.Log("CheckGameOver() apelată! Scor curent: " + GameManager.instance.scoreCount);

        if (GameManager.instance.scoreCount <= 0)
        {
            Debug.Log("Scor 0 detectat! Dezactivez butoanele și afișez 'Button_back'.");

            GameManager.instance.scoreCount = 0;
            GameManager.instance.coinTextScore.text = "x0";

            if (imageQuestion != null)
            {
                imageQuestion.SetActive(false);
            }
            // Dezactivare butoane răspuns
            foreach (Button btn in answerButtons)
            {
                btn.gameObject.SetActive(false);
            }

            // Dezactivare butonul "ContinueGame"
            if (continueButton != null)
            {
                continueButton.SetActive(false);
            }
            if (noCoinsMessage != null)
            {
                noCoinsMessage.SetActive(true);
                Debug.Log("NoCoinsMessage activat direct prin referință!");
            }
            // Dezactivare butoane manual (dacă sunt create separat și nu în `answerButtons`)
            GameObject.Find("Button_question1")?.SetActive(false);
            GameObject.Find("Button_question2")?.SetActive(false);
            GameObject.Find("Button_question3")?.SetActive(false);
            GameObject.Find("Button_question4")?.SetActive(false);
            GameObject.Find("Button_ContinueGame")?.SetActive(false);
            //  GameObject.Find("BackGroundQuiz")?.SetActive(false);

            // Activare buton "Button_back"
            if (btnBack != null)
            {
                btnBack.SetActive(true);
            }

            // Asigură-te că UI-ul este actualizat corect
            Invoke("EnsureUIUpdated", 0.1f);
        }
    }

    void EnsureUIUpdated()
    {
        GameManager.instance.coinTextScore.text = "x0";
        Debug.Log("UI actualizat forțat: " + GameManager.instance.coinTextScore.text);
    }

    public void ContinueGame()
    {
        quizCanvas.SetActive(false);
        continueButton.SetActive(true);

        if (backgroundOverlay != null)
        {
            backgroundOverlay.SetActive(false); // Dezactivează fundalul umbrit
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
        Debug.Log("🎉 Quiz finalizat! Jocul continuă.");
    }

    public void OnBackButtonPressed()
    {
        Debug.Log(" Dezactivez ColiderLeftCheckpoint și închid quiz-ul!");
        coliderLeftCheckpoint.SetActive(false); // Dezactivează coliderul stânga
        quizCanvas.SetActive(false); // Închide quiz-ul
    }
}

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

[System.Serializable]
public class LevelStats
{
    public int rightAnswer = 0;
    public int wrongAnswer = 0;
    public int firstAttemptRightAnswer = 0;
}