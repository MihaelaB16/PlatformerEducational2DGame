using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    [Header("Setari Fundal")]
    public float backgroundSwitchThreshold = 100f;
    public GameObject background1;
    public GameObject background2;

    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Gaseste background-urile automat daca nu sunt setate
        if (background1 == null)
            background1 = GameObject.Find("Background");
        if (background2 == null)
            background2 = GameObject.Find("Background2");

        Invoke("SetCorrectBackgroundAtStart", 0.2f);
    }

    void SetCorrectBackgroundAtStart()
    {
        if (player != null)
        {
            SetBackgroundBasedOnPosition(player.position.x);
        }
    }

    public void SetBackgroundBasedOnPosition(float playerX)
    {
        if (background1 == null || background2 == null)
            return;

        if (playerX > backgroundSwitchThreshold)
        {
            // Activeaza Background2
            background1.SetActive(false);
            background2.SetActive(true);
            FlagController.currentBackground = background2;
        }
        else
        {
            // Activeaza Background1
            background1.SetActive(true);
            background2.SetActive(false);
            FlagController.currentBackground = background1;
        }
    }

    // Actualizeaza fundalul pe baza pozitiei curente a jucatorului
    public void RefreshBackground()
    {
        if (player != null)
        {
            SetBackgroundBasedOnPosition(player.position.x);
        }
    }
}