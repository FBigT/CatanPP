using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Assets.Scripts.User;
using Assets.Scripts.Utils;
using System;
using System.Collections.Generic;

public class UserManager : MonoBehaviour
{
    public void Login(LoginForm loginForm, Action<LoginResponse> onSuccess, Action<string> onFail)
    {
        StartCoroutine(LoginRequest(loginForm, onSuccess, onFail));
    }

    private IEnumerator LoginRequest(LoginForm form, Action<LoginResponse> onSuccess, Action<string> onFail)
    {
        UnityWebRequest request = null;
        yield return RequestService.ConstructSimpleWebRequest(EndpointUtils.Login, Methods.POST, false, JsonUtility.ToJson(form), result => request = result);

        if (request == null)
        {
            onFail?.Invoke("Failed to construct request");
            yield break;
        }

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            LoginResponse response = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);
            onSuccess?.Invoke(response);
        }
        else
        {
            onFail?.Invoke(request.error);
        }
    }

    // Create a new user
    public void CreateUser(RegisterForm registerForm, Action onSuccess, Action<string> onFail)
    {
        StartCoroutine(CreateUserRequest(registerForm, onSuccess, onFail));
    }

    private IEnumerator CreateUserRequest(RegisterForm form, Action onSuccess, Action<string> onFail)
    {
        UnityWebRequest request = null;
        yield return RequestService.ConstructSimpleWebRequest(EndpointUtils.Register, Methods.POST, false, JsonUtility.ToJson(form), result => request = result);

        if (request == null)
        {
            onFail?.Invoke("Failed to construct request");
            yield break;
        }

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
        UnityWebRequest request = null;
        yield return RequestService.ConstructSimpleWebRequest(EndpointUtils.RegisterGuest, Methods.POST, false, null, result => request = result);

        if (request == null)
        {
            onFail?.Invoke("Failed to construct request");
            yield break;
        }
        
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
        UnityWebRequest request = null;
        yield return RequestService.ConstructSimpleWebRequest(EndpointUtils.GuestLogin(), Methods.POST, false, $"\"{guestCode}\"", result => request = result);

        if (request == null)
        {
            onFail?.Invoke("Failed to construct request");
            yield break;
        }

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

    public void RefreshToken(string refreshToken, Action<LoginResponse> onSuccess, Action<string> onFail)
    {
        StartCoroutine(RefreshTokenRequest(refreshToken, onSuccess, onFail));
    }

    public IEnumerator RefreshTokenRequest(string refreshToken, Action<LoginResponse> onSuccess, Action<string> onFail)
    {
        UnityWebRequest request = null;
        yield return RequestService.ConstructSimpleWebRequest(EndpointUtils.Refresh, Methods.POST, false, $"\"{refreshToken}\"", result => request = result);

        if (request == null)
        {
            onFail?.Invoke("Failed to construct request");
            yield break;
        }

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            LoginResponse loginResponse = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);
            onSuccess?.Invoke(loginResponse);
            yield break;
        }
        else
        {
            LocalStorageService.Clear("refresh-token");
            Debug.Log(request.result);
            Debug.LogError($"Response Body: {request.downloadHandler.text}");
            onFail?.Invoke(request.error);
            yield break;
        }
    }

    public void GetAllUsers(Action<List<UserDto>> onSuccess, Action<string> onFail)
    {
        StartCoroutine(GetUsersRequest(onSuccess, onFail));
    }

    private IEnumerator GetUsersRequest(Action<List<UserDto>> onSuccess, Action<string> onFail)
    {
        UnityWebRequest request = null;
        yield return RequestService.ConstructSimpleWebRequest(EndpointUtils.Users, Methods.GET, true, null, result => request = result);

        if (request == null)
        {
            yield break;
        }

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            List<UserDto> users = JsonUtility.FromJson<List<UserDto>>(request.downloadHandler.text);
            onSuccess?.Invoke(users);
        }
        else
        {
            onFail?.Invoke(request.error);
        }
    }

    public void GetPlayerProfileByUsername(string username, Action<PlayerProfile> onSuccess, Action<string> onFail) {
        StartCoroutine(GetPlayerProfileByUsernameRequest(username, onSuccess, onFail));
    }

    private IEnumerator GetPlayerProfileByUsernameRequest(string username, Action<PlayerProfile> onSuccess, Action<string> onFail) {
        UnityWebRequest request = null;
        yield return RequestService.ConstructSimpleWebRequest(EndpointUtils.GetPlayerPorfileByUsername(username), Methods.GET, true, null, result => request = result);

        if (request == null)
        {
            yield break;
        }

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

    public void GetCurrentPlayerProfile(Action<PlayerProfile> onSuccess, Action<string> onFail)
    {
        StartCoroutine(GetCurrentPlayerProfileRequest(onSuccess, onFail));
    }

    private IEnumerator GetCurrentPlayerProfileRequest(Action<PlayerProfile> onSuccess, Action<string> onFail)
    {
        UnityWebRequest request = null;
        yield return RequestService.ConstructSimpleWebRequest(EndpointUtils.Proflie, Methods.GET, true, null, result => request = result);

        if (request == null)
        {
            onFail?.Invoke("Failed to construct request");
            yield break;
        }
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

    public void GetPlayerProfileById(long id, Action<PlayerProfile> onSuccess, Action<string> onFail)
    {
        StartCoroutine(GetPlayerProfileByIdRequest(id, onSuccess, onFail));
    }

    private IEnumerator GetPlayerProfileByIdRequest(long id, Action<PlayerProfile> onSuccess, Action<string> onFail)
    {
        UnityWebRequest request = null;
        yield return RequestService.ConstructSimpleWebRequest(EndpointUtils.GetPlayerPorfileById(id), Methods.GET, true, null, result => request = result);

        if (request == null)
        {
            yield break;
        }

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
}
