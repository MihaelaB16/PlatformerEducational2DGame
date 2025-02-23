using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;  //!!! pentru a folosi "Text" linia 8

public class ScoreScript : MonoBehaviour
{
    //private Text coinTextScore;
    private AudioSource audioManager;
   // private int scoreCount;

    private void Awake()
    {
        audioManager = GetComponent<AudioSource>();
    }
    void Start()
    {
     //   coinTextScore=GameObject.Find("CoinsText").GetComponent<Text>();
    }

    private void OnTriggerEnter2D(Collider2D target)
    {
        if (target.tag==MyTags.COIN_TAG)
        {
            audioManager.Play();
            target.gameObject.SetActive(false);
            //scoreCount++;
            //coinTextScore.text = "x" + scoreCount;
            // Actualizăm scorul prin GameManager
            GameManager.instance.AddScore(1);

            StartCoroutine(ReappearCoinAfterTime(target.gameObject, 120f)); // 60f = 60 secunde
        }
    }

    IEnumerator ReappearCoinAfterTime(GameObject coin, float delay)
    {
        yield return new WaitForSeconds(delay); // Așteaptă 1 minut
        coin.SetActive(true); // Re-apari moneda
    }
}
