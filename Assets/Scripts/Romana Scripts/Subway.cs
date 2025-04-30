using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Subway : MonoBehaviour
{
    private Transform subway2Position;

    void Start()
    {
        // Find the object tagged as "subway2" and store its position
        GameObject subway2 = GameObject.FindGameObjectWithTag(MyTags.SUBWAY2_TAG);
        if (subway2 != null)
        {
            subway2Position = subway2.transform;
        }
        else
        {
            Debug.LogError("No object with tag 'subway2' found in the scene.");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the colliding object is the player
        if (other.CompareTag(MyTags.PLAYER_TAG))
        {
            // Check if this object is tagged as "subway1"
            if (gameObject.CompareTag(MyTags.SUBWAY1_TAG) && subway2Position != null)
            {
                Debug.Log("Player collided with subway1. Moving to subway2 position.");
                // Restore the player's position to subway2's position
                other.transform.position = subway2Position.position;
            }
        }
    }
}
