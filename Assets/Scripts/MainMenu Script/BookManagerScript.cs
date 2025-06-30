using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BookManagerScript : MonoBehaviour
{
    [Header("Book Buttons")]
    public Button bookButton;

    [Header("Canvas")]
    public GameObject bookCanvas;

    [Header("Page Images")]
    public Image pageDisplayImage;
    public Sprite page1Image;
    public Sprite page2Image;
    public Sprite page3Image;

    [Header("Navigation Buttons")]
    public Button nextPageButton;
    public Button backPageButton;

    [Header("Close Button")]
    public Button closeButton;

    private int currentPage = 1;

    void Start()
    {
        if (bookButton != null)
        {
            bookButton.onClick.AddListener(OpenBook);
        }

        if (nextPageButton != null)
        {
            nextPageButton.onClick.AddListener(NextPage);
        }

        if (backPageButton != null)
        {
            backPageButton.onClick.AddListener(BackPage);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseBook);
        }

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
            currentPage = 1;
            UpdatePage();
        }
    }

    public void CloseBook()
    {
        if (bookCanvas != null)
        {
            bookCanvas.SetActive(false);
        }
    }

    public void NextPage()
    {
        if (currentPage < 3)
        {
            currentPage++;
            UpdatePage();
        }
    }

    public void BackPage()
    {
        if (currentPage > 1)
        {
            currentPage--;
            UpdatePage();
        }
    }

    // Actualizeaza imaginea si butoanele pe baza paginii curente
    private void UpdatePage()
    {
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

        UpdateNavigationButtons();
    }

    // Controleaza vizibilitatea butoanelor de navigare
    private void UpdateNavigationButtons()
    {
        if (currentPage == 1)
        {
            if (nextPageButton != null) nextPageButton.gameObject.SetActive(true);
            if (backPageButton != null) backPageButton.gameObject.SetActive(false);
        }
        else if (currentPage == 2)
        {
            if (nextPageButton != null) nextPageButton.gameObject.SetActive(true);
            if (backPageButton != null) backPageButton.gameObject.SetActive(true);
        }
        else if (currentPage == 3)
        {
            if (nextPageButton != null) nextPageButton.gameObject.SetActive(false);
            if (backPageButton != null) backPageButton.gameObject.SetActive(true);
        }
    }

    public void SetPage(int pageNumber)
    {
        if (pageNumber >= 1 && pageNumber <= 3)
        {
            currentPage = pageNumber;
            UpdatePage();
        }
    }

    public int GetCurrentPage()
    {
        return currentPage;
    }
}