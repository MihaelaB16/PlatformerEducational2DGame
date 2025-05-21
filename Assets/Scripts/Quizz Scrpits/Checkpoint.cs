using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Checkpoint : MonoBehaviour
{
    public GameObject quizCanvas; // Referință către UI-ul quizului
    public GameObject[] questionButtons; // Array cu toate butoanele întrebărilor
    public GameObject imageQuestion;
    public GameObject btnBack;
    public GameObject noCoinsMessage;
    public GameObject btnContinue;
    public GameObject coliderLeftCheckpoint; // Collider stânga
    public GameObject coliderRightCheckpoint; // Collider dreapta



    public void OnBackButtonPressed()
    {
        Debug.Log("🔄 Dezactivez ColiderLeftCheckpoint și închid quiz-ul!");
        coliderLeftCheckpoint.SetActive(false); // Dezactivează coliderul stânga
        quizCanvas.SetActive(false); // Închide quiz-ul
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // ✅ Verificăm dacă nu e completat
        {
            StartCoroutine(HandleCheckpoint());
        }
    }

    IEnumerator HandleCheckpoint()
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(0.5f);
        Time.timeScale = 1f;

        coliderLeftCheckpoint.SetActive(true);
        coliderRightCheckpoint.SetActive(true);

        quizCanvas.SetActive(true);
        yield return null; // Așteaptă un frame pentru ca UI-ul să se inițializeze

        if (GameManager.instance != null)
        {
            GameManager.instance.RefreshUIReferences();
        }

        if (GameManager.instance.scoreCount <= 0)
        {
            Debug.Log("⚠️ Scor zero/negativ detectat! Afișez doar butonul Back.");

            // Dezactivează toate butoanele de întrebări
            foreach (GameObject button in questionButtons)
            {
                button.SetActive(false);
            }

            // Găsește și activează doar butonul Back
            if (btnBack != null)
            {
                btnBack.SetActive(true);
            }
            // Opțional: Fă vizibil un mesaj explicativ pentru jucător
            if (noCoinsMessage != null)
            {
                noCoinsMessage.SetActive(true);
            }
            if (imageQuestion != null)
            {
                imageQuestion.SetActive(false);
            }
            if(btnContinue != null)
{
                btnContinue.SetActive(false);
            }
        }
        else
        {
            if (imageQuestion != null)
            {
                imageQuestion.SetActive(true);
            }
            // Scor pozitiv, afișează butoanele de întrebări
            foreach (GameObject button in questionButtons)
            {
                button.SetActive(true);
            }

            
            if (noCoinsMessage != null)
            {
                noCoinsMessage.SetActive(false);
            }

        }
    }

}