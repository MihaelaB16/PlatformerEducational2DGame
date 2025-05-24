using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    [Header("Background Settings")]
    public float backgroundSwitchThreshold = 100f;
    public GameObject background1;
    public GameObject background2;

    private Transform player;

    void Start()
    {
        // Găsește jucătorul
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Găsește background-urile automat dacă nu sunt setate
        if (background1 == null)
            background1 = GameObject.Find("Background");
        if (background2 == null)
            background2 = GameObject.Find("Background2");

        // Setează fundalul corect la start
        Invoke("SetCorrectBackgroundAtStart", 0.2f);
    }

    void SetCorrectBackgroundAtStart()
    {
        if (player != null)
        {
            SetBackgroundBasedOnPosition(player.position.x);
            Debug.Log($"BackgroundManager: Set initial background based on position {player.position.x}");
        }
    }

    public void SetBackgroundBasedOnPosition(float playerX)
    {
        if (background1 == null || background2 == null)
        {
            Debug.LogWarning("BackgroundManager: Background objects not assigned!");
            return;
        }

        if (playerX > backgroundSwitchThreshold)
        {
            // Activează Background2
            background1.SetActive(false);
            background2.SetActive(true);
            FlagController.currentBackground = background2;
            Debug.Log($"BackgroundManager: Activated Background2 (position: {playerX} > {backgroundSwitchThreshold})");
        }
        else
        {
            // Activează Background1
            background1.SetActive(true);
            background2.SetActive(false);
            FlagController.currentBackground = background1;
            Debug.Log($"BackgroundManager: Activated Background1 (position: {playerX} <= {backgroundSwitchThreshold})");
        }
    }

    // Metodă publică pentru a fi apelată din alte scripturi
    public void RefreshBackground()
    {
        if (player != null)
        {
            SetBackgroundBasedOnPosition(player.position.x);
        }
    }
}
