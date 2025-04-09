using Assets.Scripts.User;
using UnityEngine.Networking;
using UnityEngine;
using System.Collections;
using System;

namespace Assets.Scripts.Utils
{
    public class SessionService : MonoBehaviour
    {
        public void CreateSession(int numberOfPlayers) {
            StartCoroutine(CreateSessionRequest(numberOfPlayers));
        }

        private IEnumerator CreateSessionRequest(int numberOfPlayers) {
            string endpoint = EndpointUtils.CreateSessions(numberOfPlayers);
            
            UnityWebRequest request = new(endpoint, "POST")
            {
                //uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonUtility.ToJson(form))),
                downloadHandler = new DownloadHandlerBuffer()
            };

            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log(request.downloadHandler.text);
                JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);
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
            string endpoint = EndpointUtils.JoinSession(sessionCode);

            UnityWebRequest request = new(endpoint, "POST")
            {
                //uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonUtility.ToJson(form))),
                downloadHandler = new DownloadHandlerBuffer()
            };

            request.SetRequestHeader("Content-Type", "application/json");

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
    }
}
