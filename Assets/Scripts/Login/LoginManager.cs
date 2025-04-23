using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    public InputField usernameInput;
    public InputField passwordInput;
    public Text messageText;
    private UserManager userManager;

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

    public void Login()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;

        if (userManager.LoginUser(username, password, out UserProgress progress))
        {
            PlayerPrefs.SetString("CurrentUsername", username);
            PlayerPrefs.SetString("CurrentPassword", password);
            messageText.text = "Login successful!";
            StartCoroutine(LoadMainMenuAfterDelay());
            // Load the game scene or continue to the main menu
        }
        else
        {
            messageText.text = "Invalid username or password.";
        }
    }

    private System.Collections.IEnumerator LoadMainMenuAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("MainMenu"); 
    }
}