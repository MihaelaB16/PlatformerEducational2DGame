using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    private UserManager userManager;

    private void Start()
    {
        // Find the UserManager instance in the scene
        userManager = UserManager.instance;
        if (userManager == null)
        {
            Debug.LogError("UserManager instance not found!");
        }
    }
    public void PlayGame()
    {
        SceneManager.LoadScene("Gameplay");

        
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GamePlay")
        {
            // Get the current logged in username from LoginManager
            string username = LoginManager.instance?.GetLoggedInUsername();

            if (string.IsNullOrEmpty(username))
            {
                Debug.LogError("No user is currently logged in!");
                return;
            }

            if (userManager != null)
            {
                userManager.InitializeGameWithUserProgress(username);
                Debug.Log($"Progress initialized for user: {username}");
            }
            else
            {
                Debug.LogError("UserManager instance is null, cannot initialize progress!");
            }

            // Remove the listener after it's been called to prevent multiple registrations
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}
