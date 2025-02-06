using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartCanvas : MonoBehaviour
{
    
    public GameObject startCanvas;  
    private bool gameStarted = false;  

    void Start()
    {
        startCanvas.SetActive(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && !gameStarted)
        {
            startCanvas.SetActive(false);

            gameStarted = true;

            SceneManager.LoadScene("SampleScene");
        }
    }
}
