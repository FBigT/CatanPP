using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.Utils
{
    public static class RequestService
    {
        static UserManager userManager = new UserManager(); 

        #nullable enable
        public static IEnumerator ConstructSimpleWebRequest(string endpoint, Methods method, bool requiresAuthorization, string? jsonBody, Action<UnityWebRequest?> onReady) {
            UnityWebRequest request = new(endpoint, method.ToString())
            {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonUtility.ToJson(jsonBody))),
                downloadHandler = new DownloadHandlerBuffer()
            };
            request.SetRequestHeader("Content-Type", "application/json");

            if (requiresAuthorization) {
                if (string.IsNullOrEmpty(LocalStorageService.GetString("token")) || !SecurityUtils.IsTokenValid(LocalStorageService.GetString("token"))) {
                    if (!string.IsNullOrEmpty(LocalStorageService.GetString("refresh-token")))
                    {
                        yield return userManager.RefreshTokenRequest(LocalStorageService.GetString("refresh-token"), (response) => {
                            LocalStorageService.SetVariable("token", response.tokenType + " " + response.token);
                            LocalStorageService.SetVariable("refresh-token", response.refreshToken);
                            Debug.Log("REFRESH");
                        }, (error) => {
                            Debug.Log(error);
                            LocalStorageService.Clear();
                            onReady?.Invoke(null);
                        });
                    }
                }
                request.SetRequestHeader("Authorization", LocalStorageService.GetString("token"));
            }

            if (jsonBody != null) {
                try
                {
                    JsonUtility.FromJsonOverwrite(jsonBody, new object());
                }
                catch (Exception)
                {
                    onReady?.Invoke(null);
                }
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonBody));
            }
            onReady?.Invoke(request);
        }
        #nullable disable


    }
}
