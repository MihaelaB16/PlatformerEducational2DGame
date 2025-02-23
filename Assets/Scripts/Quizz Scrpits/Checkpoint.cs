using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public GameObject quizCanvas; // Referință către UI-ul quizului
    public GameObject[] questionButtons; // Array cu toate butoanele întrebărilor
    public GameObject continueButton; // Butonul de continuare

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(HandleCheckpoint());
        }
    }

    IEnumerator HandleCheckpoint()
    {
        Time.timeScale = 0f; // Oprește jocul
        yield return new WaitForSecondsRealtime(1f); // Așteaptă 1 secundă în timp real
        Time.timeScale = 1f; // Reia jocul

        if (GameManager.instance.scoreCount <= 0)
        {
            quizCanvas.SetActive(true);
        }
        else
        {
            // Activează butoanele de întrebări și butonul de continuare
            foreach (GameObject button in questionButtons)
            {
                button.SetActive(true);
            }
            continueButton.SetActive(true);

            quizCanvas.SetActive(true);
        }
    }
}
