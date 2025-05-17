using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FinalScript : MonoBehaviour
{
    public GameObject quizCanvas;
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
        yield return new WaitForSecondsRealtime(0.5f);

        quizCanvas.SetActive(true);
       
    }
    public void LoadMainMenu()
    {
        if (UserManager.instance != null)
        {
            UserManager.instance.SavePlayerPosition();
            UserManager.instance.SaveProgressData();
        }
        else
        {
            Debug.LogError("UserManager instance is null. Cannot save player position or progress data.");
        }
        Debug.Log("Se încarcă scena MainMenu...");
        quizCanvas.SetActive(true);
        SceneManager.LoadScene("MainMenu");
    }
}
