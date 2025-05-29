using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Utils;
using Assets.Scripts.Models;
using Assets.Scripts.Dtos.GameMoves;
using Assets.Scripts.Dtos;
using Assets.Scripts.Enums;
using Assets.Scripts.Dtos.GameMoveResponses;
using System;
using System.Linq;
using System.Collections;
using System.Text;

namespace Assets.Scripts.DevCards.Core
{
    public class DevCardManager : MonoBehaviour
    {
        public static DevCardManager Instance { get; private set; }

        [Header("Dev Cards")]
        public List<DevCardDto> playerCards = new List<DevCardDto>();

        [Header("Debug Settings")]
        public bool enableVerboseLogging = true;
        public bool enableEventDebugging = true;

        // Events for UI
        public event Action<List<DevCardDto>> OnCardsUpdated;
        public event Action<string> OnCardBought;
        public event Action<string> OnError;

        // Dependencies
        private DevCardService devCardService;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                DebugLog("DevCardManager instance created");
            }
            else
            {
                DebugLog("Destroying duplicate DevCardManager instance");
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            DebugLog("DevCardManager Start() called");
            ExtractUserIdFromToken();

            devCardService = GetComponent<DevCardService>();
            if (devCardService == null)
            {
                devCardService = gameObject.AddComponent<DevCardService>();
                DebugLog("Created new DevCardService component");
            }
            else
            {
                DebugLog("Found existing DevCardService component");
            }

            SubscribeToWebSocketEvents();
            AddWebSocketDebugging();
            DebugSessionInfo();

            LoadPlayerCards();
        }

