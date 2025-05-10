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
            Debug.Log("LoginManager instance created.");
        }
        else
        {
            Debug.LogWarning("Duplicate LoginManager instance detected and destroyed.");
            Destroy(gameObject);
        }
    }

    void Start()
    {
        userManager = FindObjectOfType<UserManager>();
        Button loginButton = GameObject.Find("LoginButton")?.GetComponent<Button>();
        if (loginButton != null)
        {
            loginButton.onClick.RemoveAllListeners(); // Elimină ascultătorii anteriori
            loginButton.onClick.AddListener(OnLoginButtonClicked); // Adaugă metoda OnClick
            Debug.Log("Login button OnClick reassigned.");
        }
        else
        {
            Debug.LogError("Login button not found in the scene.");
        }
    }
    


    public void Register()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;

        if (userManager.RegisterUser(username, password))
        {
            messageText.text = "Înregistrare reușită!";
        }
        else
        {
            messageText.text = "Utilizatorul deja există!";
        }
    }

    public void OnLoginButtonClicked()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;

        if (userManager.LoginUser(username, password, out UserProgress progress))
        {
            SetLoggedInUsername(username); // Setează utilizatorul logat
            Debug.Log($"User '{username}' logged in successfully.");
            messageText.text = $"Utilizatorul '{username}' a fost logat cu succes.";
            SceneManager.LoadScene("MainMenu");
        }
        else
        {
            messageText.text = "Nume de utilizator sau parolă incorectă";
            Debug.LogError("Invalid username or password.");
        }
    }

    public void SetLoggedInUsername(string username)
    {
        loggedInUsername = username;
        Debug.Log($"Logged-in user set to: {loggedInUsername}");
    }
    public void ResetLoginState()
    {
        if (LoginManager.instance != null)
        {
            LoginManager.instance.SetLoggedInUsername(null);
            Debug.Log("Login state reset.");
        }
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