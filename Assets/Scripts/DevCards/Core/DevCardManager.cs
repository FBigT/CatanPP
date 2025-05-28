using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Utils;
using Assets.Scripts.Models;
using Assets.Scripts.Dtos.GameMoves;
using Assets.Scripts.Dtos;
using Assets.Scripts.Enums;
using Assets.Scripts.GameMode.Trading.Models;
using System;
using System.Linq;

namespace Assets.Scripts.DevCards.Core
{
    public class DevCardManager : MonoBehaviour
    {
        public static DevCardManager Instance { get; private set; }

        [Header("Dev Cards")]
        public List<DevCardDto> playerCards = new List<DevCardDto>();

        // Events for UI
        public event Action<List<DevCardDto>> OnCardsUpdated;
        public event Action<string> OnCardBought;
        public event Action<string> OnError;

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

        private void Start()
        {
            LoadPlayerCards();
        }

        public void BuyDevCard()
        {
            var gameMove = new GameMoveDto(GameMoveType.BUY_CARD);
            string json = JsonUtility.ToJson(gameMove);
            WebSocketService.SendMessage(json);
        }

        // Main method that both UI controllers can use
        public void PlayDevCard(DevCardDto card, DevCardType type)
        {
            switch (type)
            {
                case DevCardType.KNIGHT:
                    PlayKnight(card);
                    break;
                case DevCardType.VICTORY_POINT:
                    PlayVictoryPoint(card);
                    break;
                case DevCardType.ROAD_BUILDING:
                    PlayRoadBuilding(card);
                    break;
                case DevCardType.YEAR_OF_PLENTY:
                    PlayYearOfPlenty(card);
                    break;
            }

            // Mark card as played and update UI
            OnCardPlayed(card);
        }

        // Method that UI controllers expect to exist
        public void OnCardPlayed(DevCardDto card)
        {
            card.used = true;
            OnCardsUpdated?.Invoke(playerCards);
            Debug.Log($"Card played: {card.type}");
        }

        private void PlayKnight(DevCardDto card)
        {
            var robberMove = new RobberMoveDto
            {
                originatingTileX = 0,
                originatingTileY = 0,
                destinationTileX = 1,
                destinationTileY = 1
            };

            var playCardDto = new PlayCardDto(robberMove);
            var gameMove = new GameMoveDto(GameMoveType.PLAY_CARD);
            gameMove.moveData = playCardDto;

            string json = JsonUtility.ToJson(gameMove);
            WebSocketService.SendMessage(json);

            Debug.Log("Knight card played - move robber");
        }

        private void PlayVictoryPoint(DevCardDto card)
        {
            var playCardDto = new PlayCardDto(DevCardType.VICTORY_POINT);
            var gameMove = new GameMoveDto(GameMoveType.PLAY_CARD);
            gameMove.moveData = playCardDto;

            string json = JsonUtility.ToJson(gameMove);
            WebSocketService.SendMessage(json);

            Debug.Log("Victory Point card played");
        }

        private void PlayRoadBuilding(DevCardDto card)
        {
            var road1 = new PlaceRoadDto(0, 0, 0);
            var road2 = new PlaceRoadDto(0, 0, 1);

            var playCardDto = new PlayCardDto(DevCardType.ROAD_BUILDING);
            var gameMove = new GameMoveDto(GameMoveType.PLAY_CARD);
            gameMove.moveData = playCardDto;

            string json = JsonUtility.ToJson(gameMove);
            WebSocketService.SendMessage(json);

            Debug.Log("Road Building card played");
        }

        private void PlayYearOfPlenty(DevCardDto card)
        {
            var resourceGroup = new ResourceGroup();
            resourceGroup.wood = 1;
            resourceGroup.brick = 1;

            var playCardDto = new PlayCardDto(resourceGroup);
            var gameMove = new GameMoveDto(GameMoveType.PLAY_CARD);
            gameMove.moveData = playCardDto;

            string json = JsonUtility.ToJson(gameMove);
            WebSocketService.SendMessage(json);

            Debug.Log("Year of Plenty card played - got wood and brick");
        }

        public List<DevCardDto> GetPlayerCards()
        {
            return new List<DevCardDto>(playerCards);
        }

        public List<DevCardDto> GetCardsByType(DevCardType type)
        {
            return playerCards.Where(c => c.type == type).ToList();
        }

        private void LoadPlayerCards()
        {
            // For testing, create some sample cards
            playerCards = new List<DevCardDto>
            {
                new DevCardDto { id = 1, type = DevCardType.KNIGHT, playable = true, used = false },
                new DevCardDto { id = 2, type = DevCardType.VICTORY_POINT, playable = true, used = false },
                new DevCardDto { id = 3, type = DevCardType.ROAD_BUILDING, playable = true, used = false },
                new DevCardDto { id = 4, type = DevCardType.YEAR_OF_PLENTY, playable = true, used = false }
            };

            OnCardsUpdated?.Invoke(playerCards);
        }
    }
}
