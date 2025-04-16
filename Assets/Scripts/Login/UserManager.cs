using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public class UserManager : MonoBehaviour
{
    private string userFilePath;
    private Dictionary<string, UserData> users;

    void Awake()
    {
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
            Progress = new UserProgress() // Inițializare cu constructorul implicit
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
            if (progress.LastFlagPosition == null)
            {
                progress.LastFlagPosition = new SerializableVector3(0, 0, 0);
            }
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
            progress.LastFlagPosition = new SerializableVector3(FlagController.lastFlagPosition); // Conversie
            users[username].Progress = progress;
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
    public SerializableVector3 LastFlagPosition;
    public int Coins;
    public int Lives;
    public float Time;
    public List<string> AnsweredQuestions;

    public UserProgress()
    {
        LastFlagPosition = new SerializableVector3(0, 0, 0); // Poziție implicită
        Coins = 0;
        Lives = 3; // Număr implicit de vieți
        Time = 0.0f;
        AnsweredQuestions = new List<string>(); // Listă goală
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
