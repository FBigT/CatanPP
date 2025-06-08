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
        public long id;            // now matches {"id":...}
        public string code;
        public int maxPlayers;
        public string hostUserName;

        // keep your existing sessionId usage:
        public long sessionId => id;
    }

    public class SessionManager : MonoBehaviour
    {
        public static SessionManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public bool IsHost = false;
        public BoardGenBackendClient boardGenBackendClient;

        public void CreateSession(int maxPlayers, Action<SessionCodeDto> onSuccess, Action<string> onFail)
        {
            StartCoroutine(CreateSessionRequest(maxPlayers, onSuccess, onFail));
            IsHost = true;
        }

        private IEnumerator CreateSessionRequest(int maxPlayers, Action<SessionCodeDto> onSuccess, Action<string> onFail)
        {
            UnityWebRequest req = null;
            yield return RequestService.ConstructSimpleWebRequest(
                EndpointUtils.CreateSessions(maxPlayers),
                Methods.POST,
                true,
                null,
                result => req = result
            );
            if (req == null) { onFail?.Invoke("Failed to construct request"); yield break; }
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.Success)
            {
                var dto = JsonUtility.FromJson<SessionCodeDto>(req.downloadHandler.text);
                LocalStorageService.SetVariable("session-id", dto.sessionId.ToString());
                LocalStorageService.SetVariable("session-code", dto.code);
                onSuccess?.Invoke(dto);
                BoardGen.Instance?.GenerateAll();
            }
            else onFail?.Invoke(req.error);
        }

        public void JoinSession(string sessionCode, Action<SessionCodeDto> onSuccess, Action<string> onFail)
        {
            StartCoroutine(JoinSessionRequest(sessionCode, onSuccess, onFail));
            IsHost = false;
        }

        private IEnumerator JoinSessionRequest(
            string sessionCode,
            Action<SessionCodeDto> onSuccess,
            Action<string> onFail
        )
        {
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
                onFail?.Invoke("Failed to construct request");
                yield break;
            }

            // send the HTTP request
            yield return req.SendWebRequest();

            // *** ADD THIS DEBUG LINE ***
            Debug.Log($"[JoinSession] Raw JSON: {req.downloadHandler.text}");

            if (req.result == UnityWebRequest.Result.Success)
            {
                var dto = JsonUtility.FromJson<SessionCodeDto>(req.downloadHandler.text);
                LocalStorageService.SetVariable("session-id", dto.sessionId.ToString());
                LocalStorageService.SetVariable("session-code", dto.code);
                onSuccess?.Invoke(dto);
            }
            else
            {
                onFail?.Invoke(req.error);
            }
        }


        public void CloseSession(Action onSuccess, Action<string> onFail)
        {
            StartCoroutine(CloseSessionRequest(onSuccess, onFail));
            IsHost = false;
        }

        private IEnumerator CloseSessionRequest(Action onSuccess, Action<string> onFail)
        {
            UnityWebRequest req = null;
            yield return RequestService.ConstructSimpleWebRequest(
                EndpointUtils.CloseSession,
                Methods.POST,
                true,
                null,
                result => req = result
            );
            if (req == null) { onFail?.Invoke("Failed to construct request"); yield break; }
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.Success) onSuccess?.Invoke(); else onFail?.Invoke(req.error);
        }

        public void DeleteSessionSave(long id)
        {
            StartCoroutine(DeleteSessionSaveRequest(id));
            IsHost = false;
        }

        private IEnumerator DeleteSessionSaveRequest(long id)
        {
            UnityWebRequest req = null;
            yield return RequestService.ConstructSimpleWebRequest(
                EndpointUtils.DeleteSessionSave(id),
                Methods.POST,
                true,
                null,
                result => req = result
            );
            if (req != null) yield return req.SendWebRequest();
        }

        public void GetAllSessionSaves(Action<List<SessionSave>> onSuccess, Action<string> onFail)
        {
            StartCoroutine(GetAllSessionSavesRequests(onSuccess, onFail));
        }

        private IEnumerator GetAllSessionSavesRequests(Action<List<SessionSave>> onSuccess, Action<string> onFail)
        {
            UnityWebRequest req = null;
            yield return RequestService.ConstructSimpleWebRequest(
                EndpointUtils.GetSessionSaves,
                Methods.GET,
                true,
                null,
                result => req = result
            );
            if (req == null) { onFail?.Invoke("Failed to construct request"); yield break; }
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.Success)
            {
                var wrapper = JsonUtility.FromJson<SessionSaveList>(req.downloadHandler.text);
                onSuccess?.Invoke(wrapper.saves);
            }
            else onFail?.Invoke(req.error);
        }

        [Serializable]
        private class SessionSaveList { public List<SessionSave> saves; }
    }
}
