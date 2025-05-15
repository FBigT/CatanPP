using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Assets.Scripts.Utils;    // for RequestService, Methods, LocalStorageService
using Assets.Scripts.Models;   // for DevCardDto

public static class DevCardEndpoints
{
    // static readonly can be initialized at runtime
    private static readonly string Base = EndpointUtils.BaseUrl + "/devcards";
    public static string Buy() => $"{Base}/buy";
    public static string List(long pid) => $"{Base}/player/{pid}";
    public static string Use(long cardId) => $"{Base}/use/{cardId}";
}

public class DevCardService : MonoBehaviour
{
    // POST /api/devcards/buy
    public IEnumerator Buy(Action<DevCardDto> onSuccess, Action<string> onError = null)
    {
        UnityWebRequest req = null;
        yield return RequestService.ConstructSimpleWebRequest(
            DevCardEndpoints.Buy(),
            Methods.POST,
            true,     // needs Authorization header
            null,     // no body
            r => req = r
        );
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            var card = JsonUtility.FromJson<DevCardDto>(req.downloadHandler.text);
            onSuccess?.Invoke(card);
        }
        else
        {
            onError?.Invoke(req.error);
        }
    }

    // GET /api/devcards/player/{playerId}
    public IEnumerator List(long playerId, Action<List<DevCardDto>> onSuccess, Action<string> onError = null)
    {
        UnityWebRequest req = null;
        yield return RequestService.ConstructSimpleWebRequest(
            DevCardEndpoints.List(playerId),
            Methods.GET,
            true,
            null,
            r => req = r
        );
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            // Wrap array into an object so JsonUtility can parse it
            string wrapped = "{\"cards\":" + req.downloadHandler.text + "}";
            var wrapper = JsonUtility.FromJson<DevCardDtoArray>(wrapped);
            onSuccess?.Invoke(new List<DevCardDto>(wrapper.cards));
        }
        else
        {
            onError?.Invoke(req.error);
        }
    }

    // POST /api/devcards/use/{cardId}
    public IEnumerator Use(long cardId, Action<DevCardDto> onSuccess, Action<string> onError = null)
    {
        UnityWebRequest req = null;
        yield return RequestService.ConstructSimpleWebRequest(
            DevCardEndpoints.Use(cardId),
            Methods.POST,
            true,
            null,
            r => req = r
        );
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            var used = JsonUtility.FromJson<DevCardDto>(req.downloadHandler.text);
            onSuccess?.Invoke(used);
        }
        else
        {
            onError?.Invoke(req.error);
        }
    }

    // Helper wrapper for List parsing
    [Serializable]
    private class DevCardDtoArray
    {
        public DevCardDto[] cards;
    }
}
