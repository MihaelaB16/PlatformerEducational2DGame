using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BookManagerScript : MonoBehaviour
{
    [Header("Book Buttons")]
    public Button bookButton; // BookButtonMate sau BookButtonRomana

    [Header("Canvas")]
    public GameObject bookCanvas; // CanvasNoteMate sau CanvasNoteRomana

    [Header("Page Images")]
    public Image pageDisplayImage; // Imaginea care se va schimba
    public Sprite page1Image; // Prima imagine
    public Sprite page2Image; // A doua imagine  
    public Sprite page3Image; // A treia imagine

    [Header("Navigation Buttons")]
    public Button nextPageButton; // NextPageButton
    public Button backPageButton; // BackPageButton

    [Header("Close Button")]
    public Button closeButton; // Buton pentru închiderea canvas-ului (op?ional)

    // Variabil? pentru pagina curent?
    private int currentPage = 1;

    void Start()
    {
        // Configureaz? butonul principal pentru deschiderea c?r?ii
        if (bookButton != null)
        {
            bookButton.onClick.AddListener(OpenBook);
        }

        // Configureaz? butoanele de navigare
        if (nextPageButton != null)
        {
            nextPageButton.onClick.AddListener(NextPage);
        }

        if (backPageButton != null)
        {
            backPageButton.onClick.AddListener(BackPage);
        }

        // Configureaz? butonul de închidere (dac? exist?)
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseBook);
        }

        // Asigur?-te c? canvas-ul este dezactivat la început
        if (bookCanvas != null)
        {
            bookCanvas.SetActive(false);
        }
    }

    public void OpenBook()
    {
        if (bookCanvas != null)
        {
            bookCanvas.SetActive(true);
            currentPage = 1; // Reseteaz? la prima pagin?
            UpdatePage();
            Debug.Log("Cartea a fost deschis?!");
        }
    }

    public void CloseBook()
    {
        if (bookCanvas != null)
        {
            bookCanvas.SetActive(false);
            Debug.Log("Cartea a fost închis?!");
        }
    }

    public void NextPage()
    {
        if (currentPage < 3) // Nu poate trece de pagina 3
        {
            currentPage++;
            UpdatePage();
            Debug.Log($"Navigat la pagina {currentPage}");
        }
    }

    public void BackPage()
    {
        if (currentPage > 1) // Nu poate merge sub pagina 1
        {
            currentPage--;
            UpdatePage();
            Debug.Log($"Navigat înapoi la pagina {currentPage}");
        }
    }

    private void UpdatePage()
    {
        // Actualizeaz? imaginea în func?ie de pagina curent?
        if (pageDisplayImage != null)
        {
            switch (currentPage)
            {
                case 1:
                    pageDisplayImage.sprite = page1Image;
                    break;
                case 2:
                    pageDisplayImage.sprite = page2Image;
                    break;
                case 3:
                    pageDisplayImage.sprite = page3Image;
                    break;
            }
        }

        // Actualizeaz? starea butoanelor de navigare
        UpdateNavigationButtons();
    }

    private void UpdateNavigationButtons()
    {
        if (currentPage == 1)
        {
            // Pagina 1: doar NextPageButton activ
            if (nextPageButton != null) nextPageButton.gameObject.SetActive(true);
            if (backPageButton != null) backPageButton.gameObject.SetActive(false);
        }
        else if (currentPage == 2)
        {
            // Pagina 2: ambele butoane active
            if (nextPageButton != null) nextPageButton.gameObject.SetActive(true);
            if (backPageButton != null) backPageButton.gameObject.SetActive(true);
        }
        else if (currentPage == 3)
        {
            // Pagina 3: doar BackPageButton activ
            if (nextPageButton != null) nextPageButton.gameObject.SetActive(false);
            if (backPageButton != null) backPageButton.gameObject.SetActive(true);
        }

        Debug.Log($"Pagina curent?: {currentPage} - NextButton: {(nextPageButton != null ? nextPageButton.gameObject.activeSelf : false)}, BackButton: {(backPageButton != null ? backPageButton.gameObject.activeSelf : false)}");
    }

    // Metod? public? pentru a seta pagina direct (util? pentru debugging)
    public void SetPage(int pageNumber)
    {
        if (pageNumber >= 1 && pageNumber <= 3)
        {
            currentPage = pageNumber;
            UpdatePage();
        }
    }

    // Metod? public? pentru a ob?ine pagina curent?
    public int GetCurrentPage()
    {
        return currentPage;
    }
}
