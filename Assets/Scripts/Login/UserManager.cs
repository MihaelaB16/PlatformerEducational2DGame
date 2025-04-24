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

    void Awake()
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

        // Inițializează poziția pentru scena GamePlay
        newUser.Progress.Scenes["GamePlay"] = new SceneData
        {
            LastFlagPosition = new SerializableVector3(-10.0f, -3.0f, 0.0f) // Poziția implicită
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
    }

    private void OnDisable()
    {
        SavePlayerPosition();
    }

    private void SavePlayerPosition()
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