        private void ExtractUserIdFromToken()
        {
            DebugLog("=== EXTRACTING USER ID FROM JWT TOKEN ===");

            string token = LocalStorageService.GetString("token");
            if (string.IsNullOrEmpty(token))
            {
                Debug.LogError("❌ No token found in LocalStorage!");
                return;
            }

            try
            {
                // Remove 'Bearer ' prefix if present
                string cleanToken = token;
                if (token.StartsWith("Bearer "))
                {
                    cleanToken = token.Substring(7);
                    DebugLog("✅ Removed 'Bearer ' prefix from token");
                }

                string[] tokenParts = cleanToken.Split('.');
                if (tokenParts.Length < 2)
                {
                    Debug.LogError("❌ Invalid JWT token format - not enough parts");
                    return;
                }

                DebugLog($"✅ Token split into {tokenParts.Length} parts");

                string payload = tokenParts[1];
                DebugLog($"✅ Extracted payload: {payload.Substring(0, Math.Min(20, payload.Length))}...");

                while (payload.Length % 4 != 0)
                {
                    payload += "=";
                }
                DebugLog($"✅ Added padding, final length: {payload.Length}");

                byte[] jsonBytes = Convert.FromBase64String(payload);
                string jsonString = Encoding.UTF8.GetString(jsonBytes);

                DebugLog($"✅ Decoded JWT payload: {jsonString}");

                var tokenData = JsonUtility.FromJson<TokenPayload>(jsonString);

                if (tokenData == null || tokenData.id <= 0)
                {
                    Debug.LogError("❌ Failed to parse token data or invalid user ID");
                    return;
                }

                DebugLog($"✅ Successfully extracted User ID: {tokenData.id}");

                // Store as string in PlayerPrefs
                PlayerPrefs.SetString("userId", tokenData.id.ToString());
                PlayerPrefs.Save();
                DebugLog($"✅ User ID {tokenData.id} stored in PlayerPrefs!");

                string storedUserIdStr = PlayerPrefs.GetString("userId", "");
                if (long.TryParse(storedUserIdStr, out long storedUserId) && storedUserId == tokenData.id)
                {
                    DebugLog($"✅ VERIFICATION SUCCESS: User ID {storedUserId} confirmed in PlayerPrefs");
                }
                else
                {
                    Debug.LogError($"❌ VERIFICATION FAILED: Expected {tokenData.id}, got {storedUserIdStr}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Failed to extract User ID from JWT token: {ex.Message}");
                Debug.LogError($"❌ Stack trace: {ex.StackTrace}");
            }
        }

        private void SubscribeToWebSocketEvents()
        {
            DebugLog("Subscribing to WebSocket events...");

            try
            {
                WebSocketService.OnBuyCardResponse += HandleBuyCardResponse;
                DebugLog("✅ Subscribed to OnBuyCardResponse");

                WebSocketService.OnPrivateBuyCard += HandlePrivateBuyCard;
                DebugLog("✅ Subscribed to OnPrivateBuyCard");

                WebSocketService.OnPlayCardResponse += HandlePlayCardResponse;
                DebugLog("✅ Subscribed to OnPlayCardResponse");
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Failed to subscribe to WebSocket events: {ex.Message}");
            }
        }

        private void AddWebSocketDebugging()
        {
            if (!enableEventDebugging) return;

            DebugLog("Adding WebSocket debugging...");

            WebSocketService.OnChatMessageReceived += (msg) => {
                DebugLog($"[WEBSOCKET] Chat message: {msg.text}");
            };

            WebSocketService.OnBuyCardResponse += (response) => {
                DebugLog($"[WEBSOCKET] OnBuyCardResponse fired: {response.username} (cards: {response.numberOfCards})");
            };

            WebSocketService.OnPrivateBuyCard += (card) => {
                DebugLog($"[WEBSOCKET] OnPrivateBuyCard fired: {card.devCardType} (ID: {card.cardId})");
            };

            WebSocketService.OnPlayCardResponse += (response) => {
                DebugLog($"[WEBSOCKET] OnPlayCardResponse fired: {response.devCardType}");
            };
        }

        private void DebugSessionInfo()
        {
            string sessionCode = LocalStorageService.GetString("session-code");
            string userIdStr = PlayerPrefs.GetString("userId", "");
            long userId = -1;
            if (!string.IsNullOrEmpty(userIdStr) && long.TryParse(userIdStr, out long parsedUserId))
            {
                userId = parsedUserId;
            }

            string token = LocalStorageService.GetString("token");
            bool wsConnected = WebSocketService.Connected;

            DebugLog($"[SESSION INFO]");
            DebugLog($"  Session Code: {sessionCode ?? "NULL"}");
            DebugLog($"  User ID: {userId}");
            DebugLog($"  Token: {(string.IsNullOrEmpty(token) ? "MISSING" : "Present")}");
            DebugLog($"  WebSocket Connected: {wsConnected}");

            if (userId == -1)
            {
                Debug.LogError("❌ USER ID IS STILL -1! JWT extraction may have failed!");
            }
            else
            {
                DebugLog($"✅ User ID looks valid: {userId}");
            }
        }

        public async void BuyDevCard()
        {
            DebugLog("=== BUY DEV CARD ATTEMPT ===");
            DebugLog("BuyDevCard() method called");

            if (!WebSocketService.Connected)
            {
                Debug.LogError("❌ WebSocket not connected!");
                OnError?.Invoke("Not connected to game session");
                return;
            }
            DebugLog("✅ WebSocket connection verified");

            string sessionCode = LocalStorageService.GetString("session-code");
            if (string.IsNullOrEmpty(sessionCode))
            {
                Debug.LogError("❌ No session code found!");
                OnError?.Invoke("No active game session");
                return;
            }
            DebugLog($"✅ Session code verified: {sessionCode}");

            string userIdStr = PlayerPrefs.GetString("userId", "");
            long userId = -1;
            if (!string.IsNullOrEmpty(userIdStr) && long.TryParse(userIdStr, out long parsedUserId))
            {
                userId = parsedUserId;
            }

            if (userId == -1)
            {
                Debug.LogError("❌ Invalid User ID (-1)! Trying to re-extract from token...");
                ExtractUserIdFromToken();

                userIdStr = PlayerPrefs.GetString("userId", "");
                if (!string.IsNullOrEmpty(userIdStr) && long.TryParse(userIdStr, out parsedUserId))
                {
                    userId = parsedUserId;
                }

                if (userId == -1)
                {
                    Debug.LogError("❌ Still invalid User ID after re-extraction!");
                    OnError?.Invoke("Invalid user authentication");
                    return;
                }
            }
            DebugLog($"✅ User ID verified: {userId}");

            try
            {
                var gameMove = new GameMoveDto(GameMoveType.BUY_CARD);
                DebugLog($"✅ Created GameMoveDto: {Newtonsoft.Json.JsonConvert.SerializeObject(gameMove)}");

                DebugLog("📤 Sending BUY_CARD via SendGameMove");
                await WebSocketService.SendGameMove(gameMove);
                DebugLog("✅ Buy dev card request sent successfully");

                Invoke(nameof(CheckForBuyCardResponse), 2f);
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Failed to send buy dev card: {ex.Message}");
                Debug.LogError($"❌ Stack trace: {ex.StackTrace}");
                OnError?.Invoke("Failed to send buy request: " + ex.Message);
            }
        }

        private void CheckForBuyCardResponse()
        {
            DebugLog("⏱️ Checking for buy card response after 2 seconds...");
            DebugLog("If no response was received, the issue might be:");
            DebugLog("  1. Backend validation failing (not your turn)");
            DebugLog("  2. Game phase restrictions");
            DebugLog("  3. Backend not processing requests");
            DebugLog("  4. WebSocket events not firing");
        }

        public async void PlayDevCard(DevCardDto card, DevCardType type)
        {
            DebugLog($"=== PLAY DEV CARD ATTEMPT ===");
            DebugLog($"Playing dev card: {type} (ID: {card.id})");

            if (card == null)
            {
                Debug.LogError("❌ Cannot play null card");
                return;
            }

            if (!card.playable)
            {
                Debug.LogWarning($"⚠️ Card {type} is not playable (playable: {card.playable}, used: {card.used})");
            }

            try
            {
                var playCardDto = new PlayCardDto(type);
                var gameMove = new GameMoveDto(playCardDto);

                DebugLog($"📤 Sending PLAY_CARD via SendGameMove: {type}");
                await WebSocketService.SendGameMove(gameMove);
                DebugLog($"✅ Play card request sent successfully: {type}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Failed to play card: {ex.Message}");
                OnError?.Invoke($"Failed to play {type} card");
            }
        }

        private void HandleBuyCardResponse(BuyCardResponseDto response)
        {
            DebugLog($"🎉 BUY_CARD response received!");
            DebugLog($"  Player: {response.username}");
            DebugLog($"  Total cards: {response.numberOfCards}");
            LoadPlayerCards();
        }

        private void HandlePrivateBuyCard(PrivateBuyCard privateBuyCard)
        {
            DebugLog($"🎉 PRIVATE_BUY_CARD response received!");
            DebugLog($"  Card Type: {privateBuyCard.devCardType}");
            DebugLog($"  Card ID: {privateBuyCard.cardId}");

            var newCard = new DevCardDto
            {
                id = privateBuyCard.cardId,
                type = privateBuyCard.devCardType,
                playable = false,
                used = false
            };

            playerCards.Add(newCard);
            DebugLog($"✅ Added card to local collection. Total cards: {playerCards.Count}");

            OnCardsUpdated?.Invoke(playerCards);
            OnCardBought?.Invoke($"You received a {privateBuyCard.devCardType} card!");

            DebugLog($"🎯 UI updated with new dev card: {privateBuyCard.devCardType} (ID: {privateBuyCard.cardId})");
        }

        private void HandlePlayCardResponse(PlayCardResponseDto response)
        {
            DebugLog($"🎉 PLAY_CARD response received!");
            DebugLog($"  Card Type: {response.devCardType}");

            var playedCard = playerCards.FirstOrDefault(c => !c.used && c.type == response.devCardType);
            if (playedCard != null)
            {
                playedCard.used = true;
                OnCardsUpdated?.Invoke(playerCards);
                DebugLog($"✅ Marked card as used: {response.devCardType}");
            }
            else
            {
                DebugLog($"⚠️ Could not find matching card to mark as used: {response.devCardType}");
            }
        }

        private void LoadPlayerCards()
        {
            string userIdStr = PlayerPrefs.GetString("userId", "");
            long playerId = 1;

            if (!string.IsNullOrEmpty(userIdStr) && long.TryParse(userIdStr, out long parsedUserId))
            {
                playerId = parsedUserId;
            }
            else
            {
                DebugLog("⚠️ No valid User ID found in PlayerPrefs, using default: 1");
            }

            DebugLog($"Loading dev cards for player ID: {playerId}");

            StartCoroutine(devCardService.List(playerId,
                cards => {
                    playerCards = cards;
                    OnCardsUpdated?.Invoke(playerCards);
                    DebugLog($"✅ Loaded {cards.Count} dev cards from backend");

                    if (enableVerboseLogging)
                    {
                        foreach (var card in cards)
                        {
                            DebugLog($"  - {card.type} (ID: {card.id}, playable: {card.playable}, used: {card.used})");
                        }
                    }
                },
                error => {
                    Debug.LogError($"❌ Failed to load dev cards: {error}");
                    OnError?.Invoke(error);
                }
            ));
        }

        public List<DevCardDto> GetPlayerCards()
        {
            return new List<DevCardDto>(playerCards);
        }

        [ContextMenu("Debug Current State")]
        public void DebugCurrentState()
        {
            DebugLog("=== CURRENT DEV CARD MANAGER STATE ===");
            DebugLog($"Player Cards Count: {playerCards.Count}");
            DebugLog($"WebSocket Connected: {WebSocketService.Connected}");
            DebugSessionInfo();
        }

        [ContextMenu("Re-extract User ID from Token")]
        public void ReextractUserIdFromToken()
        {
            ExtractUserIdFromToken();
            DebugSessionInfo();
        }

        private void DebugLog(string message)
        {
            if (enableVerboseLogging)
            {
                Debug.Log($"[DevCardManager] {message}");
            }
        }

        private void OnDestroy()
        {
            DebugLog("DevCardManager destroyed - unsubscribing from events");
            WebSocketService.OnBuyCardResponse -= HandleBuyCardResponse;
            WebSocketService.OnPrivateBuyCard -= HandlePrivateBuyCard;
            WebSocketService.OnPlayCardResponse -= HandlePlayCardResponse;
        }

        [System.Serializable]
        public class TokenPayload
        {
            public string sub;
            public long id;
            public string jti;
            public long iat;
            public long exp;
        }
    }
}
