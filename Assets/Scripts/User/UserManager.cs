using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Assets.Scripts.User;
using Assets.Scripts.Utils;
using System;

public class UserManager : MonoBehaviour
{
    public void Login(LoginForm loginForm, Action<LoginResponse> onSuccess, Action<string> onFail)
    {
        StartCoroutine(LoginRequest(loginForm, onSuccess, onFail));
    }

    private IEnumerator LoginRequest(LoginForm form, Action<LoginResponse> onSuccess, Action<string> onFail)
    {
        UnityWebRequest request = RequestService.ConstructSimpleWebRequest(EndpointUtils.Login, Methods.POST, false, JsonUtility.ToJson(form));

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log(request.downloadHandler.text);
            LoginResponse response = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);
            
        }
        else
        {
            Debug.LogError(request.error);
        }
    }

    // Create a new user
    public void CreateUser(RegisterForm registerForm, Action onSuccess, Action<string> onFail)
    {
        StartCoroutine(CreateUserRequest(registerForm, onSuccess, onFail));
    }

    private IEnumerator CreateUserRequest(RegisterForm form, Action onSuccess, Action<string> onFail)
    {
        UnityWebRequest request = RequestService.ConstructSimpleWebRequest(EndpointUtils.Register, Methods.POST, false, JsonUtility.ToJson(form));
        
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            onSuccess?.Invoke();
        }
        else
        {
            onFail?.Invoke(request.error);
        }
    }

    public void CreateGuest(Action<GuestRegisterResponse> onSuccess, Action<string> onFail)
    {
        StartCoroutine(CreateGuestRequest(onSuccess, onFail));
    }

    private IEnumerator CreateGuestRequest(Action<GuestRegisterResponse> onSuccess, Action<string> onFail)
    {
        UnityWebRequest request = RequestService.ConstructSimpleWebRequest(EndpointUtils.RegisterGuest, Methods.POST, false, null);
        
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            GuestRegisterResponse guestResponse = JsonUtility.FromJson<GuestRegisterResponse>(request.downloadHandler.text);
            onSuccess?.Invoke(guestResponse);
        }
        else
        {
            onFail?.Invoke(request.error);
        }
    }

    public void GuestLogin(string guestCode, Action<LoginResponse> onSuccess, Action<string> onFail)
    {
        StartCoroutine(GuestLoginRequest(guestCode, onSuccess, onFail));
    }

    private IEnumerator GuestLoginRequest(string guestCode, Action<LoginResponse> onSuccess, Action<string> onFail)
    {
        UnityWebRequest request = RequestService.ConstructSimpleWebRequest(EndpointUtils.GuestLogin(), Methods.POST, false, JsonUtility.ToJson(guestCode));

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            LoginResponse loginResponse = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);
            onSuccess?.Invoke(loginResponse);
        }
        else
        {
            onFail?.Invoke(request.error);
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
        UnityWebRequest request = RequestService.ConstructSimpleWebRequest(EndpointUtils.GetPlayerPorfileByUsername(username), Methods.GET, true, null);
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

    public void GetCurrentPlayerProfile(Action<PlayerProfile> onSuccess, Action<string> onFail)
    {
        StartCoroutine(GetCurrentPlayerProfileRequest(onSuccess, onFail));
    }

    private IEnumerator GetCurrentPlayerProfileRequest(Action<PlayerProfile> onSuccess, Action<string> onFail)
    {
        UnityWebRequest request = RequestService.ConstructSimpleWebRequest(EndpointUtils.Proflie, Methods.GET, true, null);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            PlayerProfile playerProfile = JsonUtility.FromJson<PlayerProfile>(request.downloadHandler.text);
            onSuccess?.Invoke(playerProfile);
        }
        else
        {
            onFail?.Invoke(request.error);
        }
    }

    public void GetPlayerProfileById(long id)
    {
        StartCoroutine(GetPlayerProfileByIdRequest(id));
    }

    private IEnumerator GetPlayerProfileByIdRequest(long id)
    {
        UnityWebRequest request = RequestService.ConstructSimpleWebRequest(EndpointUtils.GetPlayerPorfileById(id), Methods.GET, true, null);
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
