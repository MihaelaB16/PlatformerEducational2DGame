using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance; // Singleton pentru acces global
    public int scoreCount = 0; // Scorul global
    public Text coinTextScore; // Referința la textul UI pentru scor

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Găsește și inițializează CoinsText
        coinTextScore = GameObject.Find("CoinsText").GetComponent<Text>();
        coinTextScore.text = "x" + scoreCount;
    }

    public void AddScore(int amount)
    {
        scoreCount += amount;
        coinTextScore.text = "x" + scoreCount;
    }
}
