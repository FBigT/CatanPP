using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.Utils
{
    public static class RequestService
    {
        #nullable enable
        public static UnityWebRequest? ConstructSimpleWebRequest(string endpoint, Methods method, bool requiresAuthorization, string? jsonBody) {
            UnityWebRequest request = new(endpoint, method.ToString())
            {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonUtility.ToJson(jsonBody))),
                downloadHandler = new DownloadHandlerBuffer()
            };
            request.SetRequestHeader("Content-Type", "application/json");

            if (requiresAuthorization) {
                if (LocalStorageService.GetString("token") == null) return null; 
                request.SetRequestHeader("Authorization", LocalStorageService.GetString("token"));
            }

            if (jsonBody != null) {
                try
                {
                    JsonUtility.FromJsonOverwrite(jsonBody, new object());
                }
                catch (System.Exception)
                {
                    return null;
                }
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonBody));
            }
            return request;
        }
        #nullable disable
    }
}
