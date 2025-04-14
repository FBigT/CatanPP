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
        public void CreateSession(int numberOfPlayers) {
            StartCoroutine(CreateSessionRequest(numberOfPlayers));
        }

        private IEnumerator CreateSessionRequest(int numberOfPlayers) {
            UnityWebRequest request = RequestService.ConstructSimpleWebRequest(EndpointUtils.CreateSessions(numberOfPlayers), Methods.POST, true, null);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log(request.downloadHandler.text);
                yield return true;
            }
            else
            {
                Debug.LogError(request.error);
                yield return false;
            }
        }

        public void JoinSession(string sessionCode, Action<string> onSuccess, Action<string> onFail)
        {
            StartCoroutine(JoinSessionRequest(sessionCode, onSuccess, onFail));
        }

        private IEnumerator JoinSessionRequest(string sessionCode, Action<string> onSuccess, Action<string> onFail)
        {
            UnityWebRequest request = RequestService.ConstructSimpleWebRequest(EndpointUtils.JoinSession(sessionCode), Methods.POST, true, null);
            
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                onSuccess?.Invoke(request.downloadHandler.text);
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
            UnityWebRequest request = RequestService.ConstructSimpleWebRequest(EndpointUtils.DeleteSessionSave(id), Methods.DELETE, true, null);

            yield return request.SendWebRequest();
        }

        public void GetAllSessionSaves(Action<List<SessionSave>> onSuccess, Action<string> onFail)
        {
            StartCoroutine(GetAllSessionSavesRequests(onSuccess, onFail));
        }

        private IEnumerator GetAllSessionSavesRequests(Action<List<SessionSave>> onSuccess, Action<string> onFail)
        {
            UnityWebRequest request = RequestService.ConstructSimpleWebRequest(EndpointUtils.Save, Methods.GET, true, null);

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
