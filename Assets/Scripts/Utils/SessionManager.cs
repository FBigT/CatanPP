using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Assets.Scripts.Utils;  // for RequestService, EndpointUtils, Methods, LocalStorageService
using Assets.Scripts.TradingReasources.Models; // for SessionCodeDto
using Assets.Scripts.MainMenu; // for SessionSave

namespace Assets.Scripts.Utils
{
    [Serializable]
    public class SessionCodeDto
    {
        public long sessionId;
        public string code;
        public int maxPlayers;
        public string hostUserName;
    }

    public class SessionManager : MonoBehaviour
    {
        public void CreateSession(int maxPlayers, Action<SessionCodeDto> onSuccess, Action<string> onFail)
        {
            StartCoroutine(CreateSessionRequest(maxPlayers, onSuccess, onFail));
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
            }
            else onFail?.Invoke(req.error);
        }

        public void JoinSession(string sessionCode, Action<SessionCodeDto> onSuccess, Action<string> onFail)
        {
            StartCoroutine(JoinSessionRequest(sessionCode, onSuccess, onFail));
        }

        private IEnumerator JoinSessionRequest(string sessionCode, Action<SessionCodeDto> onSuccess, Action<string> onFail)
        {
            UnityWebRequest req = null;
            yield return RequestService.ConstructSimpleWebRequest(
                EndpointUtils.JoinSession(sessionCode),
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
            }
            else onFail?.Invoke(req.error);
        }

        public void CloseSession(Action onSuccess, Action<string> onFail)
        {
            StartCoroutine(CloseSessionRequest(onSuccess, onFail));
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
