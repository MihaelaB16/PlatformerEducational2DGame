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



    void Start()
    {
        LoadQuestionsFromJSON();
        ShuffleQuestions();
        DisplayNextQuestion();
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
        // Definirea căii către fișierul JSON
        string filePath = Path.Combine(Application.streamingAssetsPath, "questions.json");

        if (File.Exists(filePath))
        {
            // Citește fișierul JSON
            string json = File.ReadAllText(filePath);

            // Deserializează conținutul fișierului JSON într-o listă de întrebări
            Question[] loadedQuestions = JsonHelper.FromJson<Question>(json);

            // Adaugă întrebările încărcate în lista de întrebări
            questions = new List<Question>(loadedQuestions);
        }
        else
        {
            Debug.LogError("Fișierul JSON nu a fost găsit la calea: " + filePath);
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
        if (questions.Count > 0)
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

            Debug.Log("Întrebări rămase: " + questions.Count);  // Debugging pentru numărul de întrebări rămase
        }
        else
        {
            Debug.Log("Toate întrebările au fost epuizate. Închid quiz-ul.");
            ContinueGame();
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
                CheckGameOver(); // Verifică dacă jocul trebuie să se oprească
                return;
            }
        }

        if (collectCoinsButton != null)
        {
            collectCoinsButton.CheckScore();
        }

        CheckGameOver(); // Verifică din nou după actualizarea scorului

        if (questions.Count > 0)
        {
            Invoke("DisplayNextQuestion", 0.5f);
        }
        else
        {
            Invoke("ContinueGame", 0.5f);
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

        if (backgroundOverlay != null)
        {
            backgroundOverlay.SetActive(false); // Dezactivează fundalul umbrit
        }

        Time.timeScale = 1f;
        Debug.Log("Quiz finalizat! Jocul continuă.");

        
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
