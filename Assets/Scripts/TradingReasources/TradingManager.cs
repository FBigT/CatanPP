using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Assets.Scripts.Utils;
using Assets.Scripts.TradingReasources.Models;

namespace Assets.Scripts.TradingResources
{
    public class TradingManager : MonoBehaviour
    {
        public static TradingManager Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // ———————————————— Player-to-player trade ————————————————
        public void TradeWithPlayer(PlayerTradeDto dto, Action onSuccess, Action<string> onError)
            => StartCoroutine(TradePlayerCoroutine(dto, onSuccess, onError));

        private IEnumerator TradePlayerCoroutine(PlayerTradeDto trade, Action onSuccess, Action<string> onError)
        {
            var url = EndpointUtils.BaseUrl + "/api/trade/player";
            var json = JsonUtility.ToJson(trade);
            using var req = new UnityWebRequest(url, "POST")
            {
                uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json)),
                downloadHandler = new DownloadHandlerBuffer()
            };
            req.SetRequestHeader("Content-Type", "application/json");

            var token = LocalStorageService.GetString("token");
            if (!string.IsNullOrEmpty(token))
                req.SetRequestHeader("Authorization", $"Bearer {token}");

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success) onError?.Invoke(req.error);
            else onSuccess?.Invoke();
        }

        // ———————————————— Bank trade ————————————————
        public void TradeWithBank(BankTradeDto dto, Action onSuccess, Action<string> onError)
            => StartCoroutine(TradeBankCoroutine(dto, onSuccess, onError));

        private IEnumerator TradeBankCoroutine(BankTradeDto trade, Action onSuccess, Action<string> onError)
        {
            var url = EndpointUtils.BaseUrl + "/api/trade/bank";
            var json = JsonUtility.ToJson(trade);
            using var req = new UnityWebRequest(url, "POST")
            {
                uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json)),
                downloadHandler = new DownloadHandlerBuffer()
            };
            req.SetRequestHeader("Content-Type", "application/json");

            var token = LocalStorageService.GetString("token");
            if (!string.IsNullOrEmpty(token))
                req.SetRequestHeader("Authorization", $"Bearer {token}");

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success) onError?.Invoke(req.error);
            else onSuccess?.Invoke();
        }

        // ———————————————— Fetch players for dropdown ————————————————
        public void GetSessionPlayers(long sessionId, Action<List<SessionPlayerDto>> onSuccess, Action<string> onError)
            => StartCoroutine(GetSessionPlayersCoroutine(sessionId, onSuccess, onError));

        private IEnumerator GetSessionPlayersCoroutine(long sessionId,
                                                      Action<List<SessionPlayerDto>> onSuccess,
                                                      Action<string> onError)
        {
            if (sessionId <= 0)
            {
                onError?.Invoke("Invalid session ID");
                yield break;
            }

            var url = EndpointUtils.BaseUrl + $"/api/sessions/{sessionId}/players";
            using var req = UnityWebRequest.Get(url);

            var token = LocalStorageService.GetString("token");
            if (!string.IsNullOrEmpty(token))
                req.SetRequestHeader("Authorization", $"Bearer {token}");

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke(req.error);
            }
            else
            {
                var wrapper = JsonUtility.FromJson<SessionPlayerListDto>(req.downloadHandler.text);
                onSuccess?.Invoke(wrapper.players);
            }
        }

        [Serializable]
        private class SessionPlayerListDto
        {
            public List<SessionPlayerDto> players;
        }
    }
}
