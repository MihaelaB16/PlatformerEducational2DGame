using UnityEngine;
using UnityEngine.UI;

public class CollectCoinsButton : MonoBehaviour
{
    public GameObject quizCanvas; // Referință către UI-ul quizului
    public Button collectButton;  // Referință către butonul "Colectează Monede"

    void Start()
    {
        collectButton.gameObject.SetActive(false); // Dezactivează butonul inițial
    }
    void Update()
    {
        if (GameManager.instance.scoreCount <= 10)
        {
            collectButton.gameObject.SetActive(true);
        }
        else
        {
            collectButton.gameObject.SetActive(false);
        }
    }

    public void CheckScore()
    {
        if (GameManager.instance.scoreCount <= 10)
        {
            collectButton.gameObject.SetActive(true);
        }
        else
        {
            collectButton.gameObject.SetActive(false);
        }
    }
    public void ShowCollectButton()
    {
        
            collectButton.gameObject.SetActive(true);
        
    }

    public void CollectMoreCoins()
    {
        quizCanvas.SetActive(false);
        collectButton.gameObject.SetActive(false);
        Time.timeScale = 1f; // Reia jocul
    }
}
