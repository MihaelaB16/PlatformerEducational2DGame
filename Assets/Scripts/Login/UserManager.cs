using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;

// Manager principal pentru gestionarea utilizatorilor si progresului lor
public class UserManager : MonoBehaviour
{
    public static UserManager instance;
    private string userFilePath;
    private Dictionary<string, UserData> users;

    private string currentSceneName;
    private Dictionary<string, float> sessionTimesPerScene = new Dictionary<string, float>();

    // Initializare singleton si incarcarea utilizatorilor
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

        userFilePath = Path.Combine(Application.persistentDataPath, "users.json");
        LoadUsers();
    }

    // Urmarire timpul petrecut in fiecare scena
    void Update()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        currentSceneName = sceneName;

        if (sceneName == "MainMenu")
        {
            return;
        }

        if (!sessionTimesPerScene.ContainsKey(sceneName))
            sessionTimesPerScene[sceneName] = 0f;

        sessionTimesPerScene[sceneName] += Time.deltaTime;
    }

    // Incarca utilizatorii din fisierul JSON
    void LoadUsers()
    {
        if (File.Exists(userFilePath))
        {
            string json = File.ReadAllText(userFilePath);
            users = JsonConvert.DeserializeObject<Dictionary<string, UserData>>(json);
            if (users == null)
            {
                users = new Dictionary<string, UserData>();
            }

            // Initializeaza structura de scene daca lipseste
            foreach (var user in users.Values)
            {
                if (user.Progress.Scenes == null)
                {
                    user.Progress.Scenes = new Dictionary<string, SceneData>();
                }
            }
        }
        else
        {
            users = new Dictionary<string, UserData>();
        }
    }

    // Salveaza utilizatorii in fisierul JSON
    void SaveUsers()
    {
        string json = JsonConvert.SerializeObject(users, Formatting.Indented);
        File.WriteAllText(userFilePath, json);
    }

    // Initializeaza jocul cu progresul utilizatorului
    public void InitializeGameWithUserProgress(string username)
    {
        if (!users.ContainsKey(username))
        {
            return;
        }

        string currentScene = SceneManager.GetActiveScene().name;
        currentSceneName = currentScene;

        float previousTime = 0f;
        if (users[username].Progress.Scenes.ContainsKey(currentScene))
            previousTime = users[username].Progress.Scenes[currentScene].Time;

        sessionTimesPerScene[currentScene] = 0f;

        if (!users[username].Progress.Scenes.ContainsKey(currentScene))
        {
            return;
        }

        SceneData sceneData = users[username].Progress.Scenes[currentScene];

        // Seteaza monedele in GameManager
        if (GameManager.instance != null)
        {
            GameManager.instance.scoreCount = sceneData.Coins;
            if (GameManager.instance.coinTextScore != null)
            {
                GameManager.instance.coinTextScore.text = "x" + sceneData.Coins;
            }

            GameManager.instance.ResetGameplayTime();
        }

        // Seteaza vietile jucatorului
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerDamage playerDamage = player.GetComponent<PlayerDamage>();
            if (playerDamage != null)
            {
                playerDamage.SetLives(sceneData.Lives);
            }
        }
    }

    // Inregistreaza un utilizator nou
    public bool RegisterUser(string username, string password)
    {
        if (users.ContainsKey(username))
        {
            return false; // Utilizatorul deja exista
        }

        UserData newUser = new UserData
        {
            Username = username,
            Password = password,
            Progress = new UserProgress()
        };

        // Initializeaza pozitiile pentru ambele scene
        newUser.Progress.Scenes["GamePlay"] = new SceneData
        {
            LastFlagPosition = new SerializableVector3(-10.0f, -3.0f, 0.0f)
        };

        newUser.Progress.Scenes["GamePlayRomana"] = new SceneData
        {
            LastFlagPosition = new SerializableVector3(-10.0f, -3.0f, 0.0f)
        };

        users[username] = newUser;
        SaveUsers();
        return true;
    }

    // Logare utilizator cu verificarea credentialelor
    public bool LoginUser(string username, string password, out UserProgress progress)
    {
        if (users.ContainsKey(username) && users[username].Password == password)
        {
            progress = users[username].Progress;

            // Initializeaza lista de intrebari daca nu exista
            if (progress.AnsweredQuestions == null)
            {
                progress.AnsweredQuestions = new List<string>();
            }

            return true;
        }

        progress = null;
        return false;
    }

    // Salveaza progresul utilizatorului
    public void SaveProgress(string username, UserProgress progress)
    {
        if (users.ContainsKey(username))
        {
            users[username].Progress = progress;
            SaveUsers();
        }
    }

    // Salveaza pozitia jucatorului pentru o anumita scena
    public void SavePlayerPosition(string username, string sceneName, Vector3 position)
    {
        DisplayTimeInConsole();
        if (users.ContainsKey(username))
        {
            if (users[username].Progress.Scenes == null)
            {
                users[username].Progress.Scenes = new Dictionary<string, SceneData>();
            }

            if (!users[username].Progress.Scenes.ContainsKey(sceneName))
            {
                users[username].Progress.Scenes[sceneName] = new SceneData();
            }

            users[username].Progress.Scenes[sceneName].LastFlagPosition = new SerializableVector3(position);
            SaveUsers();
        }
    }

    // Incarca pozitia salvata a jucatorului pentru o scena
    public Vector3 LoadPlayerPosition(string username, string sceneName)
    {
        if (string.IsNullOrEmpty(username))
        {
            return Vector3.zero;
        }

        if (string.IsNullOrEmpty(sceneName))
        {
            return Vector3.zero;
        }

        if (users.ContainsKey(username) &&
            users[username].Progress.Scenes != null &&
            users[username].Progress.Scenes.ContainsKey(sceneName))
        {
            return users[username].Progress.Scenes[sceneName].LastFlagPosition.ToVector3();
        }

        return Vector3.zero;
    }

    // Salveaza progresul la inchiderea aplicatiei
    private void OnApplicationQuit()
    {
        SavePlayerPosition();
        SaveProgressData();
    }

    private void OnDisable()
    {
        SavePlayerPosition();
        SaveProgressData();
    }

    // Salveaza toate datele de progres pentru utilizatorul curent
    public void SaveProgressData()
    {
        string currentUser = LoginManager.instance?.GetLoggedInUsername();
        if (string.IsNullOrEmpty(currentUser) || !users.ContainsKey(currentUser))
        {
            return;
        }

        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "MainMenu")
        {
            return; // Nu salveaza progres pentru meniu
        }

        if (!users[currentUser].Progress.Scenes.ContainsKey(currentScene))
        {
            users[currentUser].Progress.Scenes[currentScene] = new SceneData();
        }

        // Actualizeaza monedele din GameManager
        if (GameManager.instance != null)
        {
            int currentCoins = GameManager.instance.scoreCount;
            users[currentUser].Progress.Scenes[currentScene].Coins = currentCoins;
            UpdateCoins(currentCoins);
        }

        // Actualizeaza vietile din PlayerDamage
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerDamage playerDamage = player.GetComponent<PlayerDamage>();
            if (playerDamage != null)
            {
                int currentLives = playerDamage.GetLives();
                users[currentUser].Progress.Scenes[currentScene].Lives = currentLives;
                UpdateLives(currentLives);
            }
        }

        // Calculeaza timpul total pentru scena curenta
        float totalTimeForCurrentScene = sessionTimesPerScene.ContainsKey(currentScene)
            ? sessionTimesPerScene[currentScene]
            : 0f;

        users[currentUser].Progress.Scenes[currentScene].Time = totalTimeForCurrentScene;

        // Calculeaza si salveaza scorul si stelele pentru scenele de gameplay
        if (currentScene == "GamePlay" || currentScene == "GamePlayRomana")
        {
            var sceneData = users[currentUser].Progress.Scenes[currentScene];
            var scoreData = ScoreCalculator.CalculateSceneScore(sceneData);

            sceneData.FinalScore = scoreData.Score;
            sceneData.Stars = scoreData.Stars;
        }

        // Calculeaza totalurile pentru toate scenele
        int totalCoins = 0;
        int totalLives = 0;
        float totalTimeAcrossScenes = 0f;
        foreach (var scene in users[currentUser].Progress.Scenes.Values)
        {
            totalCoins += scene.Coins;
            totalLives += scene.Lives;
            totalTimeAcrossScenes += scene.Time;
        }

        // Actualizeaza progresul global
        users[currentUser].Progress.Coins = totalCoins;
        users[currentUser].Progress.Lives = totalLives;
        users[currentUser].Progress.Time = totalTimeAcrossScenes;

        SaveUsers();
    }
    // Obtine datele de scor calculate pentru o scena specifica
    public SceneScoreData GetSceneScoreData(string username, string sceneName)
    {
        if (users.ContainsKey(username) &&
            users[username].Progress.Scenes.ContainsKey(sceneName))
        {
            var sceneData = users[username].Progress.Scenes[sceneName];
            return ScoreCalculator.CalculateSceneScore(sceneData);
        }

        return new SceneScoreData();
    }

    // Recalculeaza toate scorurile pentru un utilizator
    public void RecalculateAllScores(string username)
    {
        if (!users.ContainsKey(username)) return;

        foreach (var kvp in users[username].Progress.Scenes)
        {
            string sceneName = kvp.Key;
            var sceneData = kvp.Value;

            // Recalculeaza doar pentru scenele de gameplay
            if (sceneName == "GamePlay" || sceneName == "GamePlayRomana")
            {
                var scoreData = ScoreCalculator.CalculateSceneScore(sceneData);
                sceneData.FinalScore = scoreData.Score;
                sceneData.Stars = scoreData.Stars;
            }
        }

        SaveUsers();
    }

    // Salveaza pozitia curenta a jucatorului din scena activa
    public void SavePlayerPosition()
    {
        string currentUser = LoginManager.instance?.GetLoggedInUsername();
        if (string.IsNullOrEmpty(currentUser) || !users.ContainsKey(currentUser))
        {
            return;
        }

        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "MainMenu")
        {
            return; // Nu salva pozitie pentru meniu
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            return;
        }

        Vector3 playerPosition = player.transform.position;

        SavePlayerPosition(currentUser, currentScene, playerPosition);
    }

    // Actualizeaza monedele pentru utilizatorul curent
    public void UpdateCoins(int coins)
    {
        string currentUser = LoginManager.instance?.GetLoggedInUsername();
        if (string.IsNullOrEmpty(currentUser) || !users.ContainsKey(currentUser))
        {
            return;
        }

        users[currentUser].Progress.Coins = coins;
        SaveUsers();
    }

    // Actualizeaza vietile pentru utilizatorul curent
    public void UpdateLives(int lives)
    {
        string currentUser = LoginManager.instance?.GetLoggedInUsername();
        if (string.IsNullOrEmpty(currentUser) || !users.ContainsKey(currentUser))
        {
            return;
        }

        users[currentUser].Progress.Lives = lives;
        SaveUsers();
    }

    // Actualizeaza timpul pentru utilizatorul curent
    public void UpdateTime(float time)
    {
        string currentUser = LoginManager.instance?.GetLoggedInUsername();
        if (string.IsNullOrEmpty(currentUser) || !users.ContainsKey(currentUser))
        {
            return;
        }

        users[currentUser].Progress.Time = time;
        SaveUsers();
    }

    // Obtine progresul complet al utilizatorului curent
    public UserProgress GetCurrentUserProgress()
    {
        string currentUser = LoginManager.instance?.GetLoggedInUsername();
        if (string.IsNullOrEmpty(currentUser) || !users.ContainsKey(currentUser))
        {
            return new UserProgress();
        }

        return users[currentUser].Progress;
    }

    // Salveaza tot progresul pentru utilizatorul curent
    public void SaveAllProgress(int coins, int lives, float time)
    {
        string currentUser = LoginManager.instance?.GetLoggedInUsername();
        if (string.IsNullOrEmpty(currentUser) || !users.ContainsKey(currentUser))
        {
            return;
        }

        users[currentUser].Progress.Coins = coins;
        users[currentUser].Progress.Lives = lives;
        users[currentUser].Progress.Time = time;
        SaveUsers();
    }

    // Restaureaza progresul pentru o scena specifica
    public void RestoreProgressForScene(string username, string sceneName)
    {
        if (!users.ContainsKey(username))
        {
            return;
        }

        if (sceneName == "GamePlay" || sceneName == "GamePlayRomana")
        {
            if (!users[username].Progress.Scenes.ContainsKey(sceneName))
            {
                return;
            }

            SceneData sceneData = users[username].Progress.Scenes[sceneName];

            // Restaureaza monedele in GameManager
            if (GameManager.instance != null)
            {
                GameManager.instance.scoreCount = sceneData.Coins;
                if (GameManager.instance.coinTextScore != null)
                    GameManager.instance.coinTextScore.text = "x" + sceneData.Coins;
            }

            // Restaureaza vietile si pozitia jucatorului
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                PlayerDamage playerDamage = player.GetComponent<PlayerDamage>();
                if (playerDamage != null)
                {
                    playerDamage.SetLives(sceneData.Lives);
                }
                player.transform.position = sceneData.LastFlagPosition.ToVector3();
            }
        }
    }

    // Metoda pentru afisarea timpului in consola (pentru debugging)
    public void DisplayTimeInConsole()
    {
        string currentUser = LoginManager.instance?.GetLoggedInUsername();
        if (string.IsNullOrEmpty(currentUser) || !users.ContainsKey(currentUser))
        {
            return;
        }

        float savedTime = users[currentUser].Progress.Time;
    }

    // Salveaza timpul de joc curent
    public void SaveCurrentGameplayTime(float gameplayTime)
    {
        string currentUser = LoginManager.instance?.GetLoggedInUsername();
        if (string.IsNullOrEmpty(currentUser) || !users.ContainsKey(currentUser))
        {
            return;
        }

        users[currentUser].Progress.Time = gameplayTime;
        SaveUsers();
    }

    // Delogarea utilizatorului si resetarea progresului
    public void LogoutUser()
    {
        string currentUser = LoginManager.instance?.GetLoggedInUsername();
        if (!string.IsNullOrEmpty(currentUser) && users.ContainsKey(currentUser))
        {
            users[currentUser].Progress = new UserProgress();
            SaveUsers();
        }
    }

    // Incarca progresul complet al jucatorului pentru o scena
    public PlayerProgressData LoadPlayerProgress(string username, string sceneName)
    {
        // Valori implicite
        PlayerProgressData data = new PlayerProgressData
        {
            Position = Vector3.zero,
            Coins = 0,
            Lives = 3
        };

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(sceneName))
            return data;

        if (users.ContainsKey(username) &&
            users[username].Progress.Scenes != null &&
            users[username].Progress.Scenes.ContainsKey(sceneName))
        {
            SceneData sceneData = users[username].Progress.Scenes[sceneName];
            data.Position = sceneData.LastFlagPosition.ToVector3();
            data.Coins = sceneData.Coins;
            data.Lives = sceneData.Lives;
        }
        return data;
    }

    // Reseteaza progresul pentru scena curenta la valorile initiale
    public void ResetProgressForCurrentScene(Vector3 initialPosition, int initialCoins, int initialLives, float initialTime)
    {
        string currentUser = LoginManager.instance?.GetLoggedInUsername();
        string currentScene = SceneManager.GetActiveScene().name;
        if (string.IsNullOrEmpty(currentUser) || !users.ContainsKey(currentUser))
            return;

        if (!users[currentUser].Progress.Scenes.ContainsKey(currentScene))
            users[currentUser].Progress.Scenes[currentScene] = new SceneData();

        var sceneData = users[currentUser].Progress.Scenes[currentScene];
        sceneData.LastFlagPosition = new SerializableVector3(initialPosition);
        sceneData.Coins = initialCoins;
        sceneData.Lives = initialLives;
        sceneData.Time = initialTime;

        SaveUsers();
    }
}

