using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Assets.Scripts.Utils;

namespace Assets.Scripts.TradingReasources
{
    public static class JsonArrayHelper
    {
        [Serializable] private class Wrapper<T> { public T[] items; }

        public static List<T> FromJson<T>(string json)
        {
            var w = JsonUtility.FromJson<Wrapper<T>>($"{{\"items\":{json}}}");
            return w == null ? new List<T>() : new List<T>(w.items);
        }
    }

    public class TradeApiClient : MonoBehaviour
    {
        [Header("Backend URL")]
        public string baseUrl = "http://localhost:8080/api/trade";


        IEnumerator PostJson(string url, string json, Action<UnityWebRequest> done)
        {
            using var req = new UnityWebRequest(url, "POST")
            {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json)),
                downloadHandler = new DownloadHandlerBuffer()
            };
            req.SetRequestHeader("Content-Type", "application/json");


            yield return req.SendWebRequest();
            done?.Invoke(req);
        }

        public void TradePlayer(PlayerTradeDto dto, Action<bool, string> cb)
        {
            string j = JsonUtility.ToJson(dto);
            StartCoroutine(PostJson($"{baseUrl}/player", j,
                r => cb(r.result == UnityWebRequest.Result.Success, r.downloadHandler.text)));
        }

        public void TradeBank(BankTradeDto dto, Action<bool, string> cb)
        {
            string j = JsonUtility.ToJson(dto);
            StartCoroutine(PostJson($"{baseUrl}/bank", j,
                r => cb(r.result == UnityWebRequest.Result.Success, r.downloadHandler.text)));
        }

        public void GetSessionPlayers(long id, Action<List<SessionPlayerDto>, string> cb) =>
            StartCoroutine(GetPlayers(id, cb));

        IEnumerator GetPlayers(long id, Action<List<SessionPlayerDto>, string> cb)
        {
            string url = $"http://localhost:8080/api/session-players/session/{id}";
            using var req = UnityWebRequest.Get(url);


            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                var list = JsonArrayHelper.FromJson<SessionPlayerDto>(req.downloadHandler.text.Trim());
                cb(list, null);
            }
            else cb(null, req.error);
        }
    }
}
