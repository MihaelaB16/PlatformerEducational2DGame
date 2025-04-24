using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    public InputField usernameInput;
    public InputField passwordInput;
    public Text messageText;
    private UserManager userManager;

    public static LoginManager instance;
    private string loggedInUsername;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        userManager = FindObjectOfType<UserManager>();
    }

    public void Register()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;

        if (userManager.RegisterUser(username, password))
        {
            messageText.text = "Registration successful!";
        }
        else
        {
            messageText.text = "Username already exists.";
        }
    }

    public void OnLoginButtonClicked()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;

        if (userManager.LoginUser(username, password, out UserProgress progress))
        {
            LoginManager.instance.SetLoggedInUsername(username);
            Debug.Log($"User '{username}' logged in successfully.");

            SceneManager.LoadScene("GamePlay");
        }
        else
        {
            Debug.LogError("Invalid username or password.");
        }
    }

    public void SetLoggedInUsername(string username)
    {
        loggedInUsername = username;
        Debug.Log($"Logged-in user set to: {loggedInUsername}");
    }

    public string GetLoggedInUsername()
    {
        return loggedInUsername;
    }


    private System.Collections.IEnumerator LoadMainMenuAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("MainMenu"); 
    }
}