using UnityEngine.Networking;
using UnityEngine;
using System.Collections;
using System;
using Assets.Scripts.MainMenu;
using System.Collections.Generic;

namespace Assets.Scripts.Utils
{
    public class SessionManager : MonoBehaviour
    {
        public void CreateSession(int numberOfPlayers, Action<SessionCodeDto> onSuccess, Action<string> onFail) {
            StartCoroutine(CreateSessionRequest(numberOfPlayers, onSuccess, onFail));
        }

        private IEnumerator CreateSessionRequest(int numberOfPlayers, Action<SessionCodeDto> onSuccess, Action<string> onFail) {
            UnityWebRequest request = null;
            yield return RequestService.ConstructSimpleWebRequest(EndpointUtils.CreateSessions(numberOfPlayers), Methods.POST, true, null, result => request = result);

            if (request == null)
            {
                onFail?.Invoke("Failed to construct request");
                yield break;
            }

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                SessionCodeDto sessionCodeDto = JsonUtility.FromJson<SessionCodeDto>(request.downloadHandler.text);
                onSuccess?.Invoke(sessionCodeDto);
            }
            else
            {
                onFail?.Invoke(request.error);
            }
        }

        public void CloseSession(Action onSuccess, Action<string> onFail)
        {
            StartCoroutine(CloseSessionRequest(onSuccess, onFail));
        }

        private IEnumerator CloseSessionRequest(Action onSuccess, Action<string> onFail)
        {
            UnityWebRequest request = null;
            yield return RequestService.ConstructSimpleWebRequest(EndpointUtils.CloseSession, Methods.POST, true, null, result => request = result);

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

        public void JoinSession(string sessionCode, Action<SessionCodeDto> onSuccess, Action<string> onFail)
        {
            StartCoroutine(JoinSessionRequest(sessionCode, onSuccess, onFail));
        }

        private IEnumerator JoinSessionRequest(string sessionCode, Action<SessionCodeDto> onSuccess, Action<string> onFail)
        {

            UnityWebRequest request = null;
            yield return RequestService.ConstructSimpleWebRequest(EndpointUtils.JoinSession(sessionCode), Methods.POST, true, null, result => request = result);

            if (request == null)
            {
                onFail?.Invoke("Failed to construct request");
                yield break;
            }

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                SessionCodeDto sessionCodeDto = JsonUtility.FromJson<SessionCodeDto>(request.downloadHandler.text);
                onSuccess?.Invoke(sessionCodeDto);
            }
            else
            {
                onFail?.Invoke(request.error);
            }
        }

        public void DeleteSessionSave(long id)
        {
            StartCoroutine(DeleteSessionSaveRequest(id));
        }

        private IEnumerator DeleteSessionSaveRequest(long id)
        {
            UnityWebRequest request = null;
            yield return RequestService.ConstructSimpleWebRequest(EndpointUtils.DeleteSessionSave(id), Methods.POST, true, null, result => request = result);

            if (request == null)
            {
                yield break;
            }

            yield return request.SendWebRequest();
        }

        public void GetAllSessionSaves(Action<List<SessionSave>> onSuccess, Action<string> onFail)
        {
            StartCoroutine(GetAllSessionSavesRequests(onSuccess, onFail));
        }

        private IEnumerator GetAllSessionSavesRequests(Action<List<SessionSave>> onSuccess, Action<string> onFail)
        {
            UnityWebRequest request = null;
            yield return RequestService.ConstructSimpleWebRequest(EndpointUtils.Save, Methods.POST, true, null, result => request = result);

            if (request == null)
            {
                onFail?.Invoke("Failed to construct request");
                yield break;
            }

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                List<SessionSave> sessionSaves = JsonUtility.FromJson<List<SessionSave>>(request.downloadHandler.text);
                onSuccess?.Invoke(sessionSaves);
            }
            else
            {
                onFail?.Invoke(request.error);
            }
        }
    }
}
