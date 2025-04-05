using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagController : MonoBehaviour
{
   // public GameObject messageText; // CanvasLevelUp
    public GameObject lastBackground;
    public GameObject nextBackground;

    private Rigidbody2D playerRb;
    private PlayerMovement playerMovement;
    public static Vector3 lastFlagPosition;
    public static GameObject currentBackground;
    void Start()
    {
        //messageText.SetActive(false);
        playerRb = GameObject.FindWithTag("Player").GetComponent<Rigidbody2D>();
        //playerMovement = GameObject.FindWithTag("Player").GetComponent<PlayerMovement>();

        // Setăm lastBackground și nextBackground la "Background" dacă nu sunt setate
        if (lastBackground == null)
        {
            lastBackground = GameObject.Find("Background");
            Debug.Log("lastBackground nu a fost setat, folosim implicit: " + lastBackground.name);
        }
        if (nextBackground == null)
        {
            nextBackground = GameObject.Find("Background");
            Debug.Log("nextBackground nu a fost setat, folosim implicit: " + nextBackground.name);
        }

        if (currentBackground == null)
        {
            currentBackground = lastBackground; // Setăm fundalul inițial
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player a atins steagul!");

           
          //  messageText.SetActive(true);

            playerRb.velocity = Vector2.zero;
          

            // Schimbă fundalul
            if (lastBackground != null)
            {
                Debug.Log("Dezactivez fundalul anterior: " + lastBackground.name);
                lastBackground.SetActive(false);
            }
            Debug.Log("Activez următorul fundal: " + nextBackground.name);
            nextBackground.SetActive(true);
            currentBackground = nextBackground;
            lastFlagPosition = transform.position;

            Invoke("HideMessage", 3f);
        }
    }

    void HideMessage()
    {
        
    }
}
