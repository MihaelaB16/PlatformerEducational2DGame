using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Manager pentru autentificare si gestionarea sesiunilor de utilizatori
public class LoginManager : MonoBehaviour
{
    public InputField usernameInput;
    public InputField passwordInput;
    public Text messageText;
    private UserManager userManager;

    public static LoginManager instance;
    private string loggedInUsername;

    // Initializare singleton pentru persistenta intre scene
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

    // Configurare initiala si conectarea la UserManager
    void Start()
    {
        userManager = FindObjectOfType<UserManager>();
        Button loginButton = GameObject.Find("LoginButton")?.GetComponent<Button>();
        if (loginButton != null)
        {
            loginButton.onClick.RemoveAllListeners(); // Elimina ascultatorii anteriori
            loginButton.onClick.AddListener(OnLoginButtonClicked); // Adauga metoda OnClick
        }
    }

    // Inregistreaza un utilizator nou
    public void Register()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;

        if (userManager.RegisterUser(username, password))
        {
            messageText.text = "Inregistrare reusita!";
        }
        else
        {
            messageText.text = "Utilizatorul deja exista!";
        }
    }

    // Gestioneaza logarea utilizatorului
    public void OnLoginButtonClicked()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;

        if (userManager.LoginUser(username, password, out UserProgress progress))
        {
            SetLoggedInUsername(username); // Seteaza utilizatorul logat
            messageText.text = $"Utilizatorul '{username}' a fost logat cu succes.";
            SceneManager.LoadScene("MainMenu");
        }
        else
        {
            messageText.text = "Nume de utilizator sau parola incorecta";
        }
    }

    // Seteaza utilizatorul curent logat
    public void SetLoggedInUsername(string username)
    {
        loggedInUsername = username;
    }

    // Reseteaza starea de logare
    public void ResetLoginState()
    {
        if (LoginManager.instance != null)
        {
            LoginManager.instance.SetLoggedInUsername(null);
        }
    }

    // Obtine numele utilizatorului curent logat
    public string GetLoggedInUsername()
    {
        return loggedInUsername;
    }

    // Incarca meniul principal cu intarziere (pentru efecte vizuale)
    private System.Collections.IEnumerator LoadMainMenuAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("MainMenu");
    }
}