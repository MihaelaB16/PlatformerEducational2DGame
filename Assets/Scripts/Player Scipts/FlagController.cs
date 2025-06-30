using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagController : MonoBehaviour
{
    [Header("Fundaluri")]
    public GameObject lastBackground;
    public GameObject nextBackground;

    private Rigidbody2D playerRb;
    public static Vector3 lastFlagPosition;
    public static GameObject currentBackground;

    void Start()
    {
        playerRb = GameObject.FindWithTag("Player").GetComponent<Rigidbody2D>();

        // Gaseste fundalurile automat daca nu sunt setate
        if (lastBackground == null)
            lastBackground = GameObject.Find("Background");

        if (nextBackground == null)
            nextBackground = GameObject.Find("Background2");

        if (currentBackground == null)
            currentBackground = lastBackground;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerRb.velocity = Vector2.zero;

            // Schimba fundalul
            if (lastBackground != null)
                lastBackground.SetActive(false);

            nextBackground.SetActive(true);
            currentBackground = nextBackground;
            lastFlagPosition = transform.position;
        }
    }
}