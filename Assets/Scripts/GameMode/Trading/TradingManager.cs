// Assets/Scripts/GameMode/Trading/TradingManager.cs
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Assets.Scripts.Utils;
using Assets.Scripts.GameMode.Trading.Models;

namespace Assets.Scripts.GameMode.Trading
{
    /// <summary>
    /// Manages REST calls for session players and trades, keeps auth fresh,
    /// exposes <c>OnPlayersLoaded</c> so other systems (StructurePlacer, etc.)
    /// can cache their <c>sessionPlayerId</c>.
    /// </summary>
    public class TradingManager : MonoBehaviour
    {
        /* ────────────────────────── singleton ───────────────────────── */
        public static TradingManager Instance { get; private set; }
        /* ---------- NEW: let other scripts know when players arrive ---------- */
        public static event Action<List<SessionPlayerDto>> OnPlayersLoaded = delegate { };
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        

        /* ────────────────────────── public API ───────────────────────── */
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
            StartCoroutine(TradeRoutine(EndpointUtils.TradeWithPlayer,
                                        JsonUtility.ToJson(dto),
                                        onSuccess, onError));
        }

        public void TradeWithBank(BankTradeDto dto,
                                  Action onSuccess,
                                  Action<string> onError)
        {
            StartCoroutine(TradeRoutine(EndpointUtils.TradeWithBank,
                                        JsonUtility.ToJson(dto),
                                        onSuccess, onError));
        }

        /* ───────────────────────‑ internals ‑────────────────────────── */

        #region Auth refresh
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

            var body = System.Text.Encoding.UTF8.GetBytes($"\"{refresh}\"");
            using UnityWebRequest req = new UnityWebRequest(EndpointUtils.Refresh, "POST")
            {
                uploadHandler = new UploadHandlerRaw(body),
                downloadHandler = new DownloadHandlerBuffer()
            };
            req.SetRequestHeader("Content-Type", "application/json");

            Debug.Log("[TokenCheck] Attempting to refresh token...");
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                AuthResponse resp = JsonUtility.FromJson<AuthResponse>(req.downloadHandler.text);
                string newToken = resp.tokenType + " " + resp.token;

                LocalStorageService.SetVariable("token", newToken);
                LocalStorageService.SetVariable("refresh-token", resp.refreshToken);

                Debug.Log("[TokenCheck] Token refresh successful.");
            }
            else
            {
                Debug.LogError("[TokenCheck] Token refresh failed: " + req.error);
            }
        }
        #endregion

        #region GET players
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
                yield break;
            }

            string url = EndpointUtils.GetSessionPlayers(sessionId);
            using UnityWebRequest req = UnityWebRequest.Get(url);
            req.SetRequestHeader("Authorization", jwt);

            Debug.Log($"[GetPlayersRoutine] GET {url}");
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[GetPlayersRoutine] {req.error} ({req.responseCode})");
                onError?.Invoke(req.error);
                yield break;
            }

            Debug.Log("[GetPlayersRoutine] Response: " + req.downloadHandler.text);

            try
            {
                SessionPlayerDto[] arr =
                    JsonHelper.FromJson<SessionPlayerDto>(req.downloadHandler.text);
                var list = new List<SessionPlayerDto>(arr);

                onSuccess?.Invoke(list);
                /* >>> notify subscribers (StructurePlacer, etc.) <<< */
                OnPlayersLoaded?.Invoke(list);
            }
            catch (Exception ex)
            {
                Debug.LogError("[GetPlayersRoutine] JSON parse error: " + ex.Message);
                onError?.Invoke("Failed to parse player data.");
            }
        }
        #endregion

        #region POST trades
        IEnumerator TradeRoutine(string url,
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

            using UnityWebRequest req = new UnityWebRequest(url, "POST")
            {
                uploadHandler = new UploadHandlerRaw(
                                    System.Text.Encoding.UTF8.GetBytes(jsonBody)),
                downloadHandler = new DownloadHandlerBuffer()
            };
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("Authorization", jwt);

            Debug.Log($"[TradeRoutine] POST {url} – payload: {jsonBody}");
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[TradeRoutine] {req.error} ({req.responseCode})");
                onError?.Invoke(req.error);
            }
            else
            {
                Debug.Log("[TradeRoutine] Trade successful.");
                onSuccess?.Invoke();
            }
        }
        #endregion

        /* ──────────────────── DTOs for internal use ─────────────────── */
        [Serializable]
        class AuthResponse
        {
            public string tokenType;
            public string token;
            public string refreshToken;
        }
    }
}
