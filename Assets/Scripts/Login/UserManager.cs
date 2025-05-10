using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;

public class UserManager : MonoBehaviour
{
    public static UserManager instance;
    private string userFilePath;
    private Dictionary<string, UserData> users;

    private float currentSessionTime = 0f;
    private float jsonTimeGamePlay = 0f;
    private float jsonTimeGamePlayRomana = 0f;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this; // Assign the current instance
            DontDestroyOnLoad(gameObject); // Optional: Keep this instance across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }

        userFilePath = Path.Combine(Application.persistentDataPath, "users.json");
        LoadUsers();
    }


    void Update()
    {
        currentSessionTime += Time.deltaTime; // Increment session time
    }


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

            // Inițializează structura JSON dacă lipsește
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
    void SaveUsers()
    {
        string json = JsonConvert.SerializeObject(users, Formatting.Indented);
        File.WriteAllText(userFilePath, json);
    }

    public void InitializeGameWithUserProgress(string username)
    {
        if (!users.ContainsKey(username))
        {
            Debug.LogWarning($"User '{username}' not found. Cannot initialize game state.");
            return;
        }

        string currentScene = SceneManager.GetActiveScene().name;

        if (!users[username].Progress.Scenes.ContainsKey(currentScene))
        {
            Debug.LogWarning($"No progress found for user '{username}' in scene '{currentScene}'.");
            return;
        }

        SceneData sceneData = users[username].Progress.Scenes[currentScene];

        // Set the coins in GameManager
        if (GameManager.instance != null)
        {
            GameManager.instance.scoreCount = sceneData.Coins;
            if (GameManager.instance.coinTextScore != null)
            {
                GameManager.instance.coinTextScore.text = "x" + sceneData.Coins;
            }

            GameManager.instance.ResetGameplayTime();
        }

        // Initialize time variables
        if (currentScene == "GamePlay")
        {
            jsonTimeGamePlay = sceneData.Time;
        }
        else if (currentScene == "GamePlayRomana")
        {
            jsonTimeGamePlayRomana = sceneData.Time;
        }

        currentSessionTime = 0f; // Reset session time
    }


    public bool RegisterUser(string username, string password)
    {
        if (users.ContainsKey(username))
        {
            return false; // User already exists
        }

        UserData newUser = new UserData
        {
            Username = username,
            Password = password,
            Progress = new UserProgress()
        };

        // Inițializează pozițiile și progresul pentru ambele scene
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


    public bool LoginUser(string username, string password, out UserProgress progress)
    {
        if (users.ContainsKey(username) && users[username].Password == password)
        {
            progress = users[username].Progress;

            // Inițializăm proprietățile dacă sunt null
            if (progress.AnsweredQuestions == null)
            {
                progress.AnsweredQuestions = new List<string>();
            }

            return true;
        }

        progress = null;
        return false;
    }

    public void SaveProgress(string username, UserProgress progress)
    {
        if (users.ContainsKey(username))
        {
            users[username].Progress = progress;
            SaveUsers();
        }
    }

    public void SavePlayerPosition(string username, string sceneName, Vector3 position)
    {
        Debug.Log($"Saving position for user '{username}' in scene '{sceneName}': {position}");
        UserManager.instance.DisplayTimeInConsole();
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
        else
        {
            Debug.LogWarning($"User '{username}' not found in the dictionary.");
        }
    }

    public Vector3 LoadPlayerPosition(string username, string sceneName)
    {
        if (string.IsNullOrEmpty(username))
        {
            Debug.LogError("Username is null or empty. Cannot load player position.");
            return Vector3.zero;
        }

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Scene name is null or empty. Cannot load player position.");
            return Vector3.zero;
        }

        if (users.ContainsKey(username) &&
            users[username].Progress.Scenes != null &&
            users[username].Progress.Scenes.ContainsKey(sceneName))
        {
            Debug.Log($"Loaded position for user '{username}' in scene '{sceneName}': {users[username].Progress.Scenes[sceneName].LastFlagPosition}");
            return users[username].Progress.Scenes[sceneName].LastFlagPosition.ToVector3();
        }

        Debug.LogWarning($"No saved position found for user '{username}' in scene '{sceneName}'. Returning default position.");
        return Vector3.zero;
    }


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

    public void SaveProgressData()
    {
        string currentUser = LoginManager.instance?.GetLoggedInUsername();
        if (string.IsNullOrEmpty(currentUser) || !users.ContainsKey(currentUser))
        {
            Debug.LogWarning("No valid logged-in user found or user does not exist in the dictionary.");
            return;
        }

        string currentScene = SceneManager.GetActiveScene().name;

        if (!users[currentUser].Progress.Scenes.ContainsKey(currentScene))
        {
            users[currentUser].Progress.Scenes[currentScene] = new SceneData();
        }


        // Get current score from GameManager
        if (GameManager.instance != null)
        {
            int currentCoins = GameManager.instance.scoreCount;
            users[currentUser].Progress.Scenes[currentScene].Coins = currentCoins;
            UpdateCoins(currentCoins); // Update total coins
        }

        // Get current lives from PlayerDamage
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerDamage playerDamage = player.GetComponent<PlayerDamage>();
            if (playerDamage != null)
            {
                int currentLives = playerDamage.GetLives();
                users[currentUser].Progress.Scenes[currentScene].Lives = currentLives;
                UpdateLives(currentLives); // Update total lives
            }
        }

        // Calculate total time for the current scene
        float totalTimeForCurrentScene = currentSessionTime;
        if (currentScene == "GamePlay")
        {
            totalTimeForCurrentScene += jsonTimeGamePlay;
        }
        else if (currentScene == "GamePlayRomana")
        {
            totalTimeForCurrentScene += jsonTimeGamePlayRomana;
        }

        // Save the total time in JSON for the current scene
        users[currentUser].Progress.Scenes[currentScene].Time = totalTimeForCurrentScene;

        // Calculate the total time across all scenes
        int totalCoins = 0;
        int totalLives = 0;
        float totalTimeAcrossScenes = 0f;
        foreach (var scene in users[currentUser].Progress.Scenes.Values)
        {
            totalCoins += scene.Coins;
            totalLives += scene.Lives;
            totalTimeAcrossScenes += scene.Time;
        }

        // Save the totals in the user's overall progress
        users[currentUser].Progress.Coins = totalCoins;
        users[currentUser].Progress.Lives = totalLives;
        users[currentUser].Progress.Time = totalTimeAcrossScenes;

        // Save the updated data
        SaveUsers();

        Debug.Log($"Saved progress data for user '{currentUser}': Total Time for Current Scene={totalTimeForCurrentScene}, Total Time Across Scenes={totalTimeAcrossScenes}");
    }




    public void SavePlayerPosition()
    {
        string currentUser = LoginManager.instance?.GetLoggedInUsername();
        if (string.IsNullOrEmpty(currentUser) || !users.ContainsKey(currentUser))
        {
            Debug.LogWarning("No valid logged-in user found or user does not exist in the dictionary.");
            return;
        }

        string currentScene = SceneManager.GetActiveScene().name;

        // Găsește jucătorul în scenă
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("Player object not found in the scene. Cannot save position.");
            return;
        }

        // Obține poziția jucătorului
        Vector3 playerPosition = player.transform.position;

        Debug.Log($"Saving position for user '{currentUser}' in scene '{currentScene}': {playerPosition}");
        SavePlayerPosition(currentUser, currentScene, playerPosition);
    }
    public void UpdateCoins(int coins)
    {
        string currentUser = LoginManager.instance?.GetLoggedInUsername();
        if (string.IsNullOrEmpty(currentUser) || !users.ContainsKey(currentUser))
        {
            Debug.LogWarning("No valid logged-in user found or user does not exist in the dictionary.");
            return;
        }

        users[currentUser].Progress.Coins = coins;
        SaveUsers();
        Debug.Log($"Updated coins for user '{currentUser}': {coins}");
    }

    // Update lives for the current user
    public void UpdateLives(int lives)
    {
        string currentUser = LoginManager.instance?.GetLoggedInUsername();
        if (string.IsNullOrEmpty(currentUser) || !users.ContainsKey(currentUser))
        {
            Debug.LogWarning("No valid logged-in user found or user does not exist in the dictionary.");
            return;
        }

        users[currentUser].Progress.Lives = lives;
        SaveUsers();
        Debug.Log($"Updated lives for user '{currentUser}': {lives}");
    }

    // Update time for the current user
    public void UpdateTime(float time)
    {
        string currentUser = LoginManager.instance?.GetLoggedInUsername();
        if (string.IsNullOrEmpty(currentUser) || !users.ContainsKey(currentUser))
        {
            Debug.LogWarning("No valid logged-in user found or user does not exist in the dictionary.");
            return;
        }

        users[currentUser].Progress.Time = time;
        SaveUsers();
        Debug.Log($"Updated time for user '{currentUser}': {time}");
    }



    // Get the current user's progress data
    public UserProgress GetCurrentUserProgress()
    {
        string currentUser = LoginManager.instance?.GetLoggedInUsername();
        if (string.IsNullOrEmpty(currentUser) || !users.ContainsKey(currentUser))
        {
            Debug.LogWarning("No valid logged-in user found or user does not exist in the dictionary.");
            return new UserProgress();
        }

        return users[currentUser].Progress;
    }

    // Save all progress data for current user
    public void SaveAllProgress(int coins, int lives, float time)
    {
        string currentUser = LoginManager.instance?.GetLoggedInUsername();
        if (string.IsNullOrEmpty(currentUser) || !users.ContainsKey(currentUser))
        {
            Debug.LogWarning("No valid logged-in user found or user does not exist in the dictionary.");
            return;
        }

        users[currentUser].Progress.Coins = coins;
        users[currentUser].Progress.Lives = lives;
        users[currentUser].Progress.Time = time;
        SaveUsers();
        Debug.Log($"Saved all progress for user '{currentUser}': Coins={coins}, Lives={lives}, Time={time}");
    }


    // Add this method to UserManager.cs
    public void DisplayTimeInConsole()
    {
        string currentUser = LoginManager.instance?.GetLoggedInUsername();
        if (string.IsNullOrEmpty(currentUser) || !users.ContainsKey(currentUser))
        {
            Debug.LogWarning("No valid logged-in user found or user does not exist in the dictionary.");
            return;
        }

        float savedTime = users[currentUser].Progress.Time;

        Debug.Log($"==============================================");
        Debug.Log($"Current user: {currentUser}");
        Debug.Log($"Total elapsed time: {savedTime.ToString("F2")} seconds");
        Debug.Log($"That's approximately {(savedTime / 60).ToString("F2")} minutes");
        Debug.Log($"==============================================");
    }
    public void SaveCurrentGameplayTime(float gameplayTime)
    {
        string currentUser = LoginManager.instance?.GetLoggedInUsername();
        if (string.IsNullOrEmpty(currentUser) || !users.ContainsKey(currentUser))
        {
            Debug.LogWarning("No valid logged-in user found or user does not exist in the dictionary.");
            return;
        }

        // Store the gameplayTime directly (don't add to previous time)
        users[currentUser].Progress.Time = gameplayTime;
        SaveUsers();
        Debug.Log($"Updated gameplay time for user '{currentUser}': {gameplayTime.ToString("F2")} seconds");
    }
    public void LogoutUser()
    {
        Debug.Log("Logging out the current user and resetting state.");
        currentSessionTime = 0f;
        jsonTimeGamePlay = 0f;
        jsonTimeGamePlayRomana = 0f;

        string currentUser = LoginManager.instance?.GetLoggedInUsername();
        if (!string.IsNullOrEmpty(currentUser) && users.ContainsKey(currentUser))
        {
            users[currentUser].Progress = new UserProgress();
            SaveUsers();
        }
    }
}


