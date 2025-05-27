using UnityEngine;
using UnityEngine.UI;

public class ExitCanvasDetailsScript : MonoBehaviour
{
    [Header("Canvas References")]
    public GameObject mainCanvas;      // Canvas-ul principal
    public GameObject detailsCanvas;   // Details Canvas

    [Header("Buttons")]
    public Button showDetailsButton;   // Butonul pentru afișare
    public Button exitDetailsButton;   // Butonul pentru închidere

    void Start()
    {
        // Configurează butoanele
        if (showDetailsButton != null)
        {
            showDetailsButton.onClick.AddListener(() => SwitchCanvas(true));
        }

        if (exitDetailsButton != null)
        {
            exitDetailsButton.onClick.AddListener(() => SwitchCanvas(false));
        }

        // Asigură-te că Details Canvas este dezactivat la început
        if (detailsCanvas != null)
        {
            detailsCanvas.SetActive(false);
        }
    }

    public void SwitchCanvas(bool showDetails)
    {
        if (showDetails)
        {
            // Afișează Details Canvas
            if (mainCanvas != null) mainCanvas.SetActive(false);
            if (detailsCanvas != null)
            {
                detailsCanvas.SetActive(true);

                // Actualizează datele din Details Canvas
                DetailsCanvasManager detailsManager = FindObjectOfType<DetailsCanvasManager>();
                if (detailsManager != null)
                {
                    detailsManager.RefreshDetails();
                }
            }
        }
        else
        {
            // Revino la Main Canvas
            if (detailsCanvas != null) detailsCanvas.SetActive(false);
            if (mainCanvas != null) mainCanvas.SetActive(true);
        }
    }

    // Metode publice pentru a fi apelate din UI
    public void ShowDetails()
    {
        SwitchCanvas(true);
    }

    public void ExitDetails()
    {
        SwitchCanvas(false);
    }
}