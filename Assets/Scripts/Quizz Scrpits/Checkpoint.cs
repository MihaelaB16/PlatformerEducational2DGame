using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Checkpoint : MonoBehaviour
{
    [Header("Canvas-uri")]
    public GameObject quizCanvas;
    public GameObject inputQuizCanvas;

    [Header("Elemente Quiz")]
    public GameObject[] questionButtons;
    public GameObject imageQuestion;
    public GameObject btnBack;
    public GameObject noCoinsMessage;
    public GameObject btnContinue;

    [Header("Collidere")]
    public GameObject coliderLeftCheckpoint;
    public GameObject coliderRightCheckpoint;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(HandleCheckpoint());
        }
    }

    IEnumerator HandleCheckpoint()
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(0.5f);
        Time.timeScale = 1f;

        // Activeaza collidere pentru a bloca jucatorul
        coliderLeftCheckpoint.SetActive(true);
        coliderRightCheckpoint.SetActive(true);

        // Afiseaza instructiunile quiz-ului
        inputQuizCanvas.SetActive(true);
    }

    public void OnContinueInputButtonPressed()
    {
        inputQuizCanvas.SetActive(false);
        StartCoroutine(ShowQuizAfterInput());
    }

    IEnumerator ShowQuizAfterInput()
    {
        quizCanvas.SetActive(true);
        yield return null;

        if (GameManager.instance != null)
        {
            GameManager.instance.RefreshUIReferences();
        }

        // Verifica daca jucatorul are monede suficiente
        if (GameManager.instance.scoreCount <= 0)
        {
            // Nu are monede - afiseaza doar butonul Back
            foreach (GameObject button in questionButtons)
            {
                button.SetActive(false);
            }

            if (btnBack != null)
                btnBack.SetActive(true);

            if (noCoinsMessage != null)
                noCoinsMessage.SetActive(true);

            if (imageQuestion != null)
                imageQuestion.SetActive(false);

            if (btnContinue != null)
                btnContinue.SetActive(false);
        }
        else
        {
            // Are monede - afiseaza quiz-ul complet
            if (imageQuestion != null)
                imageQuestion.SetActive(true);

            foreach (GameObject button in questionButtons)
            {
                button.SetActive(true);
            }

            if (noCoinsMessage != null)
                noCoinsMessage.SetActive(false);
        }
    }

    public void OnBackButtonPressed()
    {
        coliderLeftCheckpoint.SetActive(false);
        quizCanvas.SetActive(false);
    }
}