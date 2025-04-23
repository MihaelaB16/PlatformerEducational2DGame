using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerDamage : MonoBehaviour
{
    private Text lifeText;
    private int lifeScoreCount;

    private bool canDamage;
    private Vector3 initialPosition;
    void Awake()
    {
        lifeText = GameObject.Find("LifeText").GetComponent<Text>();
        lifeScoreCount = 3;
        lifeText.text = "x" + lifeScoreCount;

        canDamage = true;
        initialPosition = transform.position;

    }
    void Start()
    {
        Time.timeScale = 1f;  //cand se incepe jocul, timpul va fi normal
    }

    // Update is called once per frame
    public void DealDamage()
    {
        if (canDamage)
        {
            
            lifeScoreCount--;
            if (lifeScoreCount > 0)
            {
                
                Time.timeScale = 0f;
                StartCoroutine(ReturnToFlag());
                lifeText.text = "x" + lifeScoreCount;
            }
            else
            {
                //restart game
                // Time.timeScale = 0f;   //face ca totul sa stea pe loc (jocul sa fie oprit) totul va fi inghetat
                Time.timeScale = 0f;
                StartCoroutine(RestartGame());
            }
            
            canDamage = false;

            StartCoroutine(WaitForDamage());
        }
       
    }
    //void Update()
    //{
    //    float move = Input.GetAxis("Horizontal");
    //    transform.position += new Vector3(move * Time.deltaTime * 5f, 0, 0);
    //}

    IEnumerator WaitForDamage()
    {
        yield return new WaitForSeconds(2f);
        canDamage = true;
    }

    IEnumerator ReturnToFlag()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        Time.timeScale = 1f;

        // Mutăm personajul la ultimul steag sau la poziția inițială
        transform.position = (FlagController.lastFlagPosition != Vector3.zero) ? FlagController.lastFlagPosition : initialPosition;

        // Restaurăm fundalul curent sau setăm fundalul inițial
        GameObject background = GameObject.Find("Background");
        GameObject background2 = GameObject.Find("Background");

        if (FlagController.currentBackground != null)
        {
            Debug.Log("Restaurăm fundalul curent: " + FlagController.currentBackground.name);

            if (FlagController.currentBackground == background)
            {
                background.SetActive(true);
                if (background2 != null) background2.SetActive(false);
            }
            else if (FlagController.currentBackground == background2)
            {
                background2.SetActive(true);
                if (background != null) background.SetActive(false);
            }
        }
        else
        {
            // Setăm fundalul inițial dacă currentBackground nu este setat
            if (background != null)
            {
                background.SetActive(true);
                if (background2 != null) background2.SetActive(false);
                FlagController.currentBackground = background; // Setăm currentBackground la fundalul inițial
                Debug.Log("Restaurăm fundalul inițial: " + background.name);
            }
        }
        //lifeScoreCount = 3; // Resetăm viața
        lifeText.text = "x" + lifeScoreCount;
        Time.timeScale = 1f;
    }

    IEnumerator RestartGame()
    {
        yield return new WaitForSecondsRealtime(2f); // Așteaptă 2 secunde în timp real
        SceneManager.LoadScene("GamePlay"); // Reîncarcă scena "GamePlay"
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(MyTags.WATER_TAG) || other.CompareTag(MyTags.THORNS_TAG))
        {
            DealDamage();
        }
    }

}//class





















