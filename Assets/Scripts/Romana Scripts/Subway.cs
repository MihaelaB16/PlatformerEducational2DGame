using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Subway : MonoBehaviour
{
    private Transform subway2Position;

    void Start()
    {
        GameObject subway2 = GameObject.FindGameObjectWithTag(MyTags.SUBWAY2_TAG);
        if (subway2 != null)
        {
            subway2Position = subway2.transform;
        }
    }

    // Teleporteaza jucatorul la subway2 cand atinge subway1
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(MyTags.PLAYER_TAG))
        {
            if (gameObject.CompareTag(MyTags.SUBWAY1_TAG) && subway2Position != null)
            {
                other.transform.position = subway2Position.position;
            }
        }
    }
}