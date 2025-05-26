// Assets/Scripts/Utils/RequestService.cs
using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.Utils
{
    /// <summary>
    /// Helper for building/sending UnityWebRequests.
    /// – No longer instantiates a MonoBehaviour with `new` –
    ///   we create one hidden GameObject once and run coroutines there.
    /// </summary>
    public static class RequestService
    {
        /* ------------------------------------------------------------
         *  MonoBehaviour host for coroutines
         * ---------------------------------------------------------- */
        private sealed class RequestServiceHost : MonoBehaviour { }
        private static RequestServiceHost _host;          // created lazily
        private static UserManager _userManager;   // lives on that host

        private static void EnsureHost()
        {
            if (_host != null) return;

            var go = new GameObject("[RequestService]");
            UnityEngine.Object.DontDestroyOnLoad(go);
            _host = go.AddComponent<RequestServiceHost>();
            _userManager = go.AddComponent<UserManager>();     // legit AddComponent!
        }

        /* ------------------------------------------------------------
         *  Public helper
         * ---------------------------------------------------------- */
#nullable enable
        public static IEnumerator ConstructSimpleWebRequest(
            string endpoint,
            Methods method,
            bool requiresAuthorization,
            string? jsonBody,
            Action<UnityWebRequest?> onReady)
        {
            EnsureHost();

            var request = new UnityWebRequest(endpoint, method.ToString())
            {
                uploadHandler = new UploadHandlerRaw(Array.Empty<byte>()),
                downloadHandler = new DownloadHandlerBuffer()
            };
            request.SetRequestHeader("Content-Type", "application/json");

            /* ---------- JWT handling ---------- */
            if (requiresAuthorization)
            {
                yield return _host.StartCoroutine(
                    EnsureValidToken(success =>
                    {
                        if (!success)
                        {
                            onReady?.Invoke(null);
                        }
                    }));

                if (onReady == null) yield break;   // aborted above

                request.SetRequestHeader("Authorization",
                                         LocalStorageService.GetString("token"));
            }

            /* ---------- payload ---------- */
            if (!string.IsNullOrEmpty(jsonBody))
                request.uploadHandler = new UploadHandlerRaw(
                    Encoding.UTF8.GetBytes(jsonBody));

            onReady?.Invoke(request);
        }
#nullable disable

        /* ------------------------------------------------------------
         *  Internal – make sure our JWT is still good
         * ---------------------------------------------------------- */
        private static IEnumerator EnsureValidToken(Action<bool> after)
        {
            string jwt = LocalStorageService.GetString("token");
            string refresh = LocalStorageService.GetString("refresh-token");

            if (SecurityUtils.IsTokenValid(jwt))
            {
                after?.Invoke(true);
                yield break;
            }

            if (string.IsNullOrEmpty(refresh))
            {
                after?.Invoke(false);
                yield break;
            }

            bool done = false;
            bool ok = false;
            yield return _host.StartCoroutine(
                _userManager.RefreshTokenRequest(refresh,
                    resp => {
                        LocalStorageService.SetVariable("token",
                            resp.tokenType + " " + resp.token);
                        LocalStorageService.SetVariable("refresh-token",
                            resp.refreshToken);
                        ok = true;
                        done = true;
                    },
                    err => { done = true; }
                ));
            while (!done) yield return null;
            after?.Invoke(ok);
        }
    } 
}