using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreScript : MonoBehaviour
{
    [Header("Coin Settings")]
    public float coinRespawnTime = 60f;
    public AudioClip coinRespawnSound;

    private AudioSource audioManager;

    private void Awake()
    {
        audioManager = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D target)
    {
        if (target.tag == MyTags.COIN_TAG)
        {
            audioManager.Play();
            target.gameObject.SetActive(false);
            GameManager.instance.AddScore(1);
            StartCoroutine(ReappearCoinAfterTime(target.gameObject, coinRespawnTime));
        }
    }

    // Reactiveaza moneda dupa timpul specificat
    IEnumerator ReappearCoinAfterTime(GameObject coin, float delay)
    {
        yield return new WaitForSeconds(delay);

        coin.SetActive(true);
        StartCoroutine(AnimateCoinAppearance(coin));

        if (coinRespawnSound != null)
        {
            audioManager.PlayOneShot(coinRespawnSound);
        }
    }

    // Animatie de aparitie pentru moneda
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