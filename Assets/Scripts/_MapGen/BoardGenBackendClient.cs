using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System;
using Assets.Scripts.Dtos;
using Assets.Scripts.Utils;

public class BoardGenBackendClient : MonoBehaviour
{
    public static BoardGenBackendClient Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public async void SendBoardData(List<HexTile> tilesRaw)
    {
        List<TileDto> tiles = new List<TileDto>();

        foreach (var hex in tilesRaw)
        {
            tiles.Add(new TileDto(hex.Q, hex.R, hex.resourceType, hex.numberToken,0));
        }

        var ggm = new GenerateMapDto(tiles);
        var gm = new GameMoveDto(ggm);

        await WebSocketService.SendMapData(gm);
    }

    public async void SendBoardData(List<HexTile> tilesRaw, Action<string> onSuccess = null, Action<string> onFail = null)
    {
        List<TileDto> tiles = new List<TileDto>();

        foreach (var hex in tilesRaw)
        {
            tiles.Add(new TileDto(hex.Q, hex.R, hex.resourceType, hex.numberToken,0));
        }

        var ggm = new GenerateMapDto(tiles);
        var gm = new GameMoveDto(ggm);

        await WebSocketService.SendMapData(gm);
    }
}
