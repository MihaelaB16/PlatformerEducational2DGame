using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagController : MonoBehaviour
{
    public GameObject messageText; 
    private Rigidbody2D playerRb;
    void Start()
    {
        messageText.SetActive(false);
        playerRb = GameObject.FindWithTag("Player").GetComponent<Rigidbody2D>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player a atins steagul!");

           
            messageText.SetActive(true);

            playerRb.velocity = Vector2.zero;  
            playerRb.isKinematic = true;       
                                            
            Invoke("HideMessage", 3f);
        }
    }

    void HideMessage()
    {
        messageText.SetActive(false);

        playerRb.isKinematic = false;  
    }
}
