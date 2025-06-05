using Assets.Scripts.Dtos;
using Assets.Scripts.Dtos.Board;
using Assets.Scripts.GameMode.Trading.Models; // for SessionCodeDto
using Assets.Scripts.MainMenu;
using Assets.Scripts.Utils;  // for RequestService, EndpointUtils, Methods, LocalStorageService
using Catan.TerrainGeneration;
using Newtonsoft.Json; // for SessionSave
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.Utils
{
    [Serializable]
    public class SessionCodeDto
    {
        public long id;            // now matches {"id":...}
        public string code;
        public int maxPlayers;
        public string hostUserName;

        // keep your existing sessionId usage:
        public long sessionId => id;
    }

    public class SessionManager : MonoBehaviour
    {
        public bool IsHost { get; private set; } = false;

        public void CreateSession(int maxPlayers, Action<SessionCodeDto> onSuccess, Action<string> onFail)
        {
            StartCoroutine(CreateSessionRequest(maxPlayers, dto =>
            {
                IsHost = true; // This player is host/creator
                if (BoardGen.Instance != null)
                    BoardGen.Instance.generateBoardOnStart = true; // Generate board on start

                onSuccess?.Invoke(dto);
            }, onFail));
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

        public IEnumerator JoinSession(string sessionCode, Action<SessionCodeDto> onSuccess, Action<string> onFail)
        {
            UnityWebRequest req = null;
            yield return RequestService.ConstructSimpleWebRequest(
                EndpointUtils.JoinSession(sessionCode),
                Methods.POST,
                true,
                null,
                result => req = result
            );

            if (req == null)
            {
                onFail?.Invoke("Failed to construct request");
                yield break;
            }

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                var dto = JsonUtility.FromJson<SessionCodeDto>(req.downloadHandler.text);

                LocalStorageService.SetVariable("session-id", dto.sessionId.ToString());
                LocalStorageService.SetVariable("session-code", dto.code);

                IsHost = false; // This player joined, not host

                // Disable board generation on start for joiners
                if (BoardGen.Instance != null)
                    BoardGen.Instance.generateBoardOnStart = false;

                // Load the map from backend instead of generating
                GetMapState(
                    tileList =>
                    {
                        if (BoardGen.Instance != null)
                        {
                            BoardGen.Instance.BuildBoardFromTiles(tileList);
                        }
                        else
                        {
                            Debug.LogError("[JoinSession] BoardGen instance is null!");
                        }
                    },
                    error => Debug.LogError("[JoinSession] Failed to load map: " + error)
                );

                onSuccess?.Invoke(dto);
            }
            else
            {
                onFail?.Invoke(req.error);
            }
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

        public void GetMapState(Action<List<TileDto>> onSuccess, Action<string> onFail)
        {
            StartCoroutine(GetMapStateRequest(onSuccess, onFail));
        }

        private IEnumerator GetMapStateRequest(Action<List<TileDto>> onSuccess, Action<string> onFail)
        {
            UnityWebRequest req = null;
            yield return RequestService.ConstructSimpleWebRequest(
                EndpointUtils.GetMapState,
                Methods.GET,
                true,
                null,
                result => req = result
            );

            if (req == null)
            {
                onFail?.Invoke("Failed to construct request");
                yield break;
            }

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    // Deserialize JSON into List<TileDto>
                    var tileDtos = JsonConvert.DeserializeObject<List<TileDto>>(req.downloadHandler.text);
                    onSuccess?.Invoke(tileDtos);
                }
                catch (Exception ex)
                {
                    onFail?.Invoke("Deserialization error: " + ex.Message);
                }
            }
            else
            {
                onFail?.Invoke(req.error);
            }
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
