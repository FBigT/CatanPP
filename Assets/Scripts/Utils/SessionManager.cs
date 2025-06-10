using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Assets.Scripts.Utils;  // for RequestService, EndpointUtils, Methods, LocalStorageService
using Assets.Scripts.GameMode.Trading.Models; // for SessionCodeDto
using Assets.Scripts.MainMenu;
using System.Linq;
using Assets.Scripts.Dtos;
using Catan.TerrainGeneration;
using Newtonsoft.Json; // for SessionSave

namespace Assets.Scripts.Utils
{
    [Serializable]
    public class SessionCodeDto
    {
        public long id;
        public string code;
        public int maxPlayers;
        public string hostUserName;
        public long sessionId => id;
    }

    public class SessionManager : MonoBehaviour
    {
        public static SessionManager Instance { get; private set; }

        private void Awake()
        {
            Debug.Log("[SessionManager] Awake() called");
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Debug.Log("[SessionManager] ✅ Instance created and persisted");
            }
            else
            {
                Debug.Log("[SessionManager] ⚠️ Duplicate instance detected, destroying");
                Destroy(gameObject);
                return;
            }

            Debug.Log($"[SessionManager] 🏷️ Initial IsHost: {IsHost}");
        }

        public bool IsHost = false;
        public BoardGenBackendClient boardGenBackendClient;

        public void CreateSession(int maxPlayers, Action<SessionCodeDto> onSuccess, Action<string> onFail)
        {
            Debug.Log($"[SessionManager] 🏁 CreateSession called (maxPlayers={maxPlayers})");
            StartCoroutine(CreateSessionRequest(maxPlayers, onSuccess, onFail));
            IsHost = true;
            Debug.Log($"[SessionManager] 🏷️ IsHost set to: {IsHost} (host workflow)");
        }

        private IEnumerator CreateSessionRequest(int maxPlayers, Action<SessionCodeDto> onSuccess, Action<string> onFail)
        {
            Debug.Log("[SessionManager] ▶️ Starting CreateSessionRequest coroutine");
            UnityWebRequest req = null;

            yield return RequestService.ConstructSimpleWebRequest(
                EndpointUtils.CreateSessions(maxPlayers),
                Methods.POST,
                true,
                null,
                result => req = result
            );

            if (req == null)
            {
                Debug.LogError("[SessionManager] ❌ CreateSessionRequest – failed to construct request");
                onFail?.Invoke("Failed to construct request");
                yield break;
            }

            yield return req.SendWebRequest();
            Debug.Log($"[SessionManager] Response Status Code: {req.responseCode}");
            Debug.Log($"[SessionManager] Response Body: {req.downloadHandler.text}");

            if (req.result == UnityWebRequest.Result.Success)
            {
                var dto = JsonUtility.FromJson<SessionCodeDto>(req.downloadHandler.text);
                Debug.Log($"[SessionManager] ✅ Session created: id={dto.id}, code={dto.code}, host={dto.hostUserName}");
                LocalStorageService.SetVariable("session-id", dto.sessionId.ToString());
                LocalStorageService.SetVariable("session-code", dto.code);
                Debug.Log("[SessionManager] 💾 Stored session-id and session-code in localStorage");

                onSuccess?.Invoke(dto);

                // Trigger board generation on the host
                if (BoardGen.Instance != null)
                {
                    Debug.Log("[SessionManager] 🔄 Triggering host board generation (GenerateAll)");
                    if(!BoardGen.Instance.isGenerated)
                        BoardGen.Instance.GenerateAll();
                }
                else
                {
                    Debug.LogWarning("[SessionManager] ⚠️ BoardGen.Instance is null in CreateSessionRequest");
                }
            }
            else
            {
                Debug.LogError($"[SessionManager] ❌ CreateSessionRequest failed: {req.error}");
                onFail?.Invoke(req.error);
            }
        }

        public void JoinSession(string sessionCode, Action<SessionCodeDto> onSuccess, Action<string> onFail)
        {
            Debug.Log($"[SessionManager] 🏁 JoinSession called (code={sessionCode})");
            StartCoroutine(JoinSessionRequest(sessionCode, onSuccess, onFail));

            // Immediately set host flag false for joining client
            IsHost = false;
            Debug.Log($"[SessionManager] 🏷️ IsHost set to: {IsHost} (join workflow)");
        }

