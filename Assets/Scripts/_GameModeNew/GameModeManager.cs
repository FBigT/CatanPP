using Assets.Scripts.Dtos;
using Assets.Scripts.Dtos.GameMoveResponses;
using Assets.Scripts.Enums;
using Assets.Scripts.User;
using Assets.Scripts.Utils;
using Catan.GameMode;
using Gamemode.New;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Gamemode.New
{
    public class GameModeManager : MonoBehaviour
    {
        public static GameModeManager Instance { get; private set; }

        [Header("System Settings")]
        public int maxConnectRequests = 50;
        public int awaitTimeInMilliseconds = 100;

        [Header("Game Settings")]
        public int maxPlayers = 4;

        private Dictionary<string, PlayerState> playerStates = new();
        public string CurrentPlayer { get; private set; }
        public int CurrentTurn { get; private set; } = 1;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);  // Prevent duplicates
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            WaitForWebSocketConnection();
        }

        private void OnEnable()
        {
            WebSocketService.OnChatMessageReceived += OnChatMessage;
            WebSocketService.OnTradeOfferReceived += OnTradeOffer;
            WebSocketService.OnTradeResponseReceived += OnTradeResponse;
            WebSocketService.OnChatMessageReceived += OnChatMessage;
        }

        private async void WaitForWebSocketConnection()
        {
            int attempts = 0;

            while (!WebSocketService.Connected && attempts < maxConnectRequests)
            {
                await Task.Delay(awaitTimeInMilliseconds);
                attempts++;
            }

            if (WebSocketService.Connected)
            {
                Debug.Log("[GameModeManager] WebSocket is now connected.");
            }
            else
            {
                Debug.LogError("[GameModeManager] WebSocket connection timeout.");
            }
        }


        public void RegisterPlayer(string username)
        {
            if (playerStates.ContainsKey(username)) return;
            if (playerStates.Count >= maxPlayers)
            {
                Debug.LogWarning("Too many players.");
                return;
            }

            playerStates[username] = new PlayerState
            {
                username = username,
                resources = new ResourceGroup(),
                devCards = new List<Assets.Scripts.Models.DevCardType>()
            };

            Debug.Log($"Registered: {username}");
        }

        // here are handled the dev cards in this manager, dont alter anything else

        //private void OnBuyCardResponse(BuyCardResponseDto card)
        //{
        //    Debug.Log($"[GameModeManager] Player {card.username} bought a card");

        //    if (playerStates.TryGetValue(card.username, out var state))
        //    {
        //        state.devCards.Add(card.cardType);
        //    }
        //}

        //private void OnPlayCardPlayed(PlayCardResponseDto play)
        //{
        //    Debug.Log($"[GameModeManager] Player {play.username} played a card: {play.devCardType}");
        //    // Update turn, state, or map depending on card type
        //}

        private void OnTradeOffer(TradeOfferMessage offer)
        {
            Debug.Log($"[GameModeManager] Trade offer: {offer.fromUser} → {offer.toUser}");
        }

        private void OnTradeResponse(TradeResponseMessage response)
        {
            Debug.Log($"[GameModeManager] Trade response from {response.fromUser}: accepted = {response.accepted}");
        }

        private void OnChatMessage(ChatMessage msg)
        {
            Debug.Log($"[GameModeManager] Chat: {msg.senderUsername}: {msg.text}");
        }

        public bool IsPlayerTurn(string username)
        {
            return username == CurrentPlayer;
        }

        public void StartNextTurn()
        {
            var playerList = playerStates.Keys.ToList();
            if (playerList.Count == 0) return;

            int currentIndex = playerList.IndexOf(CurrentPlayer);
            int nextIndex = (currentIndex + 1) % playerList.Count;

            CurrentPlayer = playerList[nextIndex];
            CurrentTurn++;
            Debug.Log($"Turn {CurrentTurn} - {CurrentPlayer}'s turn");
        }
    }
}
