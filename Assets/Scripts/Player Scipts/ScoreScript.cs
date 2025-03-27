using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;  //!!! pentru a folosi "Text" linia 8

public class ScoreScript : MonoBehaviour
{
    //private Text coinTextScore;
    private AudioSource audioManager;
    // private int scoreCount;

    public Text coinRespawnTimerText; // Text UI pentru timer
    public float coinRespawnTime = 60f; // Timpul până la reapariția monedei
    public AudioClip coinRespawnSound; // Sunet pentru reapariție
    private bool isCoinRespawning = false;

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
        if (target.tag == MyTags.COIN_TAG)
        {
            audioManager.Play();
            target.gameObject.SetActive(false);
            //scoreCount++;
            //coinTextScore.text = "x" + scoreCount;
            // Actualizăm scorul prin GameManager
            GameManager.instance.AddScore(1);

            StartCoroutine(ReappearCoinAfterTime(target.gameObject, coinRespawnTime)); // 60f = 60 secunde
        }
    }

    IEnumerator ReappearCoinAfterTime(GameObject coin, float delay)
    {
        //  yield return new WaitForSeconds(delay); // Așteaptă 1 minut
        // coin.SetActive(true); // Re-apari moneda
        isCoinRespawning = true;
        float timeLeft = delay;

        while (timeLeft > 0)
        {
            coinRespawnTimerText.text = "Timer: " + Mathf.Ceil(timeLeft) + "s";
            timeLeft -= Time.deltaTime;
            yield return null;
        }

        coin.SetActive(true);

        // Efect vizual la reapariție
        StartCoroutine(AnimateCoinAppearance(coin));

        // Efect sonor
        if (coinRespawnSound != null)
        {
            audioManager.PlayOneShot(coinRespawnSound);
        }

        isCoinRespawning = false;
        coinRespawnTimerText.text = ""; // Șterge timer-ul după reapariție

    }

    IEnumerator AnimateCoinAppearance(GameObject coin)
    {
        Vector3 originalScale = coin.transform.localScale;
        coin.transform.localScale = Vector3.zero;

        float elapsedTime = 0f;
        float duration = 0.5f;

        while (elapsedTime < duration)
        {
            coin.transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        coin.transform.localScale = originalScale;
    }




}
