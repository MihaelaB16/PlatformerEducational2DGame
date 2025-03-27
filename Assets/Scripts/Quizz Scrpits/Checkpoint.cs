using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Checkpoint : MonoBehaviour
{
    public GameObject quizCanvas; // Referință către UI-ul quizului
    public GameObject[] questionButtons; // Array cu toate butoanele întrebărilor

    public GameObject coliderLeftCheckpoint; // Collider stânga
    public GameObject coliderRightCheckpoint; // Collider dreapta

    private static bool checkpointCompleted = false; // ✅ Marcare dacă acest checkpoint a fost completat

    public void OnBackButtonPressed()
    {
        Debug.Log("🔄 Dezactivez ColiderLeftCheckpoint și închid quiz-ul!");
        coliderLeftCheckpoint.SetActive(false); // Dezactivează coliderul stânga
        quizCanvas.SetActive(false); // Închide quiz-ul
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !checkpointCompleted) // ✅ Verificăm dacă nu e completat
        {
            StartCoroutine(HandleCheckpoint());
        }
    }

    IEnumerator HandleCheckpoint()
    {
        Time.timeScale = 0f; // Oprește jocul
        yield return new WaitForSecondsRealtime(0.5f);
        Time.timeScale = 1f; // Reia jocul

        coliderLeftCheckpoint.SetActive(true);
        coliderRightCheckpoint.SetActive(true);

        if (GameManager.instance.scoreCount <= 0)
        {
            quizCanvas.SetActive(true);
        }
        else
        {
            foreach (GameObject button in questionButtons)
            {
                button.SetActive(true);
            }

            quizCanvas.SetActive(true);
        }
    }

    public static void MarkCheckpointCompleted() // ✅ Funcție pentru a seta checkpoint-ul ca finalizat
    {
        checkpointCompleted = true;
        Debug.Log("✅ Checkpoint marcat ca finalizat!");
    }
}