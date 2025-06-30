using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("UI References")]
    public InputField usernameInput;
    public InputField passwordInput;
    public Text messageText;

    private UserManager userManager;

    private void Start()
    {
        userManager = UserManager.instance;
    }

    // Incarca o scena de joc si restaureaza progresul
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

    // Configureaza butoanele cand se incarca scena de login
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
                        LoginManager.instance.OnLoginButtonClicked();
                    }
                });
            }
        }
    }

    public void LoadLogin()
    {
        LoginManager.instance.ResetLoginState();
        Destroy(LoginManager.instance.gameObject);
        SceneManager.LoadScene("Login");
    }

    // Restaureaza progresul dupa incarcarea scenei
    private IEnumerator RestoreProgressAfterSceneLoad(string sceneName)
    {
        yield return new WaitUntil(() => SceneManager.GetActiveScene().name == sceneName);
        yield return null;

        string username = LoginManager.instance.GetLoggedInUsername();
        UserManager.instance.RestoreProgressForScene(username, sceneName);
    }
}