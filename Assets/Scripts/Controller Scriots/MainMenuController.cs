using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    private UserManager userManager;
    public InputField usernameInput; // For the username input field
    public InputField passwordInput; // For the password input field
    public Text messageText;         // For the message text

    private void Start()
    {
        // Find the UserManager instance in the scene
        userManager = UserManager.instance;
        if (userManager == null)
        {
            Debug.LogError("UserManager instance not found!");
        }
    }
    public void PlayGame(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
         SceneManager.sceneLoaded += OnSceneLoaded;
        StartCoroutine(RestoreProgressAfterSceneLoad(sceneName));
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Login")
        {
            usernameInput = GameObject.Find("UsernameInput")?.GetComponent<InputField>();
            passwordInput = GameObject.Find("PasswordInput")?.GetComponent<InputField>();
            messageText = GameObject.Find("MessageText")?.GetComponent<Text>();

            Button loginButton = GameObject.Find("LoginButton")?.GetComponent<Button>();
            if (loginButton != null)
            {
                loginButton.onClick.RemoveAllListeners();
                loginButton.onClick.AddListener(() =>
                {
                    if (LoginManager.instance != null)
                    {
                        LoginManager.instance.OnLoginButtonClicked(); // Call the method from LoginManager
                    }
                    else
                    {
                        Debug.LogError("LoginManager instance is null.");
                    }
                });
            }
            else
            {
                Debug.LogError("Login button not found in the scene.");
            }
        }
    }

    public void LoadLogin()
    {
        LoginManager.instance.ResetLoginState();
        Destroy(LoginManager.instance.gameObject);
        Debug.Log("Se încarcă scena Login...");
        SceneManager.LoadScene("Login");
    }

    private IEnumerator RestoreProgressAfterSceneLoad(string sceneName)
    {
        yield return new WaitUntil(() => SceneManager.GetActiveScene().name == sceneName);
        yield return null; // așteaptă un frame pentru inițializare completă

        string username = LoginManager.instance.GetLoggedInUsername();
        UserManager.instance.RestoreProgressForScene(username, sceneName);
    }

}