        private IEnumerator JoinSessionRequest(
            string sessionCode,
            Action<SessionCodeDto> onSuccess,
            Action<string> onFail
        )
        {
            Debug.Log("[SessionManager] ▶️ Starting JoinSessionRequest coroutine");
            UnityWebRequest req = null;

            yield return RequestService.ConstructSimpleWebRequest(
                EndpointUtils.JoinSession(sessionCode),
                Methods.POST,
                true,
                null,
                result => req = result
            );

            if (req == null)
            {
                Debug.LogError("[SessionManager] ❌ JoinSessionRequest – failed to construct request");
                onFail?.Invoke("Failed to construct request");
                yield break;
            }

            yield return req.SendWebRequest();
            Debug.Log($"[SessionManager] Response Status Code: {req.responseCode}");
            Debug.Log($"[SessionManager] Raw JSON: {req.downloadHandler.text}");

            if (req.result == UnityWebRequest.Result.Success)
            {
                var dto = JsonUtility.FromJson<SessionCodeDto>(req.downloadHandler.text);
                Debug.Log($"[SessionManager] ✅ Joined session: id={dto.id}, code={dto.code}, host={dto.hostUserName}");
                LocalStorageService.SetVariable("session-id", dto.sessionId.ToString());
                LocalStorageService.SetVariable("session-code", dto.code);
                Debug.Log("[SessionManager] 💾 Stored session-id and session-code in localStorage");

                // Invoke callback
                onSuccess?.Invoke(dto);

                // Log final host status before map logic
                Debug.Log($"[SessionManager] 🏷️ Final IsHost after join: {IsHost}");
            }
            else
            {
                Debug.LogError($"[SessionManager] ❌ JoinSessionRequest failed: {req.error}");
                onFail?.Invoke(req.error);
            }
        }

        public void CloseSession(Action onSuccess, Action<string> onFail)
        {
            Debug.Log("[SessionManager] 🏁 CloseSession called");
            StartCoroutine(CloseSessionRequest(onSuccess, onFail));
            IsHost = false;
            Debug.Log($"[SessionManager] 🏷️ IsHost set to: {IsHost} (close workflow)");
        }

        private IEnumerator CloseSessionRequest(Action onSuccess, Action<string> onFail)
        {
            Debug.Log("[SessionManager] ▶️ Starting CloseSessionRequest coroutine");
            UnityWebRequest req = null;

            yield return RequestService.ConstructSimpleWebRequest(
                EndpointUtils.CloseSession,
                Methods.POST,
                true,
                null,
                result => req = result
            );

            if (req == null)
            {
                Debug.LogError("[SessionManager] ❌ CloseSessionRequest – failed to construct request");
                onFail?.Invoke("Failed to construct request");
                yield break;
            }

            yield return req.SendWebRequest();
            Debug.Log($"[SessionManager] CloseSession response code: {req.responseCode}");

            if (req.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[SessionManager] ✅ Session closed successfully");
                onSuccess?.Invoke();
            }
            else
            {
                Debug.LogError($"[SessionManager] ❌ CloseSessionRequest failed: {req.error}");
                onFail?.Invoke(req.error);
            }
        }

        public void DeleteSessionSave(long id)
        {
            Debug.Log($"[SessionManager] 🗑️ DeleteSessionSave called (id={id})");
            StartCoroutine(DeleteSessionSaveRequest(id));
            IsHost = false;
            Debug.Log($"[SessionManager] 🏷️ IsHost set to: {IsHost} (delete save)");
        }

        private IEnumerator DeleteSessionSaveRequest(long id)
        {
            Debug.Log("[SessionManager] ▶️ Starting DeleteSessionSaveRequest coroutine");
            UnityWebRequest req = null;

            yield return RequestService.ConstructSimpleWebRequest(
                EndpointUtils.DeleteSessionSave(id),
                Methods.POST,
                true,
                null,
                result => req = result
            );

            if (req != null)
            {
                yield return req.SendWebRequest();
                Debug.Log($"[SessionManager] DeleteSessionSave response code: {req.responseCode}");
            }
            else
            {
                Debug.LogError("[SessionManager] ❌ DeleteSessionSaveRequest – failed to construct request");
            }
        }

        public void GetAllSessionSaves(Action<List<SessionSave>> onSuccess, Action<string> onFail)
        {
            Debug.Log("[SessionManager] 🏁 GetAllSessionSaves called");
            StartCoroutine(GetAllSessionSavesRequests(onSuccess, onFail));
        }

        private IEnumerator GetAllSessionSavesRequests(Action<List<SessionSave>> onSuccess, Action<string> onFail)
        {
            Debug.Log("[SessionManager] ▶️ Starting GetAllSessionSavesRequests coroutine");
            UnityWebRequest req = null;

            yield return RequestService.ConstructSimpleWebRequest(
                EndpointUtils.GetSessionSaves,
                Methods.GET,
                true,
                null,
                result => req = result
            );

            if (req == null)
            {
                Debug.LogError("[SessionManager] ❌ GetAllSessionSavesRequests – failed to construct request");
                onFail?.Invoke("Failed to construct request");
                yield break;
            }

            yield return req.SendWebRequest();
            Debug.Log($"[SessionManager] GetAllSessionSaves response code: {req.responseCode}");

            if (req.result == UnityWebRequest.Result.Success)
            {
                var wrapper = JsonUtility.FromJson<SessionSaveList>(req.downloadHandler.text);
                Debug.Log("[SessionManager] ✅ Successfully retrieved session saves");
                onSuccess?.Invoke(wrapper.saves);
            }
            else
            {
                Debug.LogError($"[SessionManager] ❌ GetAllSessionSavesRequests failed: {req.error}");
                onFail?.Invoke(req.error);
            }
        }

        [Serializable]
        private class SessionSaveList { public List<SessionSave> saves; }
    }
}
