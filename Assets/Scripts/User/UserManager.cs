using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using Assets.Scripts.User;
using Assets.Scripts.Utils;

public class UserManager : MonoBehaviour
{
    public void Login(string username, string password)
    {
        StartCoroutine(LoginRequest(new LoginForm(username, password)));
    }

    private IEnumerator LoginRequest(LoginForm form)
    {
        UnityWebRequest request = new(EndpointUtils.Login, "POST")
        {
            uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonUtility.ToJson(form))),
            downloadHandler = new DownloadHandlerBuffer()
        };

        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log(request.downloadHandler.text);
            LoginResponse response = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);
            PlayerPrefs.SetString("token", response.Token);
        }
        else
        {
            Debug.LogError(request.error);
        }
    }

    // Create a new user
    public void CreateUser(string username, string email, string password)
    {
        StartCoroutine(CreateUserRequest(new RegisterForm(username, email, password)));
    }

    private IEnumerator CreateUserRequest(RegisterForm form)
    {
        string v = JsonUtility.ToJson(form);
        UnityWebRequest request = new(EndpointUtils.Register, "POST")
        {
            uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonUtility.ToJson(form))),
            downloadHandler = new DownloadHandlerBuffer()
        };
        
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

    public void CreateGuest()
    {
        StartCoroutine(CreateGuestRequest());
    }

    private IEnumerator CreateGuestRequest()
    {
        UnityWebRequest request = new(EndpointUtils.RegisterGuest, "POST")
        {
            downloadHandler = new DownloadHandlerBuffer()
        };

        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log(request.downloadHandler.text);
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
        UnityWebRequest request = UnityWebRequest.Get(EndpointUtils.Users);
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

    public void GetPlayerProfileByUsername(string username) {
        StartCoroutine(GetPlayerProfileByUsernameRequest(username));
    }

    private IEnumerator GetPlayerProfileByUsernameRequest(string username) {
        UnityWebRequest request = UnityWebRequest.Get(EndpointUtils.GetPlayerPorfileByUsername(username));
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log(request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("Error Fetching Users: " + request.error);
        }
    }

    public void GetPlayerProfileById(long id)
    {
        StartCoroutine(GetPlayerProfileByIdRequest(id));
    }

    private IEnumerator GetPlayerProfileByIdRequest(long id)
    {
        UnityWebRequest request = UnityWebRequest.Get(EndpointUtils.GetPlayerPorfileById(id));
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log(request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("Error Fetching Users: " + request.error);
        }
    }
}
