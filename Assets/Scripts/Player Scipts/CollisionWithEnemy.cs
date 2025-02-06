using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CollisionWithEnemy : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(MyTags.PLAYER_TAG))
        {
            if (gameObject.CompareTag(MyTags.BEETLE_TAG) || gameObject.CompareTag(MyTags.SNAIL_TAG)|| gameObject.CompareTag(MyTags.FROG_TAG)|| gameObject.CompareTag(MyTags.SPIDER_TAG)|| gameObject.CompareTag(MyTags.WATER_TAG))
            {
                SceneManager.LoadScene("SampleScene");
            }
        }
    }
}
