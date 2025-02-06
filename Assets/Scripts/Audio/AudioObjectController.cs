using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioObjectController : MonoBehaviour
{
    public AudioSource soundSource; 
    public float soundDistance = 5f;  

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (!soundSource.isPlaying)
            {
                soundSource.Play();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            soundSource.Stop();
        }
    }

    void Update()
    {
        float distance = Vector2.Distance(transform.position, GameObject.FindWithTag("Player").transform.position);

        if (distance <= soundDistance && !soundSource.isPlaying)
        {
            soundSource.Play();
        }
        else if (distance > soundDistance && soundSource.isPlaying)
        {
            soundSource.Stop();
        }
    }
}
