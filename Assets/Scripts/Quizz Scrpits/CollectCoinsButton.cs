using UnityEngine;
using UnityEngine.UI;

public class CollectCoinsButton : MonoBehaviour
{
    [Header("UI References")]
    public GameObject quizCanvas;
    public Button collectButton;

    void Start()
    {
        collectButton.gameObject.SetActive(false);
    }

    void Update()
    {
        // Afiseaza butonul daca jucatorul are putine monede
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
        Time.timeScale = 1f;
    }
}