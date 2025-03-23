using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;

public class UserManager : MonoBehaviour
{
    private string baseUrl = "http://localhost:8080/api/users"; // Change this if hosting elsewhere

    // Automatically runs when Unity starts
    void Start()
    {
        CreateUser("unity_player", 200); // Create a test user
        GetAllUsers(); // Fetch users from backend
    }

    // Create a new user
    public void CreateUser(string username, int score)
    {
        StartCoroutine(CreateUserRequest(username, score));
    }

    private IEnumerator CreateUserRequest(string username, int score)
    {
        string jsonData = "{\"username\": \"" + username + "\", \"score\": " + score + "}";

        UnityWebRequest request = new UnityWebRequest(baseUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonData));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("User Created: " + request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("Error Creating User: " + request.error);
        }
    }

    // Get all users
    public void GetAllUsers()
    {
        StartCoroutine(GetUsersRequest());
    }

    private IEnumerator GetUsersRequest()
    {
        UnityWebRequest request = UnityWebRequest.Get(baseUrl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Users: " + request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("Error Fetching Users: " + request.error);
        }
    }
}