[System.Serializable]
public class UserData
{
    public string Username;
    public string Password;
    public UserProgress Progress;
}

[System.Serializable]
public class UserProgress
{
    public Dictionary<string, SceneData> Scenes = new Dictionary<string, SceneData>();
    public int Coins;
    public int Lives;
    public float Time;
    public List<string> AnsweredQuestions;

    public UserProgress()
    {
        Coins = 0;
        Lives = 3; // Număr implicit de vieți
        Time = 0.0f;
        AnsweredQuestions = new List<string>(); // Listă goală
    }
}

[System.Serializable]
public class SceneData
{
    public SerializableVector3 LastFlagPosition;
    public int Coins;
    public int Lives;
    public float Time;
    public List<string> AnsweredQuestions;

    public SceneData()
    {
        LastFlagPosition = new SerializableVector3(0, 0, 0);
        Coins = 0;
        Lives = 3; // Număr implicit de vieți
        Time = 0.0f;
        AnsweredQuestions = new List<string>();
    }
}



[System.Serializable]
public class SerializableVector3
{
    public float x;
    public float y;
    public float z;

    public SerializableVector3() { }

    public SerializableVector3(Vector3 vector)
    {
        x = vector.x;
        y = vector.y;
        z = vector.z;
    }

    // Constructor nou care acceptă trei argumente float
    public SerializableVector3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }
}