// Datele unui utilizator pentru serializare JSON
[System.Serializable]
public class UserData
{
    public string Username;
    public string Password;
    public UserProgress Progress;
}

// Progresul complet al unui utilizator
[System.Serializable]
public class UserProgress
{
    public Dictionary<string, SceneData> Scenes = new Dictionary<string, SceneData>();
    public int Coins;
    public int Lives;
    public float Time;
    public int rightAnswer = 0; // Total raspunsuri corecte
    public int wrongAnswer = 0; // Total raspunsuri gresite
    public List<string> AnsweredQuestions;

    public UserProgress()
    {
        Coins = 0;
        Lives = 3; // Numarul implicit de vieti
        Time = 0.0f;
        AnsweredQuestions = new List<string>();
    }
}

// Datele unei scene specifice
[System.Serializable]
public class SceneData
{
    public SerializableVector3 LastFlagPosition;
    public int Coins;
    public int Lives;
    public float Time;
    public List<string> AnsweredQuestions;

    // Scorurile calculate
    public float FinalScore = 0f;
    public int Stars = 1;

    // Statistici pe niveluri
    public LevelStats Level1 = new LevelStats();
    public LevelStats Level2 = new LevelStats();

    public SceneData()
    {
        LastFlagPosition = new SerializableVector3(0, 0, 0);
        Coins = 0;
        Lives = 3;
        Time = 0.0f;
        AnsweredQuestions = new List<string>();
        FinalScore = 0f;
        Stars = 1;
    }
}

// Structura pentru transferul datelor de progres
public struct PlayerProgressData
{
    public Vector3 Position;
    public int Coins;
    public int Lives;
}

// Vector3 serializabil pentru stocarea in JSON
[System.Serializable]
public class SerializableVector3
{
    public float x;
    public float y;
    public float z;

    public SerializableVector3() { }

    // Constructor din Vector3 Unity
    public SerializableVector3(Vector3 vector)
    {
        x = vector.x;
        y = vector.y;
        z = vector.z;
    }

    // Constructor cu coordonate individuale
    public SerializableVector3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    // Converteste inapoi la Vector3 Unity
    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }
}