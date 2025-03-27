using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

public class QuizManager : MonoBehaviour
{
    public Text questionText;
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
    private static bool quizCompleted = false; // Dacă quiz-ul a fost finalizat

    public TextAsset questionsFile; // Obiect JSON atribuit în Unity


    void Start()
    {

          if (quizCompleted) 
    {
        Debug.Log("🛑 Quiz-ul a fost deja completat! Afișez doar butonul Continue.");
        ShowContinueButton();
        return;
    }

        questionCounter = 10; // Resetăm contorul când începe quiz-ul
        LoadQuestionsFromJSON();
        ShuffleQuestions();
        DisplayNextQuestion();

        if (continueButton != null)
        {
            continueButton.SetActive(false); // Ascundem butonul Continue la start
        }
    }


    void LoadQuestions()
    {
        questions.Add(new Question("Care este capitala Franței?", new string[] { "Paris", "Londra", "Madrid", "Berlin" }, 0));
        questions.Add(new Question("Cât face 5 + 3?", new string[] { "6", "8", "7", "9" }, 1));
        questions.Add(new Question("Cel mai mare ocean este?", new string[] { "Pacific", "Atlantic", "Indian", "Arctic" }, 0));
        Debug.Log("Întrebări încărcate: " + questions.Count);  // Debugging pentru a verifica numărul de întrebări
    }
    void LoadQuestionsFromJSON()
    {
        if (questionsFile != null)
        {
            // Citește conținutul JSON direct din TextAsset
            string json = questionsFile.text;

            // Deserializează conținutul fișierului JSON într-o listă de întrebări
            Question[] loadedQuestions = JsonHelper.FromJson<Question>(json);

            // Adaugă întrebările încărcate în lista de întrebări
            questions = new List<Question>(loadedQuestions);
        }
        else
        {
            Debug.LogError("⚠️ Obiectul TextAsset pentru întrebări nu a fost setat în Inspector!");
        }
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
        if (questionCounter > 0 && questions.Count > 0) // Evităm scăderea sub 0
        {
            int randomIndex = Random.Range(0, questions.Count);
            currentQuestion = questions[randomIndex];
            questions.RemoveAt(randomIndex);

            questionText.text = currentQuestion.question;

            for (int i = 0; i < answerButtons.Length; i++)
            {
                answerButtons[i].onClick.RemoveAllListeners();

                if (i < currentQuestion.answers.Length)
                {
                    answerButtons[i].GetComponentInChildren<Text>().text = currentQuestion.answers[i];
                    int index = i;
                    answerButtons[i].onClick.AddListener(delegate { CheckAnswer(index); });

                    answerButtons[i].gameObject.SetActive(true);
                }
                else
                {
                    answerButtons[i].gameObject.SetActive(false);
                }
            }

            questionCounter--; // Scădem doar dacă există întrebări de pus
            Debug.Log("📉 Întrebări rămase: " + questionCounter);
        }
        else
        {
            Debug.Log("✅ Toate întrebările au fost finalizate! Afișez butonul Continue.");
            ShowContinueButton();
        }
    }



    public void CheckAnswer(int index)
    {
        Debug.Log("CheckAnswer apelat! Index: " + index);

        if (index == currentQuestion.correctAnswer)
        {
            GameManager.instance.AddScore(5);
        }
        else
        {
            if (GameManager.instance.scoreCount >= 10)
            {
                GameManager.instance.AddScore(-5);
            }
            else
            {
                Debug.Log("⚠️ Nu mai ai monede! Colectează mai multe pentru a continua.");
                GameManager.instance.AddScore(-5);
                if (collectCoinsButton != null)
                {
                    collectCoinsButton.ShowCollectButton();
                }
                CheckGameOver();
                return;
            }
        }

        if (collectCoinsButton != null)
        {
            collectCoinsButton.CheckScore();
        }

        CheckGameOver();

        if (questions.Count > 0)
        {
            Invoke("DisplayNextQuestion", 0.5f);
        }
        else
        {
            Debug.Log("✅ Nu mai sunt întrebări! Afișez butonul Continue.");
            Invoke("ShowContinueButton", 0.5f);
        }
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

            if (continueButton != null)
            {
                continueButton.SetActive(true);
            }

            Debug.Log("🎉 Quiz finalizat! Butonul 'Continue' este acum vizibil.");
        }
        else
        {
            Debug.Log("❌ Butonul Continue NU trebuie să apară încă. Întrebări rămase: " + questionCounter);
            if (continueButton != null)
            {
                continueButton.SetActive(false);
            }
        }
    }




    void CheckGameOver()
    {
        Debug.Log("🔍 CheckGameOver() apelată! Scor curent: " + GameManager.instance.scoreCount);

        if (GameManager.instance.scoreCount <= 0)
        {
            Debug.Log("✅ Scor 0 detectat! Dezactivez butoanele și afișez 'Button_back'.");

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
                Debug.Log("✅ Butonul 'Button_back' a fost găsit și activat!");
                buttonBack.SetActive(true);
            }
            else
            {
                Debug.LogError("❌ Eroare: 'Button_back' nu a fost găsit!");
            }

            // Asigură-te că UI-ul este actualizat corect
            Invoke("EnsureUIUpdated", 0.1f);
        }
    }


    void EnsureUIUpdated()
    {
        GameManager.instance.coinTextScore.text = "x0";
        Debug.Log("🔄 UI actualizat forțat: " + GameManager.instance.coinTextScore.text);
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

        Checkpoint.MarkCheckpointCompleted(); // ✅ Marchează checkpoint-ul ca finalizat

        Debug.Log("🎉 Quiz finalizat! Jocul continuă.");
    }


    public void OnBackButtonPressed()
    {
        Debug.Log("🔄 Dezactivez ColiderLeftCheckpoint și închid quiz-ul!");
        coliderLeftCheckpoint.SetActive(false); // Dezactivează coliderul stânga
        quizCanvas.SetActive(false); // Închide quiz-ul
    }

}

[System.Serializable]
public class Question
{
    public string question;
    public string[] answers;
    public int correctAnswer;

    public Question(string q, string[] a, int correct)
    {
        question = q;
        answers = a;
        correctAnswer = correct;
    }
}
