using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

public class QuizManager : MonoBehaviour
{
    public Text questionText;
    public Image questionImage;
    public Button[] answerButtons;
    public GameObject quizCanvas;
    public GameObject continueButton;
    public GameObject backgroundOverlay; // Referință la panelul de umbrire

    private List<Question> questions = new List<Question>();
    private Question currentQuestion;

    public CollectCoinsButton collectCoinsButton; // Referință directă


    public GameObject coliderLeftCheckpoint; // Collider stânga
    public GameObject coliderRightCheckpoint; // Collider dreapta

    private int questionCounter; // Mutat aici ca să fie clar

    public TextAsset questionsFile; // Obiect JSON atribuit în Unity


    void Start()
    {

        questionCounter = 6; // Resetăm contorul când începe quiz-ul
        LoadQuestionsFromJSON();
        ShuffleQuestions();
        DisplayNextQuestion();

        if (continueButton != null)
        {
            continueButton.SetActive(false); // Ascundem butonul Continue la start
        }
    }


    //void LoadQuestions()
    //{
    //    questions.Add(new Question("Care este capitala Franței?", new string[] { "Paris", "Londra", "Madrid", "Berlin" }, 0));
    //    questions.Add(new Question("Cât face 5 + 3?", new string[] { "6", "8", "7", "9" }, 1));
    //    questions.Add(new Question("Cel mai mare ocean este?", new string[] { "Pacific", "Atlantic", "Indian", "Arctic" }, 0));
    //    Debug.Log("Întrebări încărcate: " + questions.Count);  // Debugging pentru a verifica numărul de întrebări
    //}
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

        if (index == currentQuestion.correctAnswer)
        {
            GameManager.instance.AddScore(5);
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
                Debug.Log(" Ultima întrebare a fost răspunsă corect! Afișez butonul Continue.");
                Invoke("ShowContinueButton", 0.5f);
            }
        }
        else
        {
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

            if (questionText != null)
            {
                questionText.gameObject.SetActive(false);
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

            // Dezactivare butoane manual (dacă sunt create separat și nu în `answerButtons`)
            GameObject.Find("Button_question1")?.SetActive(false);
            GameObject.Find("Button_question2")?.SetActive(false);
            GameObject.Find("Button_question3")?.SetActive(false);
            GameObject.Find("Button_question4")?.SetActive(false);
            GameObject.Find("Button_ContinueGame")?.SetActive(false);
            //  GameObject.Find("BackGroundQuiz")?.SetActive(false);

            // Activare buton "Button_back"
            GameObject buttonBack = GameObject.Find("Button_back");
            if (buttonBack != null)
            {
                Debug.Log("Butonul 'Button_back' a fost găsit și activat!");
                buttonBack.SetActive(true);
            }
            else
            {
                Debug.LogError("Eroare: 'Button_back' nu a fost găsit!");
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
