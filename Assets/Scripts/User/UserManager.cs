using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Assets.Scripts.User;
using Assets.Scripts.Utils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Assets.Scripts.Dtos;
using UnityEngine.InputSystem;

public class UserManager : MonoBehaviour
{

    private class RefreshRequest {
        public string refreshToken;

        public RefreshRequest(string refreshToken)
        {
            this.refreshToken = refreshToken;
        }
    }

    public void Login(LoginForm loginForm, Action<LoginResponse> onSuccess, Action<string> onFail)
    {
        if (string.IsNullOrWhiteSpace(loginForm.password))
        {
            onFail?.Invoke("Invalid password.");
            return;
        }

        if (string.IsNullOrWhiteSpace(loginForm.username))
        {
            onFail?.Invoke("Username invalid.");
            return;
        }

        StartCoroutine(LoginRequest(loginForm, onSuccess, onFail));
    }

    private IEnumerator LoginRequest(LoginForm form, Action<LoginResponse> onSuccess, Action<string> onFail)
    {
        UnityWebRequest request = null;
        string encrypted = SecurityUtils.EncryptRequest(JsonUtility.ToJson(form), out string encryptedKey, out byte[] aesKey);
        yield return RequestService.ConstructSimpleWebRequest(EndpointUtils.Login, Methods.POST, false, JsonUtility.ToJson(new EncryptedMessage(encryptedKey, encrypted)), result => request = result);

        if (request == null)
        {
            onFail?.Invoke("Failed to construct request");
            yield break;
        }

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            EncryptedResponse response = JsonUtility.FromJson<EncryptedResponse>(request.downloadHandler.text);
            string v = SecurityUtils.DecryptResponse(response.payload, aesKey); 
            onSuccess?.Invoke(JsonUtility.FromJson<LoginResponse>(v));
        }
        else
        {
            onFail?.Invoke(request.error);
        }
    }

    // Create a new user
    public void CreateUser(RegisterForm registerForm, Action onSuccess, Action<string> onFail)
    {
        if (string.IsNullOrWhiteSpace(registerForm.email) ||
            !registerForm.email.Contains("@") ||
            !registerForm.email.Contains("."))
        {
            onFail?.Invoke("Invalid email address.");
            return;
        }

        if (string.IsNullOrWhiteSpace(registerForm.password))
        {
            onFail?.Invoke("Invalid password.");
            return;
        }

        if (string.IsNullOrWhiteSpace(registerForm.username))
        {
            onFail?.Invoke("Username invalid.");
            return;
        }

        StartCoroutine(CreateUserRequest(registerForm, onSuccess, onFail));
    }

    private IEnumerator CreateUserRequest(RegisterForm form, Action onSuccess, Action<string> onFail)
    {
        UnityWebRequest request = null;
        string encrypted = SecurityUtils.EncryptRequest(JsonUtility.ToJson(form), out string encryptedKey, out byte[] aesKey);
        yield return RequestService.ConstructSimpleWebRequest(EndpointUtils.Register, Methods.POST, false, JsonUtility.ToJson(new EncryptedMessage(encryptedKey, encrypted)), result => request = result);

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
        byte[] key = SecurityUtils.createKey();
        string encryptedKey = SecurityUtils.EncryptKey(key);
        EncryptedMessage encrypted = new(encryptedKey, null);
        yield return RequestService.ConstructSimpleWebRequest(EndpointUtils.RegisterGuest, Methods.POST, false, JsonUtility.ToJson(encrypted), result => request = result);

        if (request == null)
        {
            onFail?.Invoke("Failed to construct request");
            yield break;
        }
        
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {

            EncryptedResponse response = JsonUtility.FromJson<EncryptedResponse>(request.downloadHandler.text);
            string v = SecurityUtils.DecryptResponse(response.payload, key);
            onSuccess?.Invoke(JsonUtility.FromJson<GuestRegisterResponse>(v));
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
        string encrypted = SecurityUtils.EncryptRequest(JsonUtility.ToJson(new RefreshRequest(guestCode)), out string encryptedKey, out byte[] aesKey);

        yield return RequestService.ConstructSimpleWebRequest(EndpointUtils.GuestLogin(), Methods.POST, false, JsonUtility.ToJson(new EncryptedMessage(encryptedKey, encrypted)), result => request = result);

        if (request == null)
        {
            onFail?.Invoke("Failed to construct request");
            yield break;
        }

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            EncryptedResponse response = JsonUtility.FromJson<EncryptedResponse>(request.downloadHandler.text);
            string v = SecurityUtils.DecryptResponse(response.payload, aesKey);
            onSuccess?.Invoke(JsonUtility.FromJson<LoginResponse>(v));
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
        string encrypted = SecurityUtils.EncryptRequest(JsonUtility.ToJson(new RefreshRequest(refreshToken)), out string encryptedKey, out byte[] aesKey);

        /*UnityWebRequest request = new(EndpointUtils.Refresh, Methods.POST.ToString())
        {
            uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes($"\"{refreshToken}\"" ?? "")),
            downloadHandler = new DownloadHandlerBuffer()
        };
        request.SetRequestHeader("Content-Type", "application/json");*/
        
        yield return RequestService.ConstructSimpleWebRequest(EndpointUtils.Refresh, Methods.POST, false, JsonUtility.ToJson(new EncryptedMessage(encryptedKey, encrypted)), result => request = result);

        if (request == null)
        {
            Debug.LogError("Request was null in RefreshTokenRequest");
            onFail?.Invoke("Failed to construct request");
            yield break;
        }

        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.Success)
        {
            EncryptedResponse response = JsonUtility.FromJson<EncryptedResponse>(request.downloadHandler.text);
            onSuccess?.Invoke(JsonUtility.FromJson<LoginResponse>(SecurityUtils.DecryptResponse(response.payload, aesKey)));
            yield break;
        }
        else
        {
            LocalStorageService.Clear("refresh-token");
            onFail?.Invoke(request.error);
            yield break;
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
