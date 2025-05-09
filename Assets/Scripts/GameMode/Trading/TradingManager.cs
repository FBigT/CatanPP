using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Assets.Scripts.Utils;
using Assets.Scripts.GameMode.Trading.Models;

namespace Assets.Scripts.GameMode.Trading
{
    public class TradingManager : MonoBehaviour
    {
        public static TradingManager Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // PUBLIC API
        public void GetSessionPlayers(long sessionId,
                                      Action<List<SessionPlayerDto>> onSuccess,
                                      Action<string> onError)
        {
            StartCoroutine(GetPlayersRoutine(sessionId, onSuccess, onError));
        }

        public void TradeWithPlayer(PlayerTradeDto dto,
                                    Action onSuccess,
                                    Action<string> onError)
        {
            StartCoroutine(TradeRoutine(EndpointUtils.TradeWithPlayer, JsonUtility.ToJson(dto), onSuccess, onError));
        }

        public void TradeWithBank(BankTradeDto dto,
                                  Action onSuccess,
                                  Action<string> onError)
        {
            StartCoroutine(TradeRoutine(EndpointUtils.TradeWithBank, JsonUtility.ToJson(dto), onSuccess, onError));
        }

        // INTERNAL HELPERS

        IEnumerator EnsureValidToken()
        {
            string jwt = LocalStorageService.GetString("token");
            string refresh = LocalStorageService.GetString("refresh-token");

            Debug.Log($"[TokenCheck] Existing JWT: {jwt}");
            Debug.Log($"[TokenCheck] Refresh token: {refresh}");

            if (SecurityUtils.IsTokenValid(jwt))
            {
                Debug.Log("[TokenCheck] JWT is still valid.");
                yield break;
            }

            if (string.IsNullOrEmpty(refresh))
            {
                Debug.LogError("[TokenCheck] No refresh token available.");
                yield break;
            }

            string url = EndpointUtils.Refresh;
            var requestBody = System.Text.Encoding.UTF8.GetBytes($"\"{refresh}\"");

            using UnityWebRequest req = new UnityWebRequest(url, "POST")
            {
                uploadHandler = new UploadHandlerRaw(requestBody),
                downloadHandler = new DownloadHandlerBuffer()
            };

            req.SetRequestHeader("Content-Type", "application/json");

            Debug.Log("[TokenCheck] Attempting to refresh token...");
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                var resp = JsonUtility.FromJson<AuthResponse>(req.downloadHandler.text);
                string newToken = resp.tokenType + " " + resp.token;

                LocalStorageService.SetVariable("token", newToken);
                LocalStorageService.SetVariable("refresh-token", resp.refreshToken);

                Debug.Log($"[TokenCheck] Token refresh successful. New JWT: {newToken}");
            }
            else
            {
                Debug.LogError("[TokenCheck] Token refresh failed: " + req.error);
                yield break;
            }
        }

        IEnumerator GetPlayersRoutine(long sessionId,
                                     Action<List<SessionPlayerDto>> onSuccess,
                                     Action<string> onError)
        {
            if (sessionId <= 0)
            {
                onError?.Invoke("Invalid session ID");
                yield break;
            }

            yield return StartCoroutine(EnsureValidToken());

            string jwt = LocalStorageService.GetString("token");

            if (!SecurityUtils.IsTokenValid(jwt))
            {
                onError?.Invoke("User not authenticated (token invalid)");
                Debug.LogError("[GetPlayersRoutine] Token invalid even after refresh.");
                yield break;
            }

            string url = EndpointUtils.GetSessionPlayers(sessionId);
            using UnityWebRequest req = UnityWebRequest.Get(url);
            req.SetRequestHeader("Authorization", jwt);

            Debug.Log($"[GetPlayersRoutine] Sending GET to {url} with Authorization: {jwt}");

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[GetPlayersRoutine] Request failed: {req.error}, Status Code: {req.responseCode}");
                onError?.Invoke(req.error);
            }
            else
            {
                Debug.Log("[GetPlayersRoutine] Request successful. Response: " + req.downloadHandler.text);

                try
                {
                    SessionPlayerDto[] playerArray = JsonHelper.FromJson<SessionPlayerDto>(req.downloadHandler.text);
                    var playerList = new List<SessionPlayerDto>(playerArray);
                    onSuccess?.Invoke(playerList);
                }
                catch (Exception ex)
                {
                    Debug.LogError("[GetPlayersRoutine] JSON parse error: " + ex.Message);
                    onError?.Invoke("Failed to parse player data.");
                }
            }
        }

        IEnumerator TradeRoutine(string fullUrl,
                                 string jsonBody,
                                 Action onSuccess,
                                 Action<string> onError)
        {
            yield return StartCoroutine(EnsureValidToken());

            string jwt = LocalStorageService.GetString("token");

            if (!SecurityUtils.IsTokenValid(jwt))
            {
                onError?.Invoke("User not authenticated (token invalid)");
                yield break;
            }

            using UnityWebRequest req = new UnityWebRequest(fullUrl, "POST")
            {
                uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonBody)),
                downloadHandler = new DownloadHandlerBuffer()
            };

            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("Authorization", jwt);

            Debug.Log($"[TradeRoutine] Sending POST to {fullUrl} with payload: {jsonBody}");
            Debug.Log($"[TradeRoutine] Authorization: {jwt}");

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[TradeRoutine] Request failed: {req.error}, Status Code: {req.responseCode}");
                onError?.Invoke(req.error);
            }
            else
            {
                Debug.Log("[TradeRoutine] Trade successful.");
                onSuccess?.Invoke();
            }
        }

        [Serializable]
        class AuthResponse
        {
            public string tokenType;
            public string token;
            public string refreshToken;
        }

        [Serializable]
        class PlayerWrap
        {
            public List<SessionPlayerDto> players;
        }
    }
}
